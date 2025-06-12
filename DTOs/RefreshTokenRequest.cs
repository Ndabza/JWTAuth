namespace JWTAuth;

public class RefreshTokenRequest
{
    public string? Id { get; set; }
    public required string RefreshToken { get; set; }
}
