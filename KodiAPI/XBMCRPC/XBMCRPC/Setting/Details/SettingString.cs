namespace XBMCRPC.Setting.Details
{
    public class SettingString : XBMCRPC.Setting.Details.SettingBase
    {
        public bool allowempty { get; set; }
        [Newtonsoft.Json.JsonProperty("default")]
        public string Default { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Setting.Details.SettingString_optionsItem> options { get; set; }
        public string value { get; set; }
    }
}
