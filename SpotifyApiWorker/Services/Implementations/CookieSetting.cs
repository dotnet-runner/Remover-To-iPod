using SpotifyApiWorker.Services.Contracts;

namespace SpotifyApiWorker.Services.Implementations;

public class CookieSetting: ICookieSetting
{
    private readonly CookieOptions _baseOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        Path = "/"
    };

    public CookieOptions AuthSessionOptions(TimeSpan age)
    {
        _baseOptions.MaxAge = age;
        return _baseOptions;
    }

    public CookieOptions SpotifyStateSessionOptions()
    {
        _baseOptions.SameSite = SameSiteMode.Lax;
        return AuthSessionOptions(ICookieSetting.SessionTime);
    }
}