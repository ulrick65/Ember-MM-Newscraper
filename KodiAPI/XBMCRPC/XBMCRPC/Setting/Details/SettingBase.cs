namespace XBMCRPC.Setting.Details
{
    public class SettingBase : XBMCRPC.Setting.Details.Base
    {
        public object control { get; set; }
        public bool enabled { get; set; }
        public int level { get; set; }
        public string parent { get; set; }
        public XBMCRPC.Setting.Type type { get; set; }
    }
}
