using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NuGet;

namespace NuGetGallery
{
    public class CuratedFeedService : ICuratedFeedService
    {
        protected IEntityRepository<CuratedFeed> CuratedFeedRepository { get; set; }
        protected IEntityRepository<CuratedPackage> CuratedPackageRepository { get; set; }

        protected CuratedFeedService()
        {
        }

        public CuratedFeedService(
            IEntityRepository<CuratedFeed> curatedFeedRepository,
            IEntityRepository<CuratedPackage> curatedPackageRepository)
        {
            CuratedFeedRepository = curatedFeedRepository;
            CuratedPackageRepository = curatedPackageRepository;
        }

        public CuratedPackage CreatedCuratedPackage(
            CuratedFeed curatedFeed,
            Package package,
            bool included = false,
            bool automaticallyCurated = false,
            string notes = null,
            bool commitChanges = true)
        {
            if (curatedFeed == null)
            {
                throw new ArgumentNullException("curatedFeed");
            }

            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            var curatedPackageRegistration = curatedFeed.Packages
                .SingleOrDefault(cp => cp.PackageRegistrationKey == package.PackageRegistration.Key);

            if (curatedPackageRegistration == null)
            {
                curatedPackageRegistration = new CuratedPackage
                {
                    PackageRegistration = package.PackageRegistration,
                    PackageRegistrationKey = package.PackageRegistrationKey,
                    Included = included,
                    AutomaticallyCurated = automaticallyCurated,
                    Notes = notes,
                };

                curatedFeed.Packages.Add(curatedPackageRegistration);
            }

            if (!curatedPackageRegistration.CuratedPackages.Any(p => p.Key == package.Key))
                curatedPackageRegistration.CuratedPackages.Add(package);

            if (commitChanges)
            {
                CuratedFeedRepository.CommitChanges();
            }

            return curatedPackageRegistration;
        }

        public void DeleteCuratedPackage(
            int curatedFeedKey,
            int curatedPackageKey)
        {
            var curatedFeed = GetFeedByKey(curatedFeedKey, includePackages: true);
            if (curatedFeed == null)
            {
                throw new InvalidOperationException("The curated feed does not exist.");
            }

            var curatedPackage = curatedFeed.Packages.SingleOrDefault(cp => cp.Key == curatedPackageKey);
            if (curatedPackage == null)
            {
                throw new InvalidOperationException("The curated package does not exist.");
            }

            CuratedPackageRepository.DeleteOnCommit(curatedPackage);
            CuratedPackageRepository.CommitChanges();
        }

        public void ModifyCuratedPackage(
            int curatedFeedKey,
            int curatedPackageKey,
            bool included)
        {
            var curatedFeed = GetFeedByKey(curatedFeedKey, includePackages: true);
            if (curatedFeed == null)
            {
                throw new InvalidOperationException("The curated feed does not exist.");
            }

            var curatedPackage = curatedFeed.Packages.SingleOrDefault(cp => cp.Key == curatedPackageKey);
            if (curatedPackage == null)
            {
                throw new InvalidOperationException("The curated package does not exist.");
            }

            curatedPackage.Included = included;
            CuratedFeedRepository.CommitChanges();
        }

        public CuratedFeed GetFeedByName(string name, bool includePackages)
        {
            IQueryable<CuratedFeed> query = CuratedFeedRepository.GetAll();

            if (includePackages)
            {
                query = query
                    .Include(cf => cf.Packages)
                    .Include(cf => cf.Packages.Select(cp => cp.PackageRegistration));
            }

            return query
                .SingleOrDefault(cf => cf.Name == name);
        }

        public CuratedFeed GetFeedByKey(int key, bool includePackages)
        {
            IQueryable<CuratedFeed> query = CuratedFeedRepository.GetAll();

            if (includePackages)
            {
                query = query
                    .Include(cf => cf.Packages)
                    .Include(cf => cf.Packages.Select(cp => cp.PackageRegistration));
            }

            return query
                .SingleOrDefault(cf => cf.Key == key);
        }

        public IEnumerable<CuratedFeed> GetFeedsForManager(int managerKey)
        {
            return CuratedFeedRepository.GetAll()
                .Where(cf => cf.Managers.Any(u => u.Key == managerKey));
        }

        public IQueryable<CuratedPackage> GetCuratedPackages(string curatedFeedName)
        {
            var packages = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .SelectMany(cf => cf.Packages.Where(cp => cp.Included));
            return packages;
        } 

        public IQueryable<Package> GetPackages(string curatedFeedName)
        {
            var packages = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .SelectMany(cf => cf.Packages.SelectMany(cp => cp.PackageRegistration.Packages));

            return packages;
        }

        public IQueryable<PackageRegistration> GetPackageRegistrations(string curatedFeedName)
        {
            var packageRegistrations = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .SelectMany(cf => cf.Packages.Select(cp => cp.PackageRegistration));

            return packageRegistrations;
        }

        public int? GetKey(string curatedFeedName)
        {
            var results = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .Select(cf => cf.Key).Take(1).ToArray();

            return results.Length > 0 ? (int?)results[0] : null;
        }

        public void UpdateIsLatest(PackageRegistration packageRegistration)
        {
            var registrations = CuratedPackageRepository.GetAll()
                .Where(cp => cp.PackageRegistration.Key == packageRegistration.Key)
                .Include(cp => cp.CuratedPackages).ToList();

            foreach (var registration in registrations)
            {
                registration.LatestPackage = null;
                registration.LatestStablePackage = null;
                registration.LastUpdated = DateTime.UtcNow;

                // If the last listed package was just unlisted, then we won't find another one
                var latestPackage = FindPackage(registration.CuratedPackages, p => p.Listed);
                if (latestPackage != null)
                {
                    registration.LatestPackage = latestPackage;
                    registration.LatestStablePackage = latestPackage;
                    if (latestPackage.IsPrerelease)
                    {
                        // If the newest uploaded package is a prerelease package, we need to find an older package that is 
                        // a release version and set it to IsLatest.
                        var latestReleasePackage =
                            FindPackage(registration.CuratedPackages.Where(p => !p.IsPrerelease && p.Listed));
                        if (latestReleasePackage != null)
                        {
                            registration.LatestStablePackage = latestReleasePackage;
                        }
                    }
                }
            }
            CuratedFeedRepository.CommitChanges();
        }

        private static Package FindPackage(IEnumerable<Package> packages, Func<Package, bool> predicate = null)
        {
            if (predicate != null)
            {
                packages = packages.Where(predicate);
            }
            SemanticVersion version = packages.Max(p => new SemanticVersion(p.Version));

            if (version == null)
            {
                return null;
            }
            var v = version.ToString();
            return packages.First(pv => pv.Version.Equals(v, StringComparison.OrdinalIgnoreCase));
        }
    }
}
