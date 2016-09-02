using Google.Apis.Util;

namespace Lithnet.GoogleApps
{
    public enum EventEnum
    {
        [StringValue("add")]
        Add = 0,
        [StringValue("delete")]
        Delete = 1,
        [StringValue("makeAdmin")]
        MakeAdmin = 2,
        [StringValue("undelete")]
        Undelete = 3,
        [StringValue("update")]
        Update = 4
    }
}
