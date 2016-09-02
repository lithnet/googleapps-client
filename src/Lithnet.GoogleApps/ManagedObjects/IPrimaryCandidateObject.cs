namespace Lithnet.GoogleApps.ManagedObjects
{
    public interface IPrimaryCandidateObject
    {
        bool? Primary { get; set; }

        bool IsPrimary { get; }
    }
}
