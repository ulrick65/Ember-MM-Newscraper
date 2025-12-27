namespace XBMCRPC.Files
{
    public class GetSourcesResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.List.Items.SourcesItem> sources { get; set; }
    }
}
