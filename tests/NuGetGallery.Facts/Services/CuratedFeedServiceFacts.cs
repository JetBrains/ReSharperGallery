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
                : base()
            {
                StubCuratedFeed = new CuratedFeed { Key = 0, Name = "aName" };
                StubPackageRegistration = new PackageRegistration { Key = 1066, Id = "aPackageId" };
                StubPackage = new Package() { PackageRegistration = StubPackageRegistration, PackageRegistrationKey = StubPackageRegistration.Key };

                StubCuratedPackage = new CuratedPackage
                {
                    Key = 0, 
                    CuratedFeedKey = StubCuratedFeed.Key, 
                    CuratedFeed = StubCuratedFeed, 
                    PackageRegistration = StubPackageRegistration,
                    PackageRegistrationKey = StubPackageRegistration.Key
                };
                StubCuratedFeed.Packages.Add(StubCuratedPackage);

                StubCuratedFeedRepository = new Mock<IEntityRepository<CuratedFeed>>();
                StubCuratedFeedRepository
                    .Setup(repo => repo.GetAll())
                    .Returns(new CuratedFeed[] { StubCuratedFeed }.AsQueryable());

                StubCuratedPackageRepository = new Mock<IEntityRepository<CuratedPackage>>();
                StubCuratedPackageRepository
                    .Setup(repo => repo.GetAll())
                    .Returns(new CuratedPackage[] { StubCuratedPackage }.AsQueryable());
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

            public PackageRegistration StubPackageRegistration { get; set; }
            public Package StubPackage { get; set; }
            public CuratedFeed StubCuratedFeed { get; set; }
            public CuratedPackage StubCuratedPackage { get; set; }

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
                svc.StubPackageRegistration.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration.Key;

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
                svc.StubPackageRegistration.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration.Key;

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
                svc.StubPackageRegistration.Key = 1067;
                svc.StubPackage.PackageRegistrationKey = svc.StubPackageRegistration.Key;

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
                    0,
                    true);

                Assert.True(svc.StubCuratedPackage.Included);
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
                    0);

                svc.StubCuratedPackageRepository.Verify(stub => stub.DeleteOnCommit(svc.StubCuratedPackage));
                svc.StubCuratedPackageRepository.Verify(stub => stub.CommitChanges());
            }
        }
    }
}
