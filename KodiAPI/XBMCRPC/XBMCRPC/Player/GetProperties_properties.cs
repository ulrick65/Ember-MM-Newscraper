using System;
using System.Linq;
namespace XBMCRPC.Player
{
    public class GetProperties_properties : global::System.Collections.Generic.List<XBMCRPC.Player.Property.Name>
    {
        public static GetProperties_properties AllFields()
        {
            var items = Enum.GetValues(typeof(XBMCRPC.Player.Property.Name));
            var list = new GetProperties_properties();
            list.AddRange(items.Cast<XBMCRPC.Player.Property.Name>());
            return list;
        }
    }
}
