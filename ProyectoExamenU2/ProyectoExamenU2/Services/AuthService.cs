using ProyectoExamenU2.Dtos.Auth;
using ProyectoExamenU2.Dtos.Common;
using ProyectoExamenU2.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;
using ProyectoExamenU2.Databases.PrincipalDataBase;
using System.Security.Cryptography;
using ProyectoExamenU2.Helpers;

namespace ProyectoExamenU2.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly ProyectoExamenU2Context _context;
        private readonly ILogger<UserEntity> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            ProyectoExamenU2Context context,
            ILogger<UserEntity> logger,
            IConfiguration configuration
            )
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._context = context;
            this._logger = logger;
            this._configuration = configuration;
        }

        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var result = await _signInManager.
                PasswordSignInAsync(dto.Email,
                                    dto.Password,
                                    isPersistent: false,
                                    lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Generación del Token
                var userEntity = await _userManager.FindByEmailAsync(dto.Email);

                // ClaimList creation
                List<Claim> authClaims = await GetClaims(userEntity);

                var jwtToken = GetToken(authClaims);

                var refreshToken = GenerateRefreshTokenString();

                userEntity.RefreshToken = refreshToken;
                userEntity.RefreshTokenExpire = DateTime.Now
                    .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

                _context.Entry(userEntity);
                await _context.SaveChangesAsync();

                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Inicio de sesion satisfactorio",
                    Data = new LoginResponseDto
                    {
                        Name = userEntity.Name,
                        Email = userEntity.Email,
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken), // convertir token en string
                        TokenExpiration = jwtToken.ValidTo,
                        RefreshToken = refreshToken
                    }
                };
            }

            return new ResponseDto<LoginResponseDto>
            {
                Status = false,
                StatusCode = 401,
                Message = "Fallo al inicio de sesión"
            };
        }


        public async Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            string email = "";

            try
            {
                var principal = GetTokenPrincipal(dto.Token);

                var emailClaim = principal.Claims.FirstOrDefault(c =>
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                var userIdCLaim = principal.Claims.Where(x => x.Type == "UserId").FirstOrDefault();
                //_logger.LogInformation($"Correo del Usuario es: {emailClaim.Value}");
                //_logger.LogInformation($"Id del Usuario es: {userIdCLaim.Value}");

                if (emailClaim is null) return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: No se encontro un correo valido.");

                email = emailClaim.Value;

                var userEntity = await _userManager.FindByEmailAsync(email);

                if (userEntity is null) return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: El usuario no existe.");

                if (userEntity.RefreshToken != dto.RefreshToken) return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: La sesión no es valida.");
        
                if (userEntity.RefreshTokenExpire < DateTime.Now) return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: La sesión ha expirado.");


                List<Claim> authClaims = await GetClaims(userEntity);

                var jwtToken = GetToken(authClaims);

                var loginResponseDto = new LoginResponseDto
                {
                    Email = email,
                    Name = userEntity.Name,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    TokenExpiration = jwtToken.ValidTo,
                    RefreshToken = GenerateRefreshTokenString()
                };

                userEntity.RefreshToken = loginResponseDto.RefreshToken;
                userEntity.RefreshTokenExpire = DateTime.Now
                    .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

                _context.Entry(userEntity);
                await _context.SaveChangesAsync();

                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Token renovado satisfactoriamente",
                    Data = loginResponseDto
                };

            }
            catch (Exception e)
            {
                _logger.LogError(exception: e, message: e.Message);
                return ResponseHelper.ResponseError<LoginResponseDto>(500, "Ocurrio un error al renovar el token");

            }

        }
        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetTokenPrincipal(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding
                .UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }

        private async Task<List<Claim>> GetClaims(UserEntity userEntity)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, userEntity.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", userEntity.Id),
                };

            var userRoles = await _userManager.GetRolesAsync(userEntity);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                     //expires: DateTime.Now.AddHours(1),
                     expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "15")),
                    claims: authClaims, signingCredentials: new SigningCredentials(
                                                                authSigninKey,
                                                                SecurityAlgorithms.HmacSha256)
             );

            var secret = _configuration["JWT:Secret"];
            var iss = _configuration["JWT:ValidIssuer"];

            return token;
        }
    }
}
