using SpotifyAPI.Web;
using SpotifyApiWorker.Services.Contracts;
using SpotifyApiWorker.ValueObjects;

namespace SpotifyApiWorker.Services.Implementations;

public class SpotifyClientService: ISpotifyClientService
{
    private readonly ServerSessionKey _userAuthorizationSessionKey;
    private readonly IRedisService _redis;
    
    public SpotifyClientService(IHttpContextAccessor httpContextAccessor, IRedisService redis)
    {
        _redis = redis;
        
        _userAuthorizationSessionKey = new ServerSessionKey(httpContextAccessor.HttpContext.Request.Cookies["auth_id"]);
    }
    
    public async Task<IPlaylistsClient> GetPlaylists()
    {
        var spotify = await GetSpotify();

        return spotify.Playlists;
    }

    public async Task<FullPlaylist> GetPlaylist(string playlistId)
    {
        var playlists = await GetPlaylists();

        return await playlists.Get(playlistId);
    }

    public async Task<ITracksClient> GetTracks()
    {
        var spotify = await GetSpotify();
        
        return spotify.Tracks;
    }
    
    public async Task<Paging<PlaylistTrack<IPlayableItem>>?> GetTracks(string playlistId)
    {
        var playlist = await GetPlaylist(playlistId);

        return playlist.Items;
    }

    public async Task<FullTrack> GetTrack(string trackId)
    {
        var spotify = await GetSpotify();

        return await spotify.Tracks.Get(trackId);
    }

    private async Task<SpotifyClient> GetSpotify()
    {
        var accessToken = await _redis.GetAsync(_userAuthorizationSessionKey);
        
        return new(accessToken);
    }
}