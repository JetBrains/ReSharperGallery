// <auto-generated />
namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    public sealed partial class AddExplicitCuratedPackageVersionEntity : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddExplicitCuratedPackageVersionEntity));
        
        string IMigrationMetadata.Id
        {
            get { return "201506051158568_AddExplicitCuratedPackageVersionEntity"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}