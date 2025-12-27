namespace XBMCRPC.VideoLibrary
{
    public class GetSeasonsResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.Season> seasons { get; set; }
    }
}
