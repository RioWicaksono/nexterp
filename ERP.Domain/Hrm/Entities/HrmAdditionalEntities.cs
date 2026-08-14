using ERP.Domain.Common;
using ERP.Domain.Common.Modules;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Employee document entity for storing legal and professional documents.
/// Indonesian regulations require: KTP, KK, NPWP, BPJS, diploma, etc.
/// </summary>
public class EmployeeDocument : BaseEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string? FileUrl { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? IssuedBy { get; private set; }
    public bool IsVerified { get; private set; }
    public Guid? VerifiedBy { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? Notes { get; private set; }

    // Common document types
    public static class DocumentTypes
    {
        public const string KTP = "KTP";                    // Indonesian ID Card
        public const string KK = "KK";                      // Family Card
        public const string NPWP = "NPWP";                  // Tax ID
        public const string BPJSTK = "BPJSTK";              // Employment Insurance
        public const string BPJSKS = "BPJSKS";              // Health Insurance
        public const string DIPLOMA = "DIPLOMA";            // Education Certificate
        public const string TRANSCRIPT = "TRANSCRIPT";      // Academic Transcript
        public const string CV = "CV";                      // Curriculum Vitae
        public const string PHOTO = "PHOTO";                // Passport Photo
        public const string PASSPORT = "PASSPORT";          // Passport
        public const string BOOK_ACCOUNT = "BOOK_ACCOUNT"; // Bank Book
        public const string CONTRACT = "CONTRACT";          // Employment Contract
        public const string WARNING_LETTER = "WARNING_LETTER"; // Surat Peringatan
        public const string OTHER = "OTHER";
    }

    // Navigation
    private readonly Employee? _employee;
    public Employee? Employee => _employee;

    private EmployeeDocument() { }

    public static EmployeeDocument Create(
        Guid organizationId,
        Guid employeeId,
        string documentType,
        string fileName,
        string filePath,
        DateTime issueDate,
        DateTime? expiryDate = null,
        string? documentNumber = null,
        string? issuedBy = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type is required", nameof(documentType));

        return new EmployeeDocument
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            DocumentType = documentType.ToUpperInvariant(),
            FileName = fileName,
            FilePath = filePath,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            DocumentNumber = documentNumber?.Trim(),
            IssuedBy = issuedBy?.Trim(),
            Notes = notes?.Trim()
        };
    }

    public void Verify(Guid verifiedBy)
    {
        if (IsVerified)
            throw new InvalidOperationException("Document is already verified");

        IsVerified = true;
        VerifiedBy = verifiedBy;
        VerifiedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void SetFileUrl(string url)
    {
        FileUrl = url;
        UpdateTimestamp();
    }

    public void UpdateExpiry(DateTime? expiryDate)
    {
        ExpiryDate = expiryDate;
        UpdateTimestamp();
    }

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public bool IsExpiringSoon => ExpiryDate.HasValue &&
        ExpiryDate.Value > DateTime.UtcNow &&
        ExpiryDate.Value < DateTime.UtcNow.AddDays(30);
}

/// <summary>
/// Work shift entity for managing different work schedules.
/// Indonesian companies often have: pagi, siang, malam shifts.
/// </summary>
public class Shift : BaseEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public TimeSpan? BreakStart { get; private set; }
    public TimeSpan? BreakEnd { get; private set; }
    public int WorkHours => (int)(EndTime - StartTime - (BreakEnd - BreakStart).GetValueOrDefault()).TotalHours;
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }
    public bool IsNightShift { get; private set; }

    private Shift() { }

    public static Shift Create(
        Guid organizationId,
        string name,
        string code,
        TimeSpan startTime,
        TimeSpan endTime,
        TimeSpan? breakStart = null,
        TimeSpan? breakEnd = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shift name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Shift code is required", nameof(code));

        return new Shift
        {
            OrganizationId = organizationId,
            Name = name,
            Code = code.ToUpperInvariant(),
            StartTime = startTime,
            EndTime = endTime,
            BreakStart = breakStart,
            BreakEnd = breakEnd,
            Description = description?.Trim(),
            IsNightShift = startTime.Hours > 20 || endTime.Hours < 6
        };
    }

    public void Update(TimeSpan? startTime = null, TimeSpan? endTime = null,
        TimeSpan? breakStart = null, TimeSpan? breakEnd = null, string? description = null)
    {
        StartTime = startTime ?? StartTime;
        EndTime = endTime ?? EndTime;
        BreakStart = breakStart ?? BreakStart;
        BreakEnd = breakEnd ?? BreakEnd;
        Description = description ?? Description;
        IsNightShift = StartTime.Hours > 20 || EndTime.Hours < 6;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}

