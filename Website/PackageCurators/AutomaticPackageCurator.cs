using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using NuGet;

namespace NuGetGallery
{
    public abstract class AutomaticPackageCurator : IAutomaticPackageCurator
    {
        public abstract void Curate(
            Package galleryPackage,
            INupkg nugetPackage,
            bool commitChanges);

        protected virtual T GetService<T>()
        {
            return Container.Kernel.TryGet<T>();
        }

        protected static bool DependenciesAreCurated(Package galleryPackage, CuratedFeed curatedFeed)
        {
            return DependenciesAreCurated(galleryPackage.Dependencies, curatedFeed);
        }

        protected static bool DependenciesAreCurated(ICollection<PackageDependency> dependencies, CuratedFeed curatedFeed)
        {
            if (dependencies.IsEmpty())
            {
                return true;
            }

            return dependencies.All(
                d => curatedFeed.Packages
                    .Where(p => p.Included)
                    .Any(p => p.PackageRegistration.Id.Equals(d.Id, StringComparison.OrdinalIgnoreCase)));
        }
    }
}