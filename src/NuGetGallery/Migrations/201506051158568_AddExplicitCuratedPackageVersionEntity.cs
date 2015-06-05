namespace NuGetGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExplicitCuratedPackageVersionEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CuratedPackageVersions",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        CuratedPackageKey = c.Int(nullable: false),
                        CuratedFeedKey = c.Int(nullable: false),
                        PackageRegistrationKey = c.Int(nullable: false),
                        PackageKey = c.Int(nullable: false),
                        IsLatest = c.Boolean(nullable: false),
                        IsLatestStable = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.CuratedFeeds", t => t.CuratedFeedKey, cascadeDelete: true)
                .ForeignKey("dbo.PackageRegistrations", t => t.PackageRegistrationKey, cascadeDelete: true)
                .ForeignKey("dbo.Packages", t => t.PackageKey, cascadeDelete: true)
                .ForeignKey("dbo.CuratedPackages", t => t.CuratedPackageKey)
                .Index(t => t.CuratedFeedKey)
                .Index(t => t.PackageRegistrationKey)
                .Index(t => t.PackageKey)
                .Index(t => t.CuratedPackageKey)
                .Index(t => t.IsLatest)
                .Index(t => t.IsLatestStable);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.CuratedPackageVersions", new[] { "IsLatestStable" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "IsLatest" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "CuratedPackageKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "PackageKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "PackageRegistrationKey" });
            DropIndex("dbo.CuratedPackageVersions", new[] { "CuratedFeedKey" });
            DropForeignKey("dbo.CuratedPackageVersions", "CuratedPackageKey", "dbo.CuratedPackages");
            DropForeignKey("dbo.CuratedPackageVersions", "PackageKey", "dbo.Packages");
            DropForeignKey("dbo.CuratedPackageVersions", "PackageRegistrationKey", "dbo.PackageRegistrations");
            DropForeignKey("dbo.CuratedPackageVersions", "CuratedFeedKey", "dbo.CuratedFeeds");
            DropTable("dbo.CuratedPackageVersions");
        }
    }
}
