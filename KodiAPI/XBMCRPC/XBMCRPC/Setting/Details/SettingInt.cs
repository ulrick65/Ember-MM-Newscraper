namespace XBMCRPC.Setting.Details
{
    public class SettingInt : XBMCRPC.Setting.Details.SettingBase
    {
        [Newtonsoft.Json.JsonProperty("default")]
        public int Default { get; set; }
        public int maximum { get; set; }
        public int minimum { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Setting.Details.SettingInt_optionsItem> options { get; set; }
        public int step { get; set; }
        public int value { get; set; }
    }
}
