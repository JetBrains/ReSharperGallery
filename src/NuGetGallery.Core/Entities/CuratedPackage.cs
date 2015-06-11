using System.Collections.Generic;

namespace NuGetGallery
{
    // Ideally, this would be called CuratedPackageRegistration and the DB
    // table would be CuratedPackageRegistrations
    public class CuratedPackage : IEntity
    {
        public CuratedPackage()
        {
            CuratedPackageVersions = new HashSet<CuratedPackageVersion>();
        }

        public CuratedFeed CuratedFeed { get; set; }
        public int CuratedFeedKey { get; set; }

        public PackageRegistration PackageRegistration { get; set; }
        public int PackageRegistrationKey { get; set; }

        public bool AutomaticallyCurated { get; set; }
        public bool Included { get; set; }
        public string Notes { get; set; }
        public int Key { get; set; }

        // If CuratedPackage were called CuratedPackageRegistration, it would
        // then make sense for this to be a collection of CuratedPackage and
        // called CuratedPackages
        public virtual ICollection<CuratedPackageVersion> CuratedPackageVersions { get; set; }
    }
}