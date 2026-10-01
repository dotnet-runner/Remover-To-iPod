using SpotifyAPI.Web;
using SpotifyApiWorker.Exceptions;
using SpotifyApiWorker.Services.Contracts;

namespace SpotifyApiWorker.Services.Implementations;

public class Authorization: IAuthorization
{
    private readonly Uri _redirectUri = new ("http://127.0.0.1:5000/api/authorization/callback");
    public string State { get; } = Guid.NewGuid().ToString();
    
    public Uri CreateAuthorizationUri()
    {
        var login = new LoginRequest( _redirectUri, Environment.GetEnvironmentVariable("SERVER_SPOTIFY_CLIENT_ID")!, LoginRequest.ResponseType.Code)
        {
            State = State,
            Scope = [Scopes.UserReadPrivate, Scopes.UserReadEmail, Scopes.PlaylistReadPrivate]
        };
        
        return login.ToUri();
    }
    
    public async Task<AuthorizationCodeTokenResponse> TryGetAuthorizationCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new NoAuthorizationCodeException();
        
        try
        {
            var response = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    Environment.GetEnvironmentVariable("SERVER_SPOTIFY_CLIENT_ID")!,
                    Environment.GetEnvironmentVariable("SERVER_SPOTIFY_CLIENT_SECRET")!,
                    code, _redirectUri
                )
            );

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to request token: {ex.Message}");
            throw new AuthorizationCodeTokenException(ex.Message);
        }
    }
}