namespace ERP.Application.Common.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalEmployees { get; set; }
    public int TotalInventoryItems { get; set; }
    public int TotalPurchaseOrders { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalProjects { get; set; }
    public int TotalAccounts { get; set; }
    public List<ActivityDto> RecentActivities { get; set; } = new();
}

public class ActivityDto
{
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
