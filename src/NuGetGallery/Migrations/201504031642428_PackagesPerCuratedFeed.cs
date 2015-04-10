namespace NuGetGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PackagesPerCuratedFeed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CuratedPackageVersions",
                c => new
                    {
                        CuratedPackageRegistrationKey = c.Int(nullable: false),
                        PackageKey = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CuratedPackageRegistrationKey, t.PackageKey })
                .ForeignKey("dbo.CuratedPackages", t => t.CuratedPackageRegistrationKey, cascadeDelete: true)
                .ForeignKey("dbo.Packages", t => t.PackageKey, cascadeDelete: true)
                .Index(t => t.CuratedPackageRegistrationKey)
                .Index(t => t.PackageKey);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.CuratedPackageVersions", new[] { "PackageKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "CuratedPackageRegistrationKey" });
            DropForeignKey("dbo.CuratedPackageVersions", "PackageKey", "dbo.Packages");
            DropForeignKey("dbo.CuratedPackageVersions", "CuratedPackageRegistrationKey", "dbo.CuratedPackages");
            DropTable("dbo.CuratedPackageVersions");
        }
    }
}
