using System;
using System.Linq;
using System.Text.RegularExpressions;
using NuGet;

namespace NuGetGallery
{
  public class ProductPackageCurator : AutomaticPackageCurator
  {
    private static readonly Regex curatedFeedNameParser = new Regex(@"(?<name>[a-z][0-9a-z-]*)(_v(?<version>\d+.*))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public override void Curate(Package galleryPackage, INupkg nugetPackage, bool commitChanges)
    {
      if (!galleryPackage.Dependencies.Any())
        return;

      var curatedFeedNames = GetService<IEntitiesContext>().CuratedFeeds
        .ToList()
        .Select(feed => curatedFeedNameParser.Match(feed.Name))
        .Where(match => match.Success)
        .Select(match => new { Name = match.Value, Id = match.Groups["name"].Value, Version = SemanticVersion.ParseOptionalVersion(match.Groups["version"].Value)})
        .ToList();

      foreach (var dependency in galleryPackage.Dependencies)
      foreach (var curatedFeedData in curatedFeedNames)
      {
        if (!dependency.Id.Equals(curatedFeedData.Id, StringComparison.OrdinalIgnoreCase))
          continue; // not satisfied by name
        if ((curatedFeedData.Version != null) && !VersionUtility.ParseVersionSpec(dependency.VersionSpec).Satisfies(curatedFeedData.Version))
          continue; // not satisfied by version
        var curatedFeed = GetService<ICuratedFeedByNameQuery>().Execute(curatedFeedData.Name, includePackages: true);
        if (curatedFeed.Packages.Any(cp => cp.PackageRegistration.Key == galleryPackage.PackageRegistration.Key))
          continue; // already curated
        if (DependenciesAreCurated(galleryPackage.Dependencies.Except(new[] { dependency }).ToList(), curatedFeed))
        {
          GetService<ICreateCuratedPackageCommand>().Execute(
            curatedFeed,
            galleryPackage.PackageRegistration,
            automaticallyCurated: true,
            commitChanges: commitChanges);
        }
      }
    }
  }
}