namespace XBMCRPC.Favourites
{
    public class GetFavouritesResponse
    {
        public global::System.Collections.Generic.List<XBMCRPC.Favourite.Details.Favourite> favourites { get; set; }
        public XBMCRPC.List.LimitsReturned limits { get; set; }
    }
}
