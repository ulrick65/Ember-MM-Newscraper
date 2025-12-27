namespace XBMCRPC.Setting
{
    public enum Type
    {
        boolean,
        integer,
        number,
        [global::System.Runtime.Serialization.EnumMember(Value = "string")]
        String,
        action,
        list,
        path,
        addon,
    }
}
