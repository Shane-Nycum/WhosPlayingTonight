using NAudio.Wave;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// </summary>
        private static ClientCredentialsAuth auth = new ClientCredentialsAuth
        {
            ClientId = "5abfb22481924b5e91caea93f203b985",
            ClientSecret = "27af275f285f40cbb5e48d3d3f4237eb"
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
        /// Streams an mp3 from the given URL in a new thread
        /// </summary>
        /// <param name="url"> URL where the mp3 file is located </param>
        /// <returns> Thread that's streaming the mp3 </returns>
        /// <remarks>
        /// Uses NAudio library. 
        /// Solution found at https://stackoverflow.com/questions/184683/play-audio-from-a-stream-using-c-sharp.
        /// </remarks>
        public Thread StreamFromUrl(string url)
        {

            var playerThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    using (Stream ms = new MemoryStream())
                    {
                        using (Stream stream = WebRequest.Create(url)
                            .GetResponse().GetResponseStream())
                        {
                            byte[] buffer = new byte[32768];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }

                        ms.Position = 0;
                        using (WaveStream blockAlignedStream =
                            new BlockAlignReductionStream(
                                WaveFormatConversionStream.CreatePcmStream(
                                    new Mp3FileReader(ms))))
                        {
                            using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                            {
                                waveOut.Init(blockAlignedStream);
                                waveOut.Play();
                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }
                            }
                        }
                    }
                }
                catch { }

            });
            playerThread.Start();
            return playerThread;

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
            string trimArtistName = artistName.Trim();
            return artistName.Replace(" ", "+");
        }
    }

}
