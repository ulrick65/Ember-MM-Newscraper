namespace XBMCRPC.VideoLibrary
{
    public class GetTVShowsResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.TVShow> tvshows { get; set; }
    }
}
