using System;
using System.Linq;
using Moq;
using Xunit;

namespace NuGetGallery.Services
{
    public class CuratedFeedServiceFacts
    {
        public class TestableCuratedFeedService : CuratedFeedService
        {
            public TestableCuratedFeedService()
            {
                StubCuratedFeed = new CuratedFeed { Key = 0, Name = "aName" };
                StubCuratedFeed2 = new CuratedFeed {Key = 1, Name = "AnotherFeed"};
                StubPackageRegistration_ForFeed1 = new PackageRegistration { Key = 1066, Id = "aPackageId" };
                StubPackageRegistration_ForFeed2 = new PackageRegistration { Key = 1067, Id = "aPackageId2" };
                StubPackageRegistration_ForFeed1_NotIncluded = new PackageRegistration { Key = 1068, Id = "aPackageId2" };
                StubPackage = new Package { PackageRegistration = StubPackageRegistration_ForFeed1, PackageRegistrationKey = StubPackageRegistration_ForFeed1.Key, Version = "1.0.0" };
                StubPackage_IncompatibleVersion = new Package { PackageRegistration = StubPackageRegistration_ForFeed1, PackageRegistrationKey = StubPackageRegistration_ForFeed1.Key, Version = "2.0.0" };
                StubPackage_ForFeed1_NotIncluded = new Package { PackageRegistration = StubPackageRegistration_ForFeed1_NotIncluded, PackageRegistrationKey = StubPackageRegistration_ForFeed1_NotIncluded.Key };

                StubCuratedPackageRegistration_ForFeed1 = new CuratedPackage
                {
                    Key = 1, 
                    CuratedFeedKey = StubCuratedFeed.Key, 
                    CuratedFeed = StubCuratedFeed, 
                    PackageRegistration = StubPackageRegistration_ForFeed1,
                    PackageRegistrationKey = StubPackageRegistration_ForFeed1.Key,
                    Included = true
                };
                StubCuratedFeed.Packages.Add(StubCuratedPackageRegistration_ForFeed1);

                StubCuratedPackageRegistration_ForFeed2 = new CuratedPackage
                {
                    Key = 2,
                    CuratedFeedKey = StubCuratedFeed2.Key,
                    CuratedFeed = StubCuratedFeed2,
                    PackageRegistration = StubPackageRegistration_ForFeed2,
                    PackageRegistrationKey = StubPackageRegistration_ForFeed2.Key,
                    Included = true
                };
                StubCuratedFeed2.Packages.Add(StubCuratedPackageRegistration_ForFeed2);

                StubCuratedPackageRegistration_ForFeed1_NotIncluded = new CuratedPackage
                {
                    Key = 3,
                    CuratedFeedKey = StubCuratedFeed.Key,
                    CuratedFeed = StubCuratedFeed,
                    PackageRegistration = StubPackageRegistration_ForFeed1_NotIncluded,
                    PackageRegistrationKey = StubPackageRegistration_ForFeed1_NotIncluded.Key,
                    Included = false
                };
                StubCuratedFeed.Packages.Add(StubCuratedPackageRegistration_ForFeed1_NotIncluded);

                StubCuratedFeedRepository = new Mock<IEntityRepository<CuratedFeed>>();
                StubCuratedFeedRepository
                    .Setup(repo => repo.GetAll())
                    .Returns(new CuratedFeed[] { StubCuratedFeed, StubCuratedFeed2 }.AsQueryable());

                StubCuratedPackageRepository = new Mock<IEntityRepository<CuratedPackage>>();
                StubCuratedPackageRepository
                    .Setup(repo => repo.GetAll())
                    .Returns(new CuratedPackage[] { StubCuratedPackageRegistration_ForFeed1, StubCuratedPackageRegistration_ForFeed2, StubCuratedPackageRegistration_ForFeed1_NotIncluded }.AsQueryable());
            }

            public Mock<IEntityRepository<CuratedFeed>> StubCuratedFeedRepository {
                get
                {
                    return _stubCuratedFeedRepository;
                }
                set
                {
                    _stubCuratedFeedRepository = value; 
                    CuratedFeedRepository = value.Object;
                }
            }

            public Mock<IEntityRepository<CuratedPackage>> StubCuratedPackageRepository {
                get
                {
                    return _stubCuratedPackageRepository;
                }
                set
                {
                    _stubCuratedPackageRepository = value;
                    CuratedPackageRepository = value.Object;
                }
            }

            public PackageRegistration StubPackageRegistration_ForFeed1 { get; set; }
            public PackageRegistration StubPackageRegistration_ForFeed2 { get; set; }
            public PackageRegistration StubPackageRegistration_ForFeed1_NotIncluded { get; set; }
            public Package StubPackage { get; set; }
            public Package StubPackage_IncompatibleVersion { get; set; }
            public Package StubPackage_ForFeed1_NotIncluded { get; set; }
            public CuratedFeed StubCuratedFeed { get; set; }
            public CuratedFeed StubCuratedFeed2 { get; set; }
            public CuratedPackage StubCuratedPackageRegistration_ForFeed1 { get; set; }
            public CuratedPackage StubCuratedPackageRegistration_ForFeed2 { get; set; }
            public CuratedPackage StubCuratedPackageRegistration_ForFeed1_NotIncluded { get; set; }

            Mock<IEntityRepository<CuratedFeed>> _stubCuratedFeedRepository;
            Mock<IEntityRepository<CuratedPackage>> _stubCuratedPackageRepository;
        }

        public class TheCreateCuratedPackageMethod
        {
            [Fact]
            public void WillThrowWhenCuratedFeedDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<ArgumentNullException>(
                    () => svc.CreatedCuratedPackage(
                        null,
                        svc.StubPackage));
            }

            [Fact]
            public void WillThrowWhenPackageRegistrationDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<ArgumentNullException>(
                    () => svc.CreatedCuratedPackage(
                        svc.StubCuratedFeed,
                        null));
            }

            [Fact]
            public void WillAddANewCuratedPackageToTheCuratedFeed()
            {
                var svc = new TestableCuratedFeedService();
                svc.StubPackageRegistration_ForFeed1.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration_ForFeed1.Key;

                svc.CreatedCuratedPackage(
                    svc.StubCuratedFeed,
                    svc.StubPackage,
                    false,
                    true,
                    "theNotes");

                var curatedPackage = svc.StubCuratedFeed.Packages.Last();
                Assert.Equal(1067, curatedPackage.PackageRegistrationKey);
                Assert.Equal(false, curatedPackage.Included);
                Assert.Equal(true, curatedPackage.AutomaticallyCurated);
                Assert.Equal("theNotes", curatedPackage.Notes);
            }

            [Fact]
            public void WillSaveTheEntityChanges()
            {
                var svc = new TestableCuratedFeedService();
                svc.StubPackageRegistration_ForFeed1.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration_ForFeed1.Key;

                svc.CreatedCuratedPackage(
                    svc.StubCuratedFeed,
                    svc.StubPackage,
                    false,
                    true,
                    "theNotes");

                svc.StubCuratedFeedRepository.Verify(stub => stub.CommitChanges());
            }

            [Fact]
            public void WillReturnTheCreatedCuratedPackage()
            {
                var svc = new TestableCuratedFeedService();
                svc.StubPackageRegistration_ForFeed1.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration_ForFeed1.Key;

                var curatedPackage = svc.CreatedCuratedPackage(
                    svc.StubCuratedFeed,
                    svc.StubPackage,
                    false,
                    true,
                    "theNotes");

                Assert.Equal(1067, curatedPackage.PackageRegistrationKey);
                Assert.Equal(false, curatedPackage.Included);
                Assert.Equal(true, curatedPackage.AutomaticallyCurated);
                Assert.Equal("theNotes", curatedPackage.Notes);
            }
        }

        public class TheModifyCuratedPackageMethod
        {
            [Fact]
            public void WillThrowWhenCuratedFeedDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<InvalidOperationException>(
                    () => svc.ModifyCuratedPackage(
                        42,
                        0,
                        false));
            }

            [Fact]
            public void WillThrowWhenCuratedPackageDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<InvalidOperationException>(
                    () => svc.ModifyCuratedPackage(
                        0,
                        404,
                        false));
            }

            [Fact]
            public void WillModifyAndSaveTheCuratedPackage()
            {
                var svc = new TestableCuratedFeedService();

                svc.ModifyCuratedPackage(
                    0,
                    1,
                    true);

                Assert.True(svc.StubCuratedPackageRegistration_ForFeed1.Included);
                svc.StubCuratedFeedRepository.Verify(stub => stub.CommitChanges());
            }
        }

        public class TheDeleteCuratedPackageMethod
        {
            [Fact]
            public void WillThrowWhenCuratedFeedDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<InvalidOperationException>(
                    () => svc.DeleteCuratedPackage(
                        42,
                        0));
            }

            [Fact]
            public void WillThrowWhenCuratedPackageDoesNotExist()
            {
                var svc = new TestableCuratedFeedService();

                Assert.Throws<InvalidOperationException>(
                    () => svc.DeleteCuratedPackage(
                        0,
                        1066));
            }

            [Fact]
            public void WillDeleteTheCuratedPackage()
            {
                var svc = new TestableCuratedFeedService();

                svc.DeleteCuratedPackage(
                    0,
                    1);

                svc.StubCuratedPackageRepository.Verify(stub => stub.DeleteOnCommit(svc.StubCuratedPackageRegistration_ForFeed1));
                svc.StubCuratedPackageRepository.Verify(stub => stub.CommitChanges());
            }
        }

        public class TheUpdateIsLatestMethod
        {
            [Fact]
            public void CommitChanges()
            {
                var service = new TestableCuratedFeedService();

                service.UpdateIsLatest(service.StubPackageRegistration_ForFeed1, true);

                service.StubCuratedFeedRepository.Verify(r => r.CommitChanges(), Times.Once());
            }

            [Fact]
            public void DoesNotSetLaterIncompatiblePackageAsLatest()
            {
                var service = new TestableCuratedFeedService();

                service.StubCuratedPackageRegistration_ForFeed1.CuratedPackages.Add(service.StubPackage);
                service.StubCuratedPackageRegistration_ForFeed1.CuratedPackages.Add(service.StubPackage_IncompatibleVersion);

                service.UpdateIsLatest(service.StubPackageRegistration_ForFeed1, true);

                Assert.Equal(service.StubPackage_IncompatibleVersion, service.StubCuratedPackageRegistration_ForFeed1.LatestPackage);

                service.StubCuratedPackageRegistration_ForFeed1.CuratedPackages.Remove(service.StubPackage_IncompatibleVersion);

                service.UpdateIsLatest(service.StubPackageRegistration_ForFeed1, true);

                Assert.Equal(service.StubPackage, service.StubCuratedPackageRegistration_ForFeed1.LatestPackage);
            }
        }

        public class TheGetPackagesMethod
        {
            [Fact]
            public void WillOnlyReturnPackageVersionsForIncludedPackageRegistrations()
            {
                var service = new TestableCuratedFeedService();
                service.StubCuratedPackageRegistration_ForFeed1.CuratedPackages.Add(service.StubPackage);
                service.StubCuratedPackageRegistration_ForFeed1_NotIncluded.CuratedPackages.Add(service.StubPackage_ForFeed1_NotIncluded);

                var packages = service.GetPackages(service.StubCuratedFeed.Name).ToList();

                Assert.Contains(service.StubPackage, packages);
                Assert.DoesNotContain(service.StubPackage_ForFeed1_NotIncluded, packages);
            }

            [Fact]
            public void WillOnlyReturnCuratedPackageVersions()
            {
                // I.e. Not all versions for a registration
                var service = new TestableCuratedFeedService();
                service.StubCuratedPackageRegistration_ForFeed1.CuratedPackages.Add(service.StubPackage);

                var packages = service.GetPackages(service.StubCuratedFeed.Name).ToList();

                Assert.Contains(service.StubPackage, packages);
                Assert.DoesNotContain(service.StubPackage_IncompatibleVersion, packages);
            }
        }

        public class TheGetCuratedPackageRegistrationsMethod
        {
            [Fact]
            public void WillOnlyReturnCuratedPackageRegistrationsForSpecificFeed()
            {
                var service = new TestableCuratedFeedService();

                var registrations = service.GetCuratedPackageRegistrations(service.StubCuratedFeed.Name).ToList();
                Assert.Contains(service.StubCuratedPackageRegistration_ForFeed1, registrations);
                Assert.DoesNotContain(service.StubCuratedPackageRegistration_ForFeed2, registrations);
            }

            [Fact]
            public void WillOnlyReturnIncludedCuratedPackageRegistrations()
            {
                var service = new TestableCuratedFeedService();

                var registrations = service.GetCuratedPackageRegistrations(service.StubCuratedFeed.Name).ToList();
                Assert.Contains(service.StubCuratedPackageRegistration_ForFeed1, registrations);
                Assert.DoesNotContain(service.StubCuratedPackageRegistration_ForFeed1_NotIncluded, registrations);
            }
        }

        public class TheGetPackageRegistrationsMethod
        {
            [Fact]
            public void WillOnlyReturnPackageRegistrationsForSelectedFeed()
            {
                var service = new TestableCuratedFeedService();

                var packageRegistrations = service.GetPackageRegistrations(service.StubCuratedFeed.Name).ToList();
                Assert.Contains(service.StubPackageRegistration_ForFeed1, packageRegistrations);
                Assert.DoesNotContain(service.StubPackageRegistration_ForFeed2, packageRegistrations);
            }

            [Fact]
            public void WillOnlyReturnIncludedPackageRegistrations()
            {
                var service = new TestableCuratedFeedService();

                var packageRegistrations = service.GetPackageRegistrations(service.StubCuratedFeed.Name).ToList();
                Assert.Contains(service.StubPackageRegistration_ForFeed1, packageRegistrations);
                Assert.DoesNotContain(service.StubPackageRegistration_ForFeed1_NotIncluded, packageRegistrations);
            }
        }
    }
}
