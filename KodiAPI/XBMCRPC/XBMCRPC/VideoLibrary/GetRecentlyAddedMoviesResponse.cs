namespace XBMCRPC.VideoLibrary
{
    public class GetRecentlyAddedMoviesResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.Movie> movies { get; set; }
    }
}
