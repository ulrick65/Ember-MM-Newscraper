using System;
using System.Linq;
namespace XBMCRPC.System
{
    public class GetProperties_properties : global::System.Collections.Generic.List<XBMCRPC.System.Property.Name>
    {
        public static GetProperties_properties AllFields()
        {
            var items = Enum.GetValues(typeof(XBMCRPC.System.Property.Name));
            var list = new GetProperties_properties();
            list.AddRange(items.Cast<XBMCRPC.System.Property.Name>());
            return list;
        }
    }
}
