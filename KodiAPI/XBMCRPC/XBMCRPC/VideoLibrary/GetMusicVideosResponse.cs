namespace XBMCRPC.VideoLibrary
{
    public class GetMusicVideosResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.MusicVideo> musicvideos { get; set; }
    }
}
