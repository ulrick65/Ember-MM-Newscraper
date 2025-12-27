namespace XBMCRPC.Player
{
    public class SeekResponse
    {
        public double percentage { get; set; }
        public XBMCRPC.Global.Time time { get; set; }
        public XBMCRPC.Global.Time totaltime { get; set; }
    }
}
