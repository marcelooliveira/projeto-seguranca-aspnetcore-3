using MedVoll.Web.Dtos;
using MedVoll.WebAPI.Dtos;
using MedVoll.WebAPI.Models;
using MedVoll.WebAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<VollMedUser> userManager;
    private readonly SignInManager<VollMedUser> signInManager;
    private readonly TokenJWTService tokenJWTService;
    private readonly IConfiguration configuration;

    public AuthController(UserManager<VollMedUser> userManager, SignInManager<VollMedUser> signInManager, TokenJWTService tokenJWTService, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.tokenJWTService = tokenJWTService;
        this.configuration = configuration;
    }

    //Endpoints
    [HttpPost("registrar-usuario")]
    public async Task<IActionResult> RegistrarUsuarioAsync([FromBody] UsuarioDto usuarioDto)
    {
        var usuarioReg = await userManager.FindByEmailAsync(usuarioDto.Email!);
        if (usuarioReg is not null)
        {
            return BadRequest("Usuário já foi registrado na base de dados.");
        }

        var usuario = new VollMedUser
        {
            UserName = usuarioDto.Email,
            Email = usuarioDto.Email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(usuario, usuarioDto.Senha);
        if (!result.Succeeded)
        {
            return BadRequest($"Falha ao registrar usuário : {result.Errors}");
        }
        await signInManager.SignInAsync(usuario, isPersistent: false);

        return Ok(new
        {
            Mensagem = "Usuário registrado com sucesso",
            Token = await tokenJWTService.GerarTokenDeUsuarioAsync(usuarioDto, usuario, userManager)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UsuarioDto usuarioDto)
    {
        var usuario = await userManager.FindByEmailAsync(usuarioDto.Email!);
        if (usuario is null)
        {
            return BadRequest("usuário não encontrado.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(usuario, usuarioDto.Senha!, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return BadRequest("Falha no login do usuário.");
        }

        UsuarioTokenDto usuarioTokenDto = await tokenJWTService.GerarTokenDeUsuarioAsync(usuarioDto, usuario, userManager);
        var refreshToken = tokenJWTService.GerarRefreshToken();
        usuarioTokenDto.RefreshToken = refreshToken;

        //Adicionar o refresh token ao usuário
        usuario.RefreshToken = refreshToken;
        var expire = int.TryParse(configuration["JWTTokenConfiguration:RefreshExpireInMinutes"],
            out int refreshExpireInMinutes);
        usuario.ExpireTime = DateTime.Now.AddMinutes(refreshExpireInMinutes);
        await userManager.UpdateAsync(usuario);

        return base.Ok(new
        {
            Mensagem = "Usuário logado com sucesso",
            Token = usuarioTokenDto
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RecuperaRefreshToken(UsuarioTokenDto userToken)
    {
        // Validação to token JWT
        string? token = userToken.Token ?? throw new ArgumentException(nameof(userToken));
        string? refreshToken = userToken.RefreshToken ?? throw new ArgumentException(nameof(userToken));
        var principal = tokenJWTService.CapturaClaimsDoTokenExpirado(token);
        if (principal == null)
        {
            return BadRequest("Token inválido.");
        }

        //Cria um novo DTO de usuário com as informações do principal
        var novoUsuarioDTO = new UsuarioDto
        {
            Email = principal.Identity?.Name,
            Senha = principal.Claims.FirstOrDefault(c => c.Type == "password")?.Value,
        };

        var vollMedUser = await userManager.FindByEmailAsync(novoUsuarioDTO.Email!);

        //Verifica se o refresh token é válido
        if (vollMedUser == null || !vollMedUser.RefreshToken!.Equals(refreshToken) || vollMedUser.ExpireTime <= DateTime.Now)
        {
            return BadRequest("Refresh token inválido.");
        }

        //Gera um novo token e um novo refresh token
        var novoToken = await tokenJWTService.GerarTokenDeUsuarioAsync(novoUsuarioDTO, vollMedUser, userManager);
        var novoRefreshToken = tokenJWTService.GerarRefreshToken();

        //Atualiza o refresh token do usuário
        vollMedUser.RefreshToken = novoRefreshToken;
        vollMedUser.ExpireTime = DateTime.Now.AddMinutes(double.Parse(configuration["JWTTokenConfiguration:RefreshExpireInMinutes"]!));

        //Atualiza o usuário
        await userManager.UpdateAsync(vollMedUser);

        //Retorna o novo token e o novo refresh token
        return Ok(new { novoToken.Token, novoRefreshToken });
    }


}