using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using BCrypt.Net;


public class DBServices: YouTubeService
{
    public static string connectionStr = @"workstation id=En_chanter_Karaoke.mssql.somee.com;packet size=4096;user id=Elya_Amram_SQLLogin_5;pwd=qvrs6xc9y2;data source=En_chanter_Karaoke.mssql.somee.com;persist security info=False;initial catalog=En_chanter_Karaoke;TrustServerCertificate=True";
    // Function to verify a password
    public static Users Login(string name, string rawPassword)
    {
        Users usr2Ret = null;

        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "SELECT * FROM Users WHERE UserName = @UserName";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserName", name);

            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    string storedPassword = rdr["Password"].ToString();

                    // Use the Verify method to check if the entered password matches the stored hash
                    if (BCrypt.Net.BCrypt.Verify(rawPassword, storedPassword))
                    {
                        usr2Ret = new Users()
                        {
                            Id = (int)rdr["UserID"],
                            UserName = rdr["UserName"].ToString(),
                            Email = rdr["Email"].ToString(),
                            Phone = rdr["Phone"].ToString(),
                            Birthday = (DateTime)rdr["Birthday"],
                            AvatarUrl = rdr["AvatarUrl"].ToString(),
                        };
                    }
                }
            }
        }

        return usr2Ret;
    }



    public static List<Users> GetUsers()
    {
        List<Users> users = new List<Users>();
        string query = "SELECT * FROM Users";
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    users.Add(new Users()
                    {
                        Id = (int)rdr["UserID"],
                        UserName = rdr["UserName"].ToString(),
                        Email = rdr["Email"].ToString(),
                        Phone = rdr["Phone"].ToString(),
                        Password = rdr["Password"].ToString(),
                        Birthday = (DateTime)rdr["Birthday"],
                        AvatarUrl = rdr["AvatarUrl"].ToString()
                    });
                }
            }
        }
        return users;
    }

    public static bool DeleteUserById(int userId)
    {
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "DELETE FROM Users WHERE UserID = @UserId";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0; // Returns true if a row was deleted, false otherwise
        }
    }

  public static async Task<string> CreateUsers(Users newUser)
{
    int newUserId;

    // Étape 1: Création initiale de l'utilisateur sans PlaylistId
    using (SqlConnection con = new SqlConnection(connectionStr))
    {
        try
        {
            string query = "INSERT INTO Users (UserName, Email, Phone, Password, Birthday) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@UserName, @Email, @Phone, @Password, @Birthday)";

            SqlCommand cmd = new SqlCommand(query, con);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            cmd.Parameters.AddWithValue("@UserName", newUser.UserName);
            cmd.Parameters.AddWithValue("@Email", newUser.Email);
            cmd.Parameters.AddWithValue("@Phone", newUser.Phone);
            cmd.Parameters.AddWithValue("@Password", hashedPassword);
            cmd.Parameters.AddWithValue("@Birthday", newUser.Birthday.Date);

            con.Open();
            newUserId = (int)cmd.ExecuteScalar(); // Récupération du UserID généré
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601) // Contraintes d'unicité
            {
                if (ex.Message.Contains("UQ_Users_Email"))
                    return "Cet e-mail est déjà enregistré. Veuillez utiliser un autre e-mail.";
                if (ex.Message.Contains("UQ_Users_UserName"))
                    return "Ce nom d'utilisateur est déjà pris. Veuillez en choisir un autre.";
            }
            return $"Erreur de base de données : {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Une erreur inattendue est survenue : {ex.Message}";
        }
    }

    // Étape 2: Création de la playlist YouTube et récupération du PlaylistId
    string playlistId = await CreatePlaylistYoutube(newUser.UserName, newUser.Birthday.ToString("yyyy-MM-dd"));
    if (playlistId == null)
    {
        return "Erreur lors de la création de la playlist YouTube.";
    }

    // Étape 3: Mise à jour de l'utilisateur avec le PlaylistId
    using (SqlConnection con = new SqlConnection(connectionStr))
    {
        try
        {
            string updateQuery = "UPDATE Users SET PlaylistId = @PlaylistId WHERE UserID = @UserID";
            SqlCommand updateCmd = new SqlCommand(updateQuery, con);

            updateCmd.Parameters.AddWithValue("@PlaylistId", playlistId);
            updateCmd.Parameters.AddWithValue("@UserID", newUserId);

            con.Open();
            int rowsAffected = updateCmd.ExecuteNonQuery();

            return rowsAffected > 0 ? "Utilisateur et playlist créés avec succès." : "Mise à jour du PlaylistId échouée.";
        }
        catch (Exception ex)
        {
            return $"Erreur lors de la mise à jour de l'utilisateur avec le PlaylistId : {ex.Message}";
        }
    }
}


    public static bool UpdateUser(int id, Users updatedUser)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                string query = "UPDATE Users SET UserName = @UserName, Email = @Email, Phone = @Phone, Password = @Password, Birthday = @Birthday, AvatarUrl = @AvatarUrl WHERE UserID = @UserID";
                SqlCommand cmd = new SqlCommand(query, con);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                cmd.Parameters.AddWithValue("@UserID", id);
                cmd.Parameters.AddWithValue("@UserName", updatedUser.UserName);
                cmd.Parameters.AddWithValue("@Email", updatedUser.Email);
                cmd.Parameters.AddWithValue("@Phone", updatedUser.Phone);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);
                cmd.Parameters.AddWithValue("@Birthday", updatedUser.Birthday);
                cmd.Parameters.AddWithValue("@AvatarUrl", updatedUser.AvatarUrl);  // AvatarUrl peut être mis à jour

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating user: " + ex.Message);
            return false;
        }
    }

    public static Users GetUserById(int userId)
    {
        Users user2Ret = null;
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "SELECT * FROM Users WHERE UserID = @UserId";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    user2Ret = new Users()
                    {
                        Id = (int)rdr["UserID"],
                        UserName = rdr["UserName"].ToString(),
                        Email = rdr["Email"].ToString(),
                        Phone = rdr["Phone"].ToString(),
                        Password = rdr["Password"].ToString(),
                        Birthday = (DateTime)rdr["Birthday"],
                        AvatarUrl = rdr["AvatarUrl"].ToString()
                    };
                }
            }
        }
        return user2Ret;
    }

    public static Users GetUserByEmail(string email)
    {
        Users user2Ret = null;
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "SELECT * FROM Users WHERE Email = @Email";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Email", email);

            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    user2Ret = new Users()
                    {
                        Id = (int)rdr["UserID"],
                        UserName = rdr["UserName"].ToString(),
                        Email = rdr["Email"].ToString(),
                        Phone = rdr["Phone"].ToString(),
                        Password = rdr["Password"].ToString(),
                        Birthday = (DateTime)rdr["Birthday"],
                        AvatarUrl = rdr["AvatarUrl"].ToString()
                    };
                }
            }
        }
        return user2Ret;
    }

    public static bool UpdateUserPasswordById(int id, string newPassword)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                string query = "UPDATE Users SET Password = @Password WHERE UserID = @UserID";
                // Hachage du nouveau mot de passe
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", id);
                cmd.Parameters.AddWithValue("@Password", hashedPassword); // Sauvegarde du mot de passe haché

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur lors de la mise à jour du mot de passe : " + ex.Message);
            return false;
        }
    }

    // public static string CreatePlaylistYoutube(string userName , string Birthday)
    // {
    //     string query = "select access_token from Tokens";
    //     using (SqlConnection con = new SqlConnection(connectionStr))
    //     {
    //         SqlCommand cmd = new SqlCommand(query, con);
    //         con.Open();
    //         //call api youtube to create playlist
    //         //if access_token is expired then call Renew_Token(access_token)
    //         Renew_Token(access_token);
    //         //...
    //         return PlaylistId;
    //     }
    // }

    // public static string  Renew_Token(access_token)
    // {
    //     // API call to renew token
    //     // verifie si le token est pas expire
    //     //...
    //     return NewToken;
    // }

    // public static bool  Add_Video_To_Playlist(string videoId, int playlistId)
    // {
    //     // put video into playlist list API YOUTUBE 
    //     // verifie si le token est pas expire
    // }
    

    // public static bool  Remove_Video_From_Playlist(string videoId, int playlistId)
    // {
    //     // remove video from playlist list API YOUTUBE 
    //     // verifie si le token est pas expire
    // }

}

public class PasswordService
{
    // Function to verify a password
    public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
    {
        // Use BCrypt's Verify method to compare the entered password with the stored hashed password
        return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashedPassword);
    }
}
