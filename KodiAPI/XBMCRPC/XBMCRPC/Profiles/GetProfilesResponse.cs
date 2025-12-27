namespace XBMCRPC.Profiles
{
    public class GetProfilesResponse
    {
        public XBMCRPC.List.LimitsReturned limits { get; set; }
        public global::System.Collections.Generic.List<XBMCRPC.Profiles.Details.Profile> profiles { get; set; }
    }
}
