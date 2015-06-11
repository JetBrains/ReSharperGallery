using System;
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
        if (!CuratedFeedWantsAllVersions(curatedFeedData.Version) && !CuratedFeedSatisfiesDependency(curatedFeedData.Version, dependency))
          continue; // not satisfied by version
        var curatedFeedService = GetService<ICuratedFeedService>();
        var curatedFeed = curatedFeedService.GetFeedByName(curatedFeedData.Name, includePackages: true);
        foreach (var d in galleryPackage.Dependencies.Except(new[] { dependency }))
        {
          curatedFeedService.CreatedCuratedPackage(
            curatedFeed,
            d.Package,
            included: true,
            automaticallyCurated: true,
            commitChanges: commitChanges);
        }

        curatedFeedService.CreatedCuratedPackage(
          curatedFeed,
          galleryPackage,
          included: true,
          automaticallyCurated: true,
          commitChanges: commitChanges);
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