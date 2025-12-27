namespace XBMCRPC.List.Item
{
    public class All : XBMCRPC.List.Item.Base
    {
        public string channel { get; set; }
        public int channelnumber { get; set; }
        public XBMCRPC.PVR.Channel.Type channeltype { get; set; }
        public string endtime { get; set; }
        public bool hidden { get; set; }
        public bool locked { get; set; }
        public string starttime { get; set; }
    }
}
