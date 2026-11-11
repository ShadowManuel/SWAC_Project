namespace AuthApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    public UserRole Role { get; set; } = UserRole.User;
    public int SupportLevel { get; set; } = 1;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
