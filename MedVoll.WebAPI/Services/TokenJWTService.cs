using MedVoll.WebAPI.Dtos;
using MedVoll.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MedVoll.WebAPI.Services
{
    public class TokenJWTService
    {
        private readonly IConfiguration configuration;

        public TokenJWTService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<UsuarioTokenDto> GerarTokenDeUsuarioAsync(UsuarioDto usuarioDto, VollMedUser usuario, UserManager<VollMedUser> userManager)
        {
            // Definimos uma lista de Claims, que são informações do usuário e que queremos que estejam no token
            var claims = new List<Claim>
            {
         new Claim("Alura","C#"),
         new Claim(JwtRegisteredClaimNames.UniqueName, usuarioDto.Email!),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
     };

            var roles = await userManager.GetRolesAsync(usuario);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Definimos a chave de acesso ao token.O valor da chave é obtido da configuração JWTKey:key, convertida para um array de bytes via Encoding.UTF8.GetBytes.
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey:key"]!));

            // Definimos as credenciais do token - chave, algoritmo de segurança e tipo de criptografia.
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            //Definimos o tempo de expiração do token.
            var expiracao = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWTTokenConfiguration:ExpireInMinutes"]!));

            // Definimos a descrição do token.
            JwtSecurityToken? token = null;
            try
            {
                token = new JwtSecurityToken(
                 issuer: configuration["JWTTokenConfiguration:Issuer"], //Quem emitiu o token
                 audience: configuration["JWTTokenConfiguration:Audience"],//Para quem é dedicado o token
                 claims: claims,
                 expires: expiracao,
                 signingCredentials: credenciais
             );

            }
            catch (Exception exc)
            {
                throw new ArgumentException("Encontrado erro ao gerar Token!", exc);
            }

            return new UsuarioTokenDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracao = expiracao,
                Autenticado = true
            };
        }

        public string GerarRefreshToken()
        {
            var bytes = new byte[128];
            using var numeroRandomico = RandomNumberGenerator.Create();
            numeroRandomico.GetBytes(bytes);
            var refreshToken = Convert.ToBase64String(bytes);
            return refreshToken;
        }

        internal ClaimsPrincipal CapturaClaimsDoTokenExpirado(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("O token não pode ser nulo ou vazio.", nameof(token));

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey:key"]!));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Ignora a validação do tempo de expiração
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWTTokenConfiguration:Issuer"],
                ValidAudience = configuration["JWTTokenConfiguration:Audience"],
                IssuerSigningKey = chave
            };

            var tokenHandlerValidator = new JwtSecurityTokenHandler();

            var principal = tokenHandlerValidator.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("O token é inválido ou não utiliza o algoritmo esperado.");
            }
            return principal;
        }


    }
}
