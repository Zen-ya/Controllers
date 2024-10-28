public class KaraokeRooms {
    public int RoomID { get; set; }
    public string RoomName { get; set; }
    public int OwnerID {get; set; }
    public DateTime CreatedAt { get; set; }
    public string RoomType { get; set; }

    public string RoomPassword { get; set; }

    public override string ToString()
    {
        return $"RoomID: {RoomID}, RoomName: {RoomName}, OwnerID: {OwnerID}, CreateAt: {CreatedAt}, RoomType: {RoomType}, RoomPassword: {RoomPassword}";
    }
}