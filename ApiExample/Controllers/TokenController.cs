using ApiExample.Requests;
using ApiExample.Responses;
using ApiHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiExample.Controllers
{
    public class TokenController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Get(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(Result.Fail("Please fill user id or password."));

            if (request.UserId.ToLower() != "admin" && request.Password.ToLower() != "admin")
                return NotFound(Result.Fail("Invalid credentials."));

            var response = new LoginResponse
            {
                Token = GenerateJwt(request.UserId.ToLower()),
                RefreshToken = GenerateRefreshToken()
            };

            return Ok(Result<LoginResponse>.Success(response));
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenRequest request)
        {
            var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
            var userId = FindFirstValue(userPrincipal, ClaimTypes.NameIdentifier);
            if (userId != "admin")
                return NotFound(Result.Fail("Invalid credentials."));

            //TODO validation refresh token from db

            var response = new LoginResponse
            {
                Token = GenerateJwt(userId),
                RefreshToken = GenerateRefreshToken()
            };

            return Ok(Result<LoginResponse>.Success(response));
        }

        private string GenerateJwt(string userId)
        {
            //get signing credentials
            var appSettings = _configuration.GetSection("appSettings");
            var secret = appSettings["Secret"];
            var key = Encoding.ASCII.GetBytes(secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            //get claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),
               signingCredentials: signingCredentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.WriteToken(token);

            return encryptedToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        #region Refresh Token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var appSettings = _configuration.GetSection("appSettings");
            var secret = appSettings["Secret"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private string FindFirstValue(ClaimsPrincipal principal, string claimType)
        {
            var claim = principal.FindFirst(claimType);

            return claim != null ? claim.Value : null;
        }
        #endregion
    }
}
