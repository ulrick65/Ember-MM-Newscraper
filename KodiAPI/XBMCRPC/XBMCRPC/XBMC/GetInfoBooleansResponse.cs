namespace XBMCRPC.XBMC
{
    public class GetInfoBooleansResponse
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "Library.IsScanningVideo")]
        public bool IsScanningVideo { get; set; }
    }
}
