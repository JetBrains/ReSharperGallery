using System;

namespace NuGetGallery
{
    public class CuratedPackageVersion : IEntity
    {
        public CuratedPackageVersion()
        {
            LastUpdated = DateTime.UtcNow;
        }

        public int Key { get; set; }

        public CuratedPackage CuratedPackage { get; set; }
        public int CuratedPackageKey { get; set; }

        public CuratedFeed CuratedFeed { get; set; }
        public int CuratedFeedKey { get; set; }

        public PackageRegistration PackageRegistration { get; set; }
        public int PackageRegistrationKey { get; set; }

        public Package Package { get; set; }
        public int PackageKey { get; set; }

        public bool IsLatest { get; set; }
        public bool IsLatestStable { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}