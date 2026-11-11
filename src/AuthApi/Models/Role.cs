namespace AuthApi.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }

    public List<User> Users { get; set; } = new();
}