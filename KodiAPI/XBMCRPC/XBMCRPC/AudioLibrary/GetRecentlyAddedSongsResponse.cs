namespace XBMCRPC.AudioLibrary
{
    public class GetRecentlyAddedSongsResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Audio.Details.Song> songs { get; set; }
    }
}
