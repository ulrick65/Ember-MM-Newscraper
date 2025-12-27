namespace XBMCRPC.VideoLibrary
{
    public class GetMovieSetsResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.MovieSet> sets { get; set; }
    }
}