/// <summary>
/// Holiday entity for managing Indonesian national and regional holidays.
/// </summary>
public class Holiday : BaseEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public int Year { get; private set; }
    public string HolidayType { get; private set; } = string.Empty;
    public bool IsRecurring { get; private set; }
    public bool IsOptional { get; private set; }
    public string? Region { get; private set; }

    // Common holiday types
    public static class HolidayTypes
    {
        public const string NATIONAL = "NATIONAL";           // Libur Nasional
        public const string REGIONAL = "REGIONAL";           // Libur Provinsi
        public const string COMPANY = "COMPANY";            // Libur Perusahaan
        public const string RELIGIOUS = "RELIGIOUS";       // Hari Raya Agama
        public const string NATIONAL_HOLIDAY = "NATIONAL_HOLIDAY";
    }

    private Holiday() { }

    public static Holiday Create(
        Guid organizationId,
        string name,
        DateTime date,
        string holidayType,
        bool isRecurring = true,
        bool isOptional = false,
        string? region = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Holiday name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(holidayType))
            throw new ArgumentException("Holiday type is required", nameof(holidayType));

        return new Holiday
        {
            OrganizationId = organizationId,
            Name = name,
            Date = date.Date,
            Year = date.Year,
            HolidayType = holidayType.ToUpperInvariant(),
            IsRecurring = isRecurring,
            IsOptional = isOptional,
            Region = region?.Trim()
        };
    }

    public void Update(string? name = null, DateTime? date = null, string? holidayType = null)
    {
        Name = name?.Trim() ?? Name;
        Date = date ?? Date;
        HolidayType = holidayType?.ToUpperInvariant() ?? HolidayType;
        Year = Date.Year;
        UpdateTimestamp();
    }
}

/// <summary>
/// Leave entitlement entity for tracking leave allocations based on employment type and years of service.
/// Indonesian regulations: min 12 days annual leave for workers with >= 1 year service.
/// </summary>
public class LeaveEntitlement : BaseEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string LeaveType { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public decimal TotalDays { get; private set; }
    public decimal CarryForwardDays { get; private set; }
    public decimal UsedDays { get; private set; }
    public decimal PendingDays { get; private set; }
    public decimal AvailableDays => TotalDays + CarryForwardDays - UsedDays - PendingDays;
    public bool IsAutoAllocated { get; private set; }
    public DateTime? AllocationDate { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    private readonly Employee? _employee;
    public Employee? Employee => _employee;

    private LeaveEntitlement() { }

    public static LeaveEntitlement Create(
        Guid organizationId,
        Guid employeeId,
        string leaveType,
        int year,
        decimal totalDays,
        bool isAutoAllocated = false,
        string? notes = null)
    {
        return new LeaveEntitlement
        {
            OrganizationId = organizationId,
            EmployeeId = employeeId,
            LeaveType = leaveType.ToUpperInvariant(),
            Year = year,
            TotalDays = totalDays,
            IsAutoAllocated = isAutoAllocated,
            AllocationDate = DateTime.UtcNow,
            Notes = notes?.Trim()
        };
    }

    /// <summary>
    /// Calculate annual leave entitlement based on years of service (Indonesian UU Ketenagakerjaan).
    /// Year 1-4: 12 days, Year 5+: increases by 1 day per year, max 21 days
    /// </summary>
    public static decimal CalculateAnnualLeaveDays(int yearsOfService, decimal baseDays = 12)
    {
        if (yearsOfService < 1) return 0;
        if (yearsOfService <= 4) return baseDays;

        // Year 5+: +1 day per year, max 21 days
        return Math.Min(baseDays + (yearsOfService - 4), 21);
    }

    public void AddDays(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        TotalDays += days;
        UpdateTimestamp();
    }

    public void UseDays(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        if (days > AvailableDays)
            throw new InvalidOperationException($"Insufficient leave balance. Available: {AvailableDays}, Requested: {days}");

        UsedDays += days;
        UpdateTimestamp();
    }

    public void SetPending(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));

        PendingDays = days;
        UpdateTimestamp();
    }

    public void AdjustCarryForward(decimal days)
    {
        if (days < 0)
            throw new ArgumentException("Carry forward days cannot be negative", nameof(days));

        CarryForwardDays = days;
        UpdateTimestamp();
    }

    public void ResetYear(int newYear)
    {
        // Carry forward unused days (up to 50% of total)
        var maxCarryForward = TotalDays / 2;
        CarryForwardDays = Math.Min(Math.Max(0, AvailableDays), maxCarryForward);

        UsedDays = 0;
        PendingDays = 0;
        Year = newYear;
        AllocationDate = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
