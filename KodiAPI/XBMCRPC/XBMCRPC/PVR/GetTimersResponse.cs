namespace XBMCRPC.PVR
{
    public class GetTimersResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.PVR.Details.Timer> timers { get; set; }
    }
}
