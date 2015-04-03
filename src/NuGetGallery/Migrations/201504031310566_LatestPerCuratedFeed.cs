namespace NuGetGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LatestPerCuratedFeed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CuratedPackages", "LatestPackageKey", c => c.Int());
            AddColumn("dbo.CuratedPackages", "LatestStablePackageKey", c => c.Int());
            AddColumn("dbo.CuratedPackages", "LastUpdated", c => c.DateTime(nullable: false, defaultValue: DateTime.UtcNow));
            AddForeignKey("dbo.CuratedPackages", "LatestPackageKey", "dbo.Packages", "Key");
            AddForeignKey("dbo.CuratedPackages", "LatestStablePackageKey", "dbo.Packages", "Key");
            CreateIndex("dbo.CuratedPackages", "LatestPackageKey");
            CreateIndex("dbo.CuratedPackages", "LatestStablePackageKey");

            Sql(
@"UPDATE CuratedPackages
SET LatestPackageKey = Packages.[Key]
FROM CuratedPackages
INNER JOIN Packages ON CuratedPackages.PackageRegistrationKey = Packages.PackageRegistrationKey
WHERE Packages.IsLatest = 1");

            Sql(
@"UPDATE CuratedPackages
SET LatestStablePackageKey = Packages.[Key]
FROM CuratedPackages
INNER JOIN Packages ON CuratedPackages.PackageRegistrationKey = Packages.PackageRegistrationKey
WHERE Packages.IsLatestStable = 1");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CuratedPackages", new[] { "LatestStablePackageKey" });
            DropIndex("dbo.CuratedPackages", new[] { "LatestPackageKey" });
            DropForeignKey("dbo.CuratedPackages", "LatestStablePackageKey", "dbo.Packages");
            DropForeignKey("dbo.CuratedPackages", "LatestPackageKey", "dbo.Packages");
            DropColumn("dbo.CuratedPackages", "LastUpdated");
            DropColumn("dbo.CuratedPackages", "LatestStablePackageKey");
            DropColumn("dbo.CuratedPackages", "LatestPackageKey");
        }
    }
}
