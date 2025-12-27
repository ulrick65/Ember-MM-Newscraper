namespace XBMCRPC.Addons
{
    public class GetAddonDetailsResponse
    {
        public XBMCRPC.Addon.Details addon { get; set; }
        public XBMCRPC.List.LimitsReturned limits { get; set; }
    }
}
