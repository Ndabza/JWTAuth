using JWTAuth.DTOs;
using JWTAuth.Models;

namespace JWTAuth.Interfaces;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserResponse request);
    Task<TokenResponse?> LoginAsync(UserResponse request);
    Task<TokenResponse?> RequestRefreshTokenAsync(RefreshTokenRequest request);
}
