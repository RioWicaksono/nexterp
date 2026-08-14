using System.Security.Cryptography;
using System.Text;
using ERP.Domain.Common;

namespace ERP.Domain.Base;

/// <summary>
/// User entity with authentication and authorization support
/// </summary>
public class User : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsSuperAdmin { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Refresh token stored as SHA-256 hash for security
    // Plain token is never stored - only its hash
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    // Navigation properties
    private readonly Organization? _organization;
    public Organization? Organization => _organization;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Constants for security settings
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 30;

    // Full name property
    public string FullName => string.IsNullOrEmpty(LastName)
        ? FirstName
        : $"{FirstName} {LastName}";

    // Factory method
    public static User Create(
        Guid organizationId,
        string username,
        string email,
        string passwordHash,
        string firstName,
        string? lastName = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        return new User
        {
            OrganizationId = organizationId,
            Username = username.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName?.Trim(),
            Phone = phone?.Trim()
        };
    }

    public void SetOrganization(Guid organizationId)
    {
        OrganizationId = organizationId;
        UpdateTimestamp();
    }

    public void UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        string? phone = null)
    {
        FirstName = firstName?.Trim() ?? FirstName;
        LastName = lastName?.Trim() ?? LastName;
        Phone = phone?.Trim() ?? Phone;
        UpdateTimestamp();
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdateTimestamp();
    }

    public void SetAsSuperAdmin()
    {
        IsSuperAdmin = true;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }

    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
        }
        UpdateTimestamp();
    }

    public void RecordSuccessfulLogin(string? ipAddress = null)
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        UpdateTimestamp();
    }

    /// <summary>
    /// Sets refresh token by storing its SHA-256 hash.
    /// Plain token is never persisted - only a hash for comparison.
    /// </summary>
    public void SetRefreshToken(string token, TimeSpan expiry)
    {
        // Hash the token using SHA-256
        RefreshTokenHash = ComputeTokenHash(token);
        RefreshTokenExpiry = DateTime.UtcNow.Add(expiry);
        UpdateTimestamp();
    }

    public void ClearRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiry = null;
        UpdateTimestamp();
    }

    /// <summary>
    /// Validates refresh token using constant-time comparison to prevent timing attacks.
    /// </summary>
    public bool ValidateRefreshToken(string token)
    {
        // First check expiry (fast check)
        if (!RefreshTokenExpiry.HasValue || RefreshTokenExpiry.Value <= DateTime.UtcNow)
            return false;

        // If no token hash stored, return false
        if (string.IsNullOrEmpty(RefreshTokenHash))
            return false;

        // If no token provided, return false
        if (string.IsNullOrEmpty(token))
            return false;

        // Compute hash of provided token and compare using constant-time comparison
        var providedHash = ComputeTokenHash(token);
        return ConstantTimeEquals(RefreshTokenHash, providedHash);
    }

    /// <summary>
    /// Computes SHA-256 hash of the token.
    /// </summary>
    private static string ComputeTokenHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }

    public void AssignRole(UserRole userRole)
    {
        if (!_userRoles.Any(ur => ur.RoleId == userRole.RoleId))
        {
            _userRoles.Add(userRole);
            UpdateTimestamp();
        }
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            UpdateTimestamp();
        }
    }

    public bool HasPermission(string permission)
    {
        if (IsSuperAdmin) return true;
        return _userRoles.Any(ur => ur.Role?.HasPermission(permission) == true);
    }
}
