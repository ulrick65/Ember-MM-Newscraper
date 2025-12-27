namespace XBMCRPC.Setting.Details
{
    public class SettingBool : XBMCRPC.Setting.Details.SettingBase
    {
        [Newtonsoft.Json.JsonProperty("default")]
        public bool Default { get; set; }
        public bool value { get; set; }
    }
}
