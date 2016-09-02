using Google.Apis.Util;

namespace Lithnet.GoogleApps
{
    public enum OrderByEnum
    {
        [StringValue("email")]
        Email = 0,
        [StringValue("familyName")]
        FamilyName = 1,
        [StringValue("givenName")]
        GivenName = 2
    }
}
