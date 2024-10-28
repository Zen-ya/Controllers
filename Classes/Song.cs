public class Songs{
    public int SongID { get; set; }
    public string SongName { get; set; }
    public string Artist { get; set; }
    public string LinkSong { get; set; }
    public int SongTypeID { get; set; }

    public override string ToString()
    {
        return $"SongID: {SongID}, SongName: {SongName}, Artist: {Artist}, LinkSong: {LinkSong}, SongTypeID: {SongTypeID}";
    }
}