using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class YouTubeService
{
    private static readonly string connectionStr = "workstation id=En_chanter_Karaoke.mssql.somee.com;packet size=4096;user id=Elya_Amram_SQLLogin_5;pwd=qvrs6xc9y2;data source=En_chanter_Karaoke.mssql.somee.com;persist security info=False;initial catalog=En_chanter_Karaoke;TrustServerCertificate=True";
    private static string accessToken;
    public static int UserID ;

    public static async Task<string> CreatePlaylistYoutube(string userName, string birthday)
    {
        string query = "SELECT access_token FROM Tokens";
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
             Console.WriteLine("CreatePlaylistYoutube : ");
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            accessToken = (string)cmd.ExecuteScalar();
            Console.WriteLine(accessToken);
            // Vérification et renouvellement du token si nécessaire
            accessToken = await EnsureValidToken(accessToken);
             Console.WriteLine("After verifie token CreatePlaylistYoutube : ");
            // Appel à l'API pour créer la playlist
            var playlistId = await CallCreatePlaylistAPI(userName, birthday);
            //ajoute ici un update pour ajouter le playlistId 
            Console.WriteLine("C'est le Playlist ID :  => "+ playlistId);
            return playlistId;
        }
    }

    private static async Task<string> EnsureValidToken(string token)
    {
        if (await IsTokenExpired(token))
        {
            Console.WriteLine("Token expire, renew now...");
            accessToken = await Renew_Token(token);
        }
        return accessToken;
    }

    private static async Task<bool> IsTokenExpired(string token)
    {
        // Exemple de vérification simple en appelant une ressource protégée
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("https://www.googleapis.com/youtube/v3/playlists?part=id&mine=true");
            return response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
    }

    public static async Task<string> Renew_Token(string expiredToken)
{ Console.WriteLine("Renew_Token : ");
    using (HttpClient client = new HttpClient())
    {
        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "484173504020-775g99p6thebc5lg80jr6v8m9lrqio7r.apps.googleusercontent.com"),
            new KeyValuePair<string, string>("client_secret", "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"),
            new KeyValuePair<string, string>("refresh_token", "1//036l6dHjRBgGsCgYIARAAGAMSNwF-L9IrmeP_okPDU7EoSbgGr7Remj4vE1X15pz86ZzFVedAhoSNRaQgJiT4l9q-obK4bpidku4"),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

        HttpResponseMessage response = await client.PostAsync("https://oauth2.googleapis.com/token", requestBody);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JObject.Parse(jsonResponse);
            string newAccessToken = tokenData["access_token"]?.ToString();

            if (!string.IsNullOrEmpty(newAccessToken))
            {
                // Mettre à jour le token dans la base de données
                await UpdateAccessTokenInDatabase(newAccessToken);
                return newAccessToken;
            }
            else
            {
                throw new Exception("the new token is empty.");
            }
        }
        else
        {   Console.WriteLine("Échec du renouvellement du token :  ");
            throw new Exception("erro renew token : " + response.ReasonPhrase);
        }
    }
}
    private static async Task UpdateAccessTokenInDatabase(string newAccessToken)
    { Console.WriteLine("Update access token : ");
        // Chaine de connexion à la base de données
        using (SqlConnection connection = new SqlConnection(connectionStr))
        {
            await connection.OpenAsync();

            string updateQuery = "UPDATE Tokens SET access_token = @newAccessToken WHERE id = 1"; // Adaptez selon votre schéma

            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@newAccessToken", newAccessToken);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new Exception("Aucune mise à jour effectuée sur le jeton d'accès.");
                }
            }
        }
    }
    private static async Task<string> CallCreatePlaylistAPI(string userName, string birthday)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                snippet = new
                {
                    title = $"{userName}'s Playlist - {birthday}",
                    description = $"Playlist de {userName} créée via l'API YouTube."
                },
                status = new
                {
                    privacyStatus = "private"
                }
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://www.googleapis.com/youtube/v3/playlists?part=snippet%2Cstatus", content);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            var playlistData = JObject.Parse(result);
            return playlistData["id"]?.ToString();
        }
    }

    public static async Task<bool> Add_Video_To_Playlist(string videoId, string playlistId)
    {
        await EnsureValidToken(accessToken);
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestBody = new
            {
                snippet = new
                {
                    playlistId = playlistId,
                    resourceId = new
                    {
                        kind = "youtube#video",
                        videoId = videoId
                    }
                }
            };

            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet", content);
            return response.IsSuccessStatusCode;
        }
    }

    public static async Task<bool> Remove_Video_From_Playlist(string playlistItemId)
    {
        await EnsureValidToken(accessToken);
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.DeleteAsync($"https://www.googleapis.com/youtube/v3/playlistItems?id={playlistItemId}");
            return response.IsSuccessStatusCode;
        }
    }
}
