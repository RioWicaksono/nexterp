namespace ERP.Application.Common.Behaviors;

/// <summary>
/// Attribute to mark Commands or Queries that require a specific module to be active.
/// When applied, the ModuleAuthorizationBehavior will check if the user's organization
/// has access to the specified module before executing the handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RequiresModuleAttribute : Attribute
{
    /// <summary>
    /// The module code that is required (e.g., "HRM", "INVENTORY", "ACCOUNTING").
    /// </summary>
    public string ModuleCode { get; }

    /// <summary>
    /// Optional error message when access is denied.
    /// If not specified, a default message will be used.
    /// </summary>
    public string? ErrorMessage { get; init; }

    public RequiresModuleAttribute(string moduleCode)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
            throw new ArgumentException("Module code cannot be empty", nameof(moduleCode));

        ModuleCode = moduleCode.ToUpperInvariant();
    }
}
