namespace XBMCRPC.VideoLibrary
{
    public class GetRecentlyAddedMusicVideosResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.MusicVideo> musicvideos { get; set; }
    }
}
