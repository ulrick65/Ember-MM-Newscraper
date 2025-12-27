namespace XBMCRPC.VideoLibrary
{
    public class GetEpisodesResponse
    {
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.Episode> episodes { get; set; }
        public XBMCRPC.List.LimitsReturned limits { get; set; }
    }
}
