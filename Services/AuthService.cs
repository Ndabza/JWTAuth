using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JWTAuth.DTOs;
using JWTAuth.Interfaces;
using JWTAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuth.Services;

public class AuthService : IAuthService
{
    private AuthDbContext _context;
    private IConfiguration _configuration;

    public AuthService(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<TokenResponse?> LoginAsync(UserResponse request)
    {
        var user = await _context.User.FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null)
            return null;

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, request.Password!) == PasswordVerificationResult.Failed)
            return null;

        var responseData = new TokenResponse
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshToken(user)
        };

        return responseData;
    }

    public async Task<TokenResponse?> RequestRefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await VerifyRefreshToken(request.Id!, request.RefreshToken);

        if(user is null)
            return null;

        var responseData = new TokenResponse
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshToken(user)
        };

        return responseData;
    }

    public async Task<User?> RegisterAsync(UserResponse request)
    {
        if (await _context.User.AnyAsync(x => x.Username == request.Username))
            return null;

        var user = new User();

        var hash = new PasswordHasher<User>().HashPassword(user, request.Password!);

        user.Username = request.Username;
        user.PasswordHash = hash;
        user.Role = "Student";

        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;

    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>{
                new Claim(ClaimTypes.Name,user.Username!),
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Role, user.Role!)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshToken(User user)
    {
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenLifeTime = DateTime.UtcNow.AddDays(5);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private async Task<User?> VerifyRefreshToken(string Id, string RefreshToken)
    {
        var user = await _context.User.FindAsync(Id);

        if (user is null || user?.RefreshToken != RefreshToken || user.RefreshTokenLifeTime <= DateTime.UtcNow)
            return null;

        return user;
    }

}
