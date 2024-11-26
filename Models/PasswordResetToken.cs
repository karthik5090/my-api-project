public class PasswordResetToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }  // This property should match the column in the DB
}
