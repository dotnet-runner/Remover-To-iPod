using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyApiWorker.Exceptions;
using SpotifyApiWorker.Infrastructure;
using SpotifyApiWorker.Services.Contracts;

namespace SpotifyApiWorker.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorizationController : ControllerBase
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
        if (Request.Cookies[CookieNames.SpotifySessionId] is not null)
            return Redirect("/api/authorization/me");
        
        var authUri = _authorization.CreateAuthorizationUri().ToString();
        var sessionId = _sessionKeyGenerator.Generate();
        
        await _redis.WriteAsync(sessionId, _authorization.State, ICookieSetting.SessionTime);
        
        Response.Cookies.Append(CookieNames.ScopeSessionId, sessionId.ToString(),
            _cookieSetting.SpotifyStateSessionOptions());
        return Redirect(authUri);
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state,
        [FromQuery] string? error = null)
    {
        if (error is not null)
            return Unauthorized("Authorization Error");
        
        var userSessionKey = Request.Cookies[CookieNames.ScopeSessionId] ?? string.Empty;
        var sessionState = await _redis.GetAsync(userSessionKey);
        
        if (sessionState != state)
            return BadRequest("Authorization Error, cookie is not correct");
        
        _ = _redis.DeleteAsync(new(userSessionKey));
        Response.Cookies.Delete(CookieNames.ScopeSessionId);
        
        var authCode = await _authorization.TryGetAuthorizationCode(code);
        var accessToken = authCode.AccessToken;
        
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new AccessTokenException();
        
        var accessTokenId = _sessionKeyGenerator.Generate();
        await _redis.WriteAsync(accessTokenId, accessToken);
        Response.Cookies.Append(CookieNames.SpotifySessionId, accessTokenId);
        
        return Ok();
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (Request.Cookies[CookieNames.SpotifySessionId] is null)
            return Unauthorized();
        
        var accessTokenId = Request.Cookies[CookieNames.SpotifySessionId];
        
        if(string.IsNullOrWhiteSpace(accessTokenId))
            return BadRequest("Authorization Error");
        
        var accessToken = await _redis.GetAsync(accessTokenId);
        
        var spotify = new SpotifyClient(accessToken);
            
        var user = await spotify.UserProfile.Current();
        return Ok(user);
    }
}