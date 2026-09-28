using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyApiWorker.Exceptions;
using SpotifyApiWorker.Services.Contracts;

namespace SpotifyApiWorker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorization _authorization;
    private readonly IServerSessionKeyGenerator _sessionKeyGenerator;
    private readonly ICookieSetting _cookieSetting;
    private readonly IRedisService _redis;
    
    public AuthorizationController(IAuthorization authorization, ICookieSetting cookieSetting, IServerSessionKeyGenerator serverSessionKeyGenerator,
        IRedisService redis)
    {
        _authorization = authorization;
        _cookieSetting = cookieSetting;
        _sessionKeyGenerator = serverSessionKeyGenerator;
        _redis = redis;
    }
    
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        if (Request.Cookies["auth_id"] is not null)
            Ok();
        
        var authUri = _authorization.CreateAuthorizationUri().ToString();
        var key = _sessionKeyGenerator.Generate();
        
        await _redis.WriteAsync(key, _authorization.State, ICookieSetting.SessionTime);
        
        Response.Cookies.Append("_userSessionKey", key.ToString(),
            _cookieSetting.SpotifyStateSessionOptions());
        return Redirect(authUri);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state,
        [FromQuery] string? error = null)
    {
        if (error is not null)
            return Unauthorized("Authorization Error");
        
        var userSessionKey = Request.Cookies["_userSessionKey"] ?? string.Empty;
        var sessionState = await _redis.GetAsync(userSessionKey);
        
        if (sessionState != state)
            return BadRequest("Authorization Error, cookie is not correct");
        
        _ = _redis.DeleteAsync(new(userSessionKey));
        Response.Cookies.Delete("_userSessionKey");

        var authCode = await _authorization.TryGetAuthorizationCode(code);
        var accessToken = authCode.AccessToken;
        
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new AccessTokenException();
        
        var accessTokenKey = _sessionKeyGenerator.Generate();
        await _redis.WriteAsync(accessTokenKey, accessToken);
        Response.Cookies.Append("auth_id", accessTokenKey);
        
        /*var spotify = new SpotifyClient(accessToken);
        var user = await spotify.UserProfile.Current();*/
        
        return Ok(Redirect("api/Authorization/me"));
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (Request.Cookies["auth_id"] is null)
            return Unauthorized();
        
        var accessTokenId = Request.Cookies["auth_id"];
        
        if(string.IsNullOrWhiteSpace(accessTokenId))
            return BadRequest("Authorization Error");
        
        var accessToken = await _redis.GetAsync(accessTokenId);
        
        var spotify = new SpotifyClient(accessToken);
            
        var user = await spotify.UserProfile.Current();
        return Ok(user);
    }
}