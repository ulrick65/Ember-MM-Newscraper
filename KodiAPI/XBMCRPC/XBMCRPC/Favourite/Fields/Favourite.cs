using System;
using System.Collections.Generic;
using System.Linq;
namespace XBMCRPC.Favourite.Fields
{
    public enum FavouriteItem
    {
        window,
        windowparameter,
        thumbnail,
        path,
    }
    public class Favourite : List<FavouriteItem>
    {
        public static Favourite AllFields()
        {
            var items = Enum.GetValues(typeof(FavouriteItem));
            var list = new Favourite();
            list.AddRange(items.Cast<FavouriteItem>());
            return list;
        }
    }
}
