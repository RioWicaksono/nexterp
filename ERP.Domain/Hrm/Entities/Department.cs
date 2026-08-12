using ERP.Domain.Common;

namespace ERP.Domain.Hrm.Entities;

/// <summary>
/// Department entity
/// </summary>
public class Department : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? Description { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly Department? _parentDepartment;
    public Department? ParentDepartment => _parentDepartment;

    private readonly List<Department> _childDepartments = new();
    public IReadOnlyCollection<Department> ChildDepartments => _childDepartments.AsReadOnly();

    private readonly List<Employee> _employees = new();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Factory method
    public static Department Create(
        Guid organizationId,
        string name,
        string? code = null,
        string? description = null,
        Guid? parentDepartmentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required", nameof(name));

        return new Department
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Code = code?.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            ParentDepartmentId = parentDepartmentId
        };
    }

    public void Update(string? name = null, string? code = null, string? description = null)
    {
        Name = name?.Trim() ?? Name;
        Code = code?.Trim().ToUpperInvariant() ?? Code;
        Description = description?.Trim() ?? Description;
        UpdateTimestamp();
    }

    public void SetParentDepartment(Guid? parentId)
    {
        ParentDepartmentId = parentId;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}

/// <summary>
/// Position/Job Title entity
/// </summary>
public class Position : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Grade { get; private set; } = 1;
    public decimal? MinSalary { get; private set; }
    public decimal? MaxSalary { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    private readonly Department? _department;
    public Department? Department => _department;

    private readonly List<Employee> _employees = new();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Factory method
    public static Position Create(
        Guid organizationId,
        Guid departmentId,
        string title,
        string? description = null,
        int grade = 1,
        decimal? minSalary = null,
        decimal? maxSalary = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Position title is required", nameof(title));

        if (grade < 1)
            throw new ArgumentException("Grade must be at least 1", nameof(grade));

        return new Position
        {
            OrganizationId = organizationId,
            DepartmentId = departmentId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Grade = grade,
            MinSalary = minSalary,
            MaxSalary = maxSalary
        };
    }

    public void Update(string? title = null, string? description = null, int? grade = null)
    {
        Title = title?.Trim() ?? Title;
        Description = description?.Trim() ?? Description;
        Grade = grade ?? Grade;
        UpdateTimestamp();
    }

    public void SetSalaryRange(decimal? min, decimal? max)
    {
        if (min.HasValue && max.HasValue && min > max)
            throw new ArgumentException("Minimum salary cannot exceed maximum salary");

        MinSalary = min;
        MaxSalary = max;
        UpdateTimestamp();
    }

    public void Activate() { IsActive = true; UpdateTimestamp(); }
    public void Deactivate() { IsActive = false; UpdateTimestamp(); }
}
