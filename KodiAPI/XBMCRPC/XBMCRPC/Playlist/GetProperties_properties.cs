using System;
using System.Linq;
namespace XBMCRPC.Playlist
{
    public class GetProperties_properties : global::System.Collections.Generic.List<XBMCRPC.Playlist.Property.Name>
    {
        public static GetProperties_properties AllFields()
        {
            var items = Enum.GetValues(typeof(XBMCRPC.Playlist.Property.Name));
            var list = new GetProperties_properties();
            list.AddRange(items.Cast<XBMCRPC.Playlist.Property.Name>());
            return list;
        }
    }
}
