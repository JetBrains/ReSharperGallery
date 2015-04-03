using System;
using System.Collections.Generic;

namespace NuGetGallery
{
    // Ideally, this would be called CuratedPackageRegistration and the DB
    // table would be CuratedPackageRegistrations
    public class CuratedPackage : IEntity
    {
        public CuratedPackage()
        {
            CuratedPackages = new HashSet<Package>();
            LastUpdated = DateTime.UtcNow;
        }

        public CuratedFeed CuratedFeed { get; set; }
        public int CuratedFeedKey { get; set; }

        public PackageRegistration PackageRegistration { get; set; }
        public int PackageRegistrationKey { get; set; }

        public bool AutomaticallyCurated { get; set; }
        public bool Included { get; set; }
        public string Notes { get; set; }
        public int Key { get; set; }

        public virtual ICollection<Package> CuratedPackages { get; set; }

        public Package LatestPackage { get; set; }
        public int? LatestPackageKey { get; set; }

        public Package LatestStablePackage { get; set; }
        public int? LatestStablePackageKey { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}