﻿using System;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace NuGetGallery.PackageCurators
{
    public class RequiredDependencyPackageCuratorFacts
    {
        public class TestableRequiredDependencyPackageCurator : RequiredDependencyPackageCurator
        {
            public const string RequiredDependencyPackageId = "RequiredDependency";

            public TestableRequiredDependencyPackageCurator()
            {
                StubCuratedFeed = new CuratedFeed
                {
                    Key = 0,
                    Name = RequiredDependencyPackageId + "_v3.0"
                };

                FakeEntitiesContext = new FakeEntitiesContext();
                FakeEntitiesContext.CuratedFeeds.Add(StubCuratedFeed);

                StubCuratedFeedService = new Mock<ICuratedFeedService>();
                StubCuratedFeedService
                    .Setup(stub => stub.GetFeedByName(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(StubCuratedFeed);
            }

            public CuratedFeed StubCuratedFeed { get; private set; }
            public FakeEntitiesContext FakeEntitiesContext { get; private set; }
            public Mock<ICuratedFeedService> StubCuratedFeedService { get; private set; }

            protected override T GetService<T>()
            {
                if (typeof (T) == typeof (IEntitiesContext))
                {
                    return (T) ((object)FakeEntitiesContext);
                }

                if (typeof(T) == typeof(ICuratedFeedService))
                {
                    return (T) StubCuratedFeedService.Object;
                }

                throw new Exception(string.Format("Tried to get an unexpected service - {0}", typeof(T)));
            }
        }

        public class TheCurateMethod
        {
            [Fact]
            public void WillNotIncludeThePackageWhenTheFeedDoesNotExist()
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.0");

                curator.FakeEntitiesContext.CuratedFeeds.Remove(curator.StubCuratedFeed);

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        It.IsAny<CuratedFeed>(),
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Never());
            }

            [Fact]
            public void WillNotIncludeThePackageWhenTheDependencyIdDoesNotMatch()
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, "NotTheRequiredDependency", "12.0");

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        It.IsAny<CuratedFeed>(),
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Never());
            }

            [Theory]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "2.0")]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.0")]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "[3.0, 3.0]")]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "[3.0, 3.1]")]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "[3.0, 4.0)")]
            public void WillIncludeThePackageWhenItHasMatchingDependency(string dependentPackageId, string versionSpec)
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, dependentPackageId, versionSpec);

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        curator.StubCuratedFeed,
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Once());
            }

            [Fact]
            public void WillIncludeThePackageWhenCuratedFeedDoesNotIncludeVersion()
            {
                var curator = new TestableRequiredDependencyPackageCurator
                {
                    StubCuratedFeed = {Name = TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId}
                };

                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "1.0");

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        curator.StubCuratedFeed,
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Once());
            }

            [Theory]
            [InlineData("3.0", true)]
            [InlineData("3.0.1", true)]
            [InlineData("[3.0.1]", true)]
            [InlineData("[3.0.1, 3.0.3]", true)]
            [InlineData("3.1", false)]
            [InlineData("(3.0.0, 3.1]", true)]
            [InlineData("(3.0.3, 3.1]", true)]
            [InlineData("[3.0-alpha1, 3.1]", true)]
            [InlineData("[3.0.3-alpha1, 3.1]", true)]
            [InlineData("[3.1-alpha1, 4.0]", false)]
            public void WillIgnorePatchLevelWhenComparingSemanticVersions(string versionSpec, bool shouldInclude)
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, versionSpec);

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        curator.StubCuratedFeed,
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    shouldInclude ? Times.Once() : Times.Never());
            }

            [Fact]
            public void WillSetTheAutomaticBitWhenIncludingThePackage()
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.0");

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        curator.StubCuratedFeed,
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        true,
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Once());
            }


            [Theory]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.1")]
            [InlineData(TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "4.0")]
            public void WillNotIncludeThePackageWhenTheDependencyVersionSpecDoesNotMatch(string dependentPackageId, string versionSpec)
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, dependentPackageId, versionSpec);

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        It.IsAny<CuratedFeed>(),
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Never());
            }

            [Fact]
            public void WillNotIncludeThePackageWhenItDependsOnAPackageThatIsNotIncluded()
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.0");
                AddDependency(package, "NotIncluded", "3.0");

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        It.IsAny<CuratedFeed>(),
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Never());
            }

            [Fact]
            public void WillNotIncludeThePackageWhenItDependsOnAPackageThatIsExcludedInTheFeed()
            {
                var curator = new TestableRequiredDependencyPackageCurator();
                curator.StubCuratedFeed.Packages.Add(new CuratedPackage { AutomaticallyCurated = false, Included = false, PackageRegistration = new PackageRegistration { Id = "ManuallyExcludedPackage" } });

                var package = CreateStubGalleryPackage();
                AddDependency(package, TestableRequiredDependencyPackageCurator.RequiredDependencyPackageId, "3.0");
                AddDependency(package, "ManuallyExcludedPackage", "3.0");

                curator.Curate(package, null, commitChanges: true);

                curator.StubCuratedFeedService.Verify(
                    stub => stub.CreatedCuratedPackage(
                        It.IsAny<CuratedFeed>(),
                        It.IsAny<PackageRegistration>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()),
                    Times.Never());
            }

            private static Package CreateStubGalleryPackage()
            {
                return new Package
                {
                    IsLatestStable = true,
                    PackageRegistration = new PackageRegistration
                    {
                        Key = 0,
                    },
                };
            }

            private static void AddDependency(Package package, string id, string versionSpec)
            {
                package.Dependencies.Add(new PackageDependency
                {
                    Id = id,
                    VersionSpec = versionSpec
                });
                package.FlattenedDependencies = package.Dependencies.Flatten();
            }
        }
    }
}