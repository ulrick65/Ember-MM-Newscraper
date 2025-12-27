namespace XBMCRPC.PVR
{
    public class GetRecordingsResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.PVR.Details.Recording> recordings { get; set; }
    }
}
