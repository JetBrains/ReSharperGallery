using NuGet;
using NuGetGallery.Packaging;

namespace NuGetGallery
{
    public interface IAutomaticallyCuratePackageCommand
    {
        void Execute(
            Package galleryPackage,
            INupkg nugetPackage,
            bool commitChanges);
    }

    public class AutomaticallyCuratePackageCommand : AppCommand, IAutomaticallyCuratePackageCommand
    {
        private readonly ICuratedFeedService curatedFeedService;

        public AutomaticallyCuratePackageCommand(IEntitiesContext entities, ICuratedFeedService curatedFeedService)
            : base(entities)
        {
            this.curatedFeedService = curatedFeedService;
        }

        public void Execute(Package galleryPackage, INupkg nugetPackage, bool commitChanges)
        {
            foreach (var curator in GetServices<IAutomaticPackageCurator>())
            {
                curator.Curate(galleryPackage, nugetPackage, commitChanges: commitChanges);
            }

            curatedFeedService.UpdateIsLatest(galleryPackage.PackageRegistration, commitChanges);
        }
    }
}