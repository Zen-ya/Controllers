using System.Data.SqlClient;
using System;
using System.Collections.Generic;

public class DBServices_Room
{
    public static string connectionStr = @"workstation id=En_chanter_Karaoke.mssql.somee.com;packet size=4096;user id=Elya_Amram_SQLLogin_5;pwd=qvrs6xc9y2;data source=En_chanter_Karaoke.mssql.somee.com;persist security info=False;initial catalog=En_chanter_Karaoke;TrustServerCertificate=True";

    public static List<KaraokeRooms> GetAllRooms()
    {
        List<KaraokeRooms> rooms = new List<KaraokeRooms>();
        string query = "SELECT * FROM KaraokeRooms";
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    rooms.Add(new KaraokeRooms()
                    {
                        RoomID = (int)rdr["RoomID"],
                        RoomName = rdr["RoomName"].ToString(),
                        OwnerID = (int)rdr["OwnerID"],
                        CreatedAt = (DateTime)rdr["CreatedAt"],
                        RoomType = rdr["RoomType"].ToString(),
                    });
                }
            }
        }
        return rooms;
    }

    public static KaraokeRooms GetRoomById(int roomId)
    {
        KaraokeRooms room2Ret = null;
        string query = "SELECT * FROM KaraokeRooms WHERE RoomID = @RoomID";
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RoomID", roomId);
            con.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    room2Ret = new KaraokeRooms()
                    {
                        RoomID = (int)rdr["RoomID"],
                        RoomName = rdr["RoomName"].ToString(),
                        OwnerID = (int)rdr["OwnerID"],
                        CreatedAt = (DateTime)rdr["CreatedAt"],
                        RoomType = rdr["RoomType"].ToString(),
                    };
                }
            }
        }
        return room2Ret;
    }

    public static bool CreateRoom(KaraokeRooms newRoom)
    {
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "INSERT INTO KaraokeRooms (RoomName, OwnerID, CreatedAt, RoomType) " +
                           "VALUES (@RoomName, @OwnerID, @CreatedAt, @RoomType)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RoomName", newRoom.RoomName);
            cmd.Parameters.AddWithValue("@OwnerID", newRoom.OwnerID);
            cmd.Parameters.AddWithValue("@CreatedAt", newRoom.CreatedAt.Date);
            cmd.Parameters.AddWithValue("@RoomType", newRoom.RoomType);

            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public static bool UpdateRoom(int roomId, KaraokeRooms updatedRoom)
    {
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "UPDATE KaraokeRooms SET RoomName = @RoomName, OwnerID = @OwnerID, CreatedAt = @CreatedAt, RoomType = @RoomType WHERE RoomID = @RoomID";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RoomID", roomId);
            cmd.Parameters.AddWithValue("@RoomName", updatedRoom.RoomName);
            cmd.Parameters.AddWithValue("@OwnerID", updatedRoom.OwnerID);
            cmd.Parameters.AddWithValue("@CreatedAt", updatedRoom.CreatedAt);
            cmd.Parameters.AddWithValue("@RoomType", updatedRoom.RoomType);
            cmd.Parameters.AddWithValue("@RoomPassword", updatedRoom.RoomPassword);
            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public static bool DeleteRoomById(int roomId)
    {
        using (SqlConnection con = new SqlConnection(connectionStr))
        {
            string query = "DELETE FROM KaraokeRooms WHERE RoomID = @RoomID";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@RoomID", roomId);

            con.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0; // Returns true if a row was deleted, false otherwise
        }
    }
}
