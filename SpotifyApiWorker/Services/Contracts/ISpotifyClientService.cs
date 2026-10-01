using SpotifyAPI.Web;

namespace SpotifyApiWorker.Services.Contracts;

public interface ISpotifyClientService
{
    Task<IPlaylistsClient> GetPlaylists();
    Task<FullPlaylist> GetPlaylist(string playlistId);
    Task<Paging<PlaylistTrack<IPlayableItem>>?> GetTracks(string playlistId);
    Task<ITracksClient> GetTracks();
    Task<FullTrack> GetTrack(string trackId);
}