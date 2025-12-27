namespace XBMCRPC.VideoLibrary
{
    public class GetMoviesResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Video.Details.Movie> movies { get; set; }
    }
}
