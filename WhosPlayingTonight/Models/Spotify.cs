using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Threading.Tasks;
using System.Configuration;

namespace WhosPlayingTonight.Models
{
    /// <summary>
    /// Provides functionality for streaming song previews using Spotify's API
    /// </summary>
    /// <remarks>
    /// Uses open-source Spotify API wrapper: https://github.com/JohnnyCrazy/SpotifyAPI-NET
    /// </remarks>
    public class Spotify
    {
        /// <summary>
        /// Credentials for accessing Spotify's API
        /// Credentials are stored in an xml file outside of source control
        /// </summary>
        private readonly ClientCredentialsAuth auth = new ClientCredentialsAuth
        {
            ClientId = ConfigurationManager.AppSettings["spotifyClientId"],
            ClientSecret = ConfigurationManager.AppSettings["spotifySecret"]
        };

        /// <summary>
        /// Returns a string that contains the URL of a 30-second preview mp3 for the given artist
        /// </summary>
        /// <param name="artistName"> Artist for whom to get the preview </param>
        /// <returns> 
        /// String that contains the URL of a 30-second preview mp3 for the given artist 
        /// </returns>
        public async Task<string> GetPreviewUrl(string artistName)
        {
            try
            {
                // Authorize Spotify API credentials
                Token token = await auth.DoAuthAsync();
                SpotifyWebAPI api = new SpotifyWebAPI() { TokenType = token.TokenType, AccessToken = token.AccessToken };

                // Get the track from Spotify API
                artistName = FormatArtistName(artistName);
                SearchItem item = api.SearchItems(artistName, SearchType.Artist);
                SeveralTracks tracks = api.GetArtistsTopTracks(item.Artists.Items[0].Id, "US");
                FullTrack track = api.GetTrack(tracks.Tracks[0].Id);

                return track.PreviewUrl;
            }
            catch
            {
                return "(Spotify preview not available)";
            }

        }

        

        /// <summary>
        /// Takes an artist's name as param, and formats it to query Spotify's API
        /// </summary>
        /// <param name="artistName">
        /// Artist's name
        /// </param>
        /// <returns>
        /// String with the artist's name formatted as necessary to query Spotify's API
        /// </returns>
        private string FormatArtistName(string artistName)
        {
            return artistName.Replace(" ", "+");
        }
    }

}
