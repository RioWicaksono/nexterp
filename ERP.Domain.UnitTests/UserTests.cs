using Xunit;
using ERP.Domain.Base;

namespace ERP.Domain.UnitTests;

/// <summary>
/// Unit tests for User entity
/// </summary>
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var passwordHash = "hashedpassword";
        var firstName = "Test";
        var lastName = "User";

        // Act
        var user = User.Create(orgId, username, email, passwordHash, firstName, lastName);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(orgId, user.OrganizationId);
        Assert.Equal(username.ToLowerInvariant(), user.Username);
        Assert.Equal(email.ToLowerInvariant(), user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.True(user.IsActive);
        Assert.False(user.IsSuperAdmin);
        Assert.False(user.IsDeleted);
    }

    [Fact]
    public void Create_WithValidData_ShouldSetCorrectFullName()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act
        var userWithLastName = User.Create(orgId, "user1", "user1@test.com", "hash", "John", "Doe");
        var userWithoutLastName = User.Create(orgId, "user2", "user2@test.com", "hash", "Jane");

        // Assert
        Assert.Equal("John Doe", userWithLastName.FullName);
        Assert.Equal("Jane", userWithoutLastName.FullName);
    }

    [Theory]
    [InlineData("", "Email is required")]
    [InlineData("   ", "Email is required")]
    public void Create_WithInvalidEmail_ShouldThrowException(string email, string expectedMessage)
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(orgId, "user", email, "hash", "Test"));
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("", "Username is required")]
    [InlineData("   ", "Username is required")]
    public void Create_WithInvalidUsername_ShouldThrowException(string username, string expectedMessage)
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            User.Create(orgId, username, "test@test.com", "hash", "Test"));
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void Create_WithInvalidFirstName_ShouldThrowException()
    {
        // Arrange
        var orgId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            User.Create(orgId, "user", "test@test.com", "hash", ""));
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateFields()
    {
        // Arrange
        var user = CreateTestUser();
        var newFirstName = "Updated";
        var newLastName = "Name";
        var newPhone = "1234567890";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newPhone);

        // Assert
        Assert.Equal(newFirstName, user.FirstName);
        Assert.Equal(newLastName, user.LastName);
        Assert.Equal(newPhone, user.Phone);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        var initialAttempts = user.FailedLoginAttempts;

        // Act
        user.RecordFailedLogin();

        // Assert
        Assert.Equal(initialAttempts + 1, user.FailedLoginAttempts);
    }

    [Fact]
    public void RecordFailedLogin_ShouldLockAfterFiveAttempts()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Assert
        Assert.True(user.IsLocked);
        Assert.NotNull(user.LockedUntil);
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldResetFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Act
        user.RecordSuccessfulLogin("127.0.0.1");

        // Assert
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.NotNull(user.LastLoginAt);
        Assert.Equal("127.0.0.1", user.LastLoginIp);
    }

    [Fact]
    public void SetRefreshToken_ShouldSetTokenAndExpiry()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "test-refresh-token";
        var expiry = TimeSpan.FromDays(7);

        // Act
        user.SetRefreshToken(token, expiry);

        // Assert
        Assert.Equal(token, user.RefreshToken);
        Assert.NotNull(user.RefreshTokenExpiry);
        Assert.True(user.RefreshTokenExpiry > DateTime.UtcNow);
    }

    [Fact]
    public void ValidateRefreshToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = CreateTestUser();
        var token = "valid-refresh-token";
        user.SetRefreshToken(token, TimeSpan.FromDays(7));

        // Act
        var isValid = user.ValidateRefreshToken(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateRefreshToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.SetRefreshToken("valid-token", TimeSpan.FromDays(7));

        // Act
        var isValid = user.ValidateRefreshToken("invalid-token");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.MarkAsDeleted();

        // Assert
        Assert.True(user.IsDeleted);
    }

    [Fact]
    public void Restore_ShouldSetIsDeletedToFalse()
    {
        // Arrange
        var user = CreateTestUser();
        user.MarkAsDeleted();

        // Act
        user.Restore();

        // Assert
        Assert.False(user.IsDeleted);
    }

    private static User CreateTestUser()
    {
        return User.Create(
            Guid.NewGuid(),
            "testuser",
            "test@example.com",
            "hashedpassword",
            "Test",
            "User");
    }
}
