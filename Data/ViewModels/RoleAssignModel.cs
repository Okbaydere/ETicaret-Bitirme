namespace Data.ViewModels;

public class RoleAssignModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasAssigned { get; set; }
}