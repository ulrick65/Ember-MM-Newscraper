using System;
using System.Linq;
namespace XBMCRPC.PVR
{
    public class GetProperties_properties : global::System.Collections.Generic.List<XBMCRPC.PVR.Property.Name>
    {
        public static GetProperties_properties AllFields()
        {
            var items = Enum.GetValues(typeof(XBMCRPC.PVR.Property.Name));
            var list = new GetProperties_properties();
            list.AddRange(items.Cast<XBMCRPC.PVR.Property.Name>());
            return list;
        }
    }
}
