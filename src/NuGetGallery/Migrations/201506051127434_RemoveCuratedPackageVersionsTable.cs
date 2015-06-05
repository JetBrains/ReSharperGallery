namespace NuGetGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveCuratedPackageVersionsTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CuratedPackages", "LatestPackageKey", "dbo.Packages");
            DropForeignKey("dbo.CuratedPackages", "LatestStablePackageKey", "dbo.Packages");
            DropForeignKey("dbo.CuratedPackageVersions", "CuratedPackageRegistrationKey", "dbo.CuratedPackages");
            DropForeignKey("dbo.CuratedPackageVersions", "PackageKey", "dbo.Packages");
            DropIndex("dbo.CuratedPackages", new[] { "LatestPackageKey" });
            DropIndex("dbo.CuratedPackages", new[] { "LatestStablePackageKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "CuratedPackageRegistrationKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "PackageKey" });
            DropColumn("dbo.CuratedPackages", "LatestPackageKey");
            DropColumn("dbo.CuratedPackages", "LatestStablePackageKey");
            DropColumn("dbo.CuratedPackages", "LastUpdated");
            DropTable("dbo.CuratedPackageVersions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CuratedPackageVersions",
                c => new
                    {
                        CuratedPackageRegistrationKey = c.Int(nullable: false),
                        PackageKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CuratedPackageRegistrationKey, t.PackageKey });
            
            AddColumn("dbo.CuratedPackages", "LastUpdated", c => c.DateTime(nullable: false));
            AddColumn("dbo.CuratedPackages", "LatestStablePackageKey", c => c.Int());
            AddColumn("dbo.CuratedPackages", "LatestPackageKey", c => c.Int());
            CreateIndex("dbo.CuratedPackageVersions", "PackageKey");
            CreateIndex("dbo.CuratedPackageVersions", "CuratedPackageRegistrationKey");
            CreateIndex("dbo.CuratedPackages", "LatestStablePackageKey");
            CreateIndex("dbo.CuratedPackages", "LatestPackageKey");
            AddForeignKey("dbo.CuratedPackageVersions", "PackageKey", "dbo.Packages", "Key", cascadeDelete: true);
            AddForeignKey("dbo.CuratedPackageVersions", "CuratedPackageRegistrationKey", "dbo.CuratedPackages", "Key", cascadeDelete: true);
            AddForeignKey("dbo.CuratedPackages", "LatestStablePackageKey", "dbo.Packages", "Key");
            AddForeignKey("dbo.CuratedPackages", "LatestPackageKey", "dbo.Packages", "Key");
        }
    }
}
