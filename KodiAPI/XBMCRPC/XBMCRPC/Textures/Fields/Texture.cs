using System;
using System.Collections.Generic;
using System.Linq;
namespace XBMCRPC.Textures.Fields
{
    public enum TextureItem
    {
        url,
        cachedurl,
        lasthashcheck,
        imagehash,
        sizes,
    }
    public class Texture : List<TextureItem>
    {
        public static Texture AllFields()
        {
            var items = Enum.GetValues(typeof(TextureItem));
            var list = new Texture();
            list.AddRange(items.Cast<TextureItem>());
            return list;
        }
    }
}
