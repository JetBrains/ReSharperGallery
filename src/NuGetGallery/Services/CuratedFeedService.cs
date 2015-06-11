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

            var isFirstPackageInRegistration = false;
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
                isFirstPackageInRegistration = true;
            }

            if (!curatedPackageRegistration.CuratedPackageVersions.Any(p => p.PackageKey == package.Key))
            {
                var curatedPackageVersion = new CuratedPackageVersion
                {
                    CuratedFeed = curatedFeed,
                    CuratedFeedKey = curatedFeed.Key,
                    PackageRegistration = package.PackageRegistration,
                    PackageRegistrationKey = package.PackageRegistrationKey,
                    Package = package,
                    PackageKey = package.Key,
                };

                // Make sure we set IsLatest + IsLatestStable for the first package, because
                // UpdateIsLatest won't be able to see this registration if we don't commit.
                // If it's the first package in the registration, then it's definitely the
                // latest. It's the latest stable package only if it's not a pre-release.
                if (isFirstPackageInRegistration)
                {
                    curatedPackageVersion.IsLatest = true;
                    curatedPackageVersion.IsLatestStable = !package.IsPrerelease;
                }

                curatedPackageRegistration.CuratedPackageVersions.Add(curatedPackageVersion);
            }

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

        // I do wish CuratedPackage were called CuratedPackageRegistration
        public IQueryable<CuratedPackage> GetCuratedPackageRegistrations(string curatedFeedName)
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
                .SelectMany(cf => cf.Packages.Where(cp => cp.Included).SelectMany(cp => cp.CuratedPackageVersions).Select(cpv => cpv.Package));

            return packages;
        }

        public IQueryable<PackageRegistration> GetPackageRegistrations(string curatedFeedName)
        {
            var packageRegistrations = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .SelectMany(cf => cf.Packages.Where(cp => cp.Included).Select(cp => cp.PackageRegistration));

            return packageRegistrations;
        }

        public int? GetKey(string curatedFeedName)
        {
            var results = CuratedFeedRepository.GetAll()
                .Where(cf => cf.Name == curatedFeedName)
                .Select(cf => cf.Key).Take(1).ToArray();

            return results.Length > 0 ? (int?)results[0] : null;
        }

        public void UpdateIsLatest(PackageRegistration packageRegistration, bool commitChanges)
        {
            var registrations = CuratedPackageRepository.GetAll()
                .Where(cp => cp.PackageRegistration.Key == packageRegistration.Key)
                .Include(cp => cp.CuratedPackageVersions)
                .ToList();

            foreach (var registration in registrations)
            {
                foreach (var pv in registration.CuratedPackageVersions.Where(p => p.IsLatest || p.IsLatestStable))
                {
                    pv.IsLatest = false;
                    pv.IsLatestStable = false;
                    pv.LastUpdated = DateTime.UtcNow;
                }

                // If the last listed package was just unlisted, then we won't find another one
                var latestPackage = FindPackage(registration.CuratedPackageVersions, p => p.Package.Listed);
                if (latestPackage != null)
                {
                    latestPackage.IsLatest = true;
                    latestPackage.LastUpdated = DateTime.UtcNow;

                    if (latestPackage.Package.IsPrerelease)
                    {
                        // If the newest uploaded package is a prerelease package, we need to find an older package that is 
                        // a release version and set it to IsLatest.
                        var latestReleasePackage =
                            FindPackage(registration.CuratedPackageVersions.Where(p => !p.Package.IsPrerelease && p.Package.Listed));
                        if (latestReleasePackage != null)
                        {
                            // We could have no release packages
                            latestReleasePackage.IsLatestStable = true;
                            latestReleasePackage.LastUpdated = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Only release versions are marked as IsLatestStable. 
                        latestPackage.IsLatestStable = true;
                    }
                }
            }

            if (commitChanges)
                CuratedPackageRepository.CommitChanges();
        }

        private static CuratedPackageVersion FindPackage(IEnumerable<CuratedPackageVersion> packages, Func<CuratedPackageVersion, bool> predicate = null)
        {
            if (predicate != null)
            {
                packages = packages.Where(predicate);
            }
            SemanticVersion version = packages.Max(p => new SemanticVersion(p.Package.Version));

            if (version == null)
            {
                return null;
            }
            var v = version.ToString();
            return packages.First(pv => pv.Package.Version.Equals(v, StringComparison.OrdinalIgnoreCase));
        }
    }
}
