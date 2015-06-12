using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet;
using NuGetGallery.Packaging;

namespace NuGetGallery
{
  public class RequiredDependencyPackageCurator : AutomaticPackageCurator
  {
    private static readonly Regex curatedFeedNameParser = new Regex(@"(?<name>[a-z][0-9a-z-]*)(_v(?<version>\d+.*))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override void Curate(Package galleryPackage, INupkg nugetPackage, bool commitChanges)
    {
      if (!galleryPackage.Dependencies.Any())
        return;

      var curatedFeedService = GetService<ICuratedFeedService>();

      var entitiesContext = GetService<IEntitiesContext>();
      var curatedFeeds = entitiesContext.CuratedFeeds
        .ToList()
        .Select(feed => new { Match = curatedFeedNameParser.Match(feed.Name), Feed = feed })
        .Where(x => x.Match.Success)
        .Select(x => new
        {
          Name = x.Match.Value,
          Id = x.Match.Groups["name"].Value,
          Version = SemanticVersion.ParseOptionalVersion(x.Match.Groups["version"].Value),
          x.Feed
        })
        .ToList();

      var feedDependencies = (from d in galleryPackage.Dependencies
        from cf in curatedFeeds
        where d.Id.Equals(cf.Id, StringComparison.OrdinalIgnoreCase)
              && (CuratedFeedWantsAllVersions(cf.Version) || CuratedFeedSatisfiesDependency(cf.Version, d))
        select new {Dependency = d, cf.Feed, FeedId = cf.Id}).ToList();

      // Packages that should also be curated to satisfy the dependencies
      var feedDependencyNames = feedDependencies.Select(fd => fd.FeedId).ToList();
      var packageDependencies = (from d in galleryPackage.Dependencies
        where !feedDependencyNames.Contains(d.Id, StringComparer.OrdinalIgnoreCase)
        from p in GetMatchingPackages(d.Id, d.VersionSpec)
        select p).ToList();

      foreach (var feedDependency in feedDependencies)
      {
        var curatedFeed = feedDependency.Feed;

        curatedFeedService.CreatedCuratedPackage(curatedFeed, galleryPackage, 
          included: true, automaticallyCurated: true, commitChanges: commitChanges);

        // Now add all packageDependencies
        foreach (var packageDependency in packageDependencies)
        {
          curatedFeedService.CreatedCuratedPackage(curatedFeed, packageDependency, included: true,
            automaticallyCurated: true, commitChanges: commitChanges);
        }
      }
    }

    private static bool CuratedFeedWantsAllVersions(SemanticVersion curatedFeedVersion)
    {
      // If we don't have a version, we want all versions
      return curatedFeedVersion == null;
    }

    private static bool CuratedFeedSatisfiesDependency(SemanticVersion curatedFeedVersion, PackageDependency dependency)
    {
      var dependencyVersion = dependency.VersionSpec;
      if (dependencyVersion == null)
        return true;

      IVersionSpec dependencyVersionSpec;
      if (!VersionUtility.TryParseVersionSpec(dependency.VersionSpec, out dependencyVersionSpec))
        return false;

      return dependencyVersionSpec.Satisfies(curatedFeedVersion)
        || StripPatchLevel(dependencyVersionSpec).Satisfies(curatedFeedVersion);
    }

    private IEnumerable<Package> GetMatchingPackages(string packageRegistrationId, string requiredVersionSpec)
    {
      var packageRegistrationRepository = GetService<IEntityRepository<PackageRegistration>>();
      var candidatePackages = packageRegistrationRepository.GetAll()
        .Include(pr => pr.Packages)
        .Where(pr => pr.Id == packageRegistrationId)
        .SelectMany(pr => pr.Packages).ToList();

      var versionSpec = VersionUtility.ParseVersionSpec(requiredVersionSpec);
      var dependencies = from p in candidatePackages
        where versionSpec.Satisfies(new SemanticVersion(p.Version))
        select p;

      return dependencies;
    }

    private static IVersionSpec StripPatchLevel(IVersionSpec dependencyVersionSpec)
    {
      // Given a curatedFeedVersion of 8.0, make [8.0.1] work. Semver says the patch
      // level should be backwards compatible, so this should be safe
      var spec = new VersionSpec
      {
        IsMinInclusive = true,
        MinVersion =
          new SemanticVersion(dependencyVersionSpec.MinVersion.Version.Major,
            dependencyVersionSpec.MinVersion.Version.Minor, 0, 0),
        IsMaxInclusive = dependencyVersionSpec.IsMaxInclusive,
        MaxVersion = dependencyVersionSpec.MaxVersion
      };

      return spec;
    }
  }
}