using System.Data.SqlClient;

public class DBServices_Song
{
    public static string CONNECTION_STRING = @"workstation id=En_chanter_Karaoke.mssql.somee.com;packet size=4096;user id=Elya_Amram_SQLLogin_5;pwd=qvrs6xc9y2;data source=En_chanter_Karaoke.mssql.somee.com;persist security info=False;initial catalog=En_chanter_Karaoke;TrustServerCertificate=True";
    // Get all songs
    public static List<Songs> GetAllSongs()
    {
        List<Songs> songs = new List<Songs>();
            string query = "SELECT * FROM Songs";
        using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
        {
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    songs.Add(new Songs()
                    {
                        SongID = int.Parse(rdr["SongID"].ToString()),
                        SongName = rdr["SongName"].ToString(),
                        Artist = rdr["Artist"].ToString(),
                        LinkSong = rdr["LinkSong"].ToString(),
                        SongTypeID = int.Parse(rdr["SongTypeID"].ToString())
                    });
                }
            }
        }
        return songs;
    }

    // Get songs by genre (SongTypeID)
    public static List<Songs> GetSongsByGenre(string genreId)
    {
        List<Songs> songs = new List<Songs>();
        using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
        {
            string query = "SELECT * FROM Songs WHERE SongTypeID = @SongTypeID";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@SongTypeID", genreId);
            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    songs.Add(new Songs
                    {
                        SongID = int.Parse(rdr["SongID"].ToString()),
                        SongName = rdr["SongName"].ToString(),
                        Artist = rdr["Artist"].ToString(),
                        LinkSong = rdr["LinkSong"].ToString(),
                        SongTypeID = int.Parse(rdr["SongTypeID"].ToString())
                    });
                }
            }
        }
        return songs;
    }

    // Create a new song
    public static bool CreateSong(Songs newSong)
    {
        using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
        {
            string query = "INSERT INTO Songs (SongName, Artist, LinkSong, SongTypeID) VALUES (@SongName, @Artist, @LinkSong, @SongTypeID)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@SongName", newSong.SongName);
            cmd.Parameters.AddWithValue("@Artist", newSong.Artist);
            cmd.Parameters.AddWithValue("@LinkSong", newSong.LinkSong);
            cmd.Parameters.AddWithValue("@SongTypeID", newSong.SongTypeID);

            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    // Delete a song by ID
    public static bool DeleteSong(string songId)
    {
        using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
        {
            string query = "DELETE FROM Songs WHERE SongID = @SongID";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@SongID", songId);

            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    // Find a song by name
    public static Songs GetSongByName(string songName)
    {
        Songs song = null;
        using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
        {
            string query = "SELECT * FROM Songs WHERE SongName = @SongName";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@SongName", songName);

            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    song = new Songs
                    {
                        SongID = int.Parse(rdr["SongID"].ToString()),
                        SongName = rdr["SongName"].ToString(),
                        Artist = rdr["Artist"].ToString(),
                        LinkSong = rdr["LinkSong"].ToString(),
                        SongTypeID = int.Parse(rdr["SongTypeID"].ToString())
                    };
                }
            }
        }
        return song;
    }
}
