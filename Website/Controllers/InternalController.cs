using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NuGetGallery.Filters;

namespace NuGetGallery.Controllers
{
  [GoogleAnalyticsMeasurementProtocol]
  public class InternalController : System.Web.Http.ApiController
  {
    private readonly IPackageService _packageService;

    public InternalController(IPackageService packageService)
    {
      _packageService = packageService;
    }

    [System.Web.Http.HttpGet]
    public IEnumerable<object> Packages(bool includePrerelease = false)
    {
      return _packageService
        .GetPackagesForListing(includePrerelease)
        .Select(_ => new
        {
          _.PackageRegistration.Id,
          _.Title,
          _.Description,
          Owners = _.PackageRegistration.Owners.Select(__ => __.Username),
          Authors = _.Authors.Select(__ => __.Name),
          Dependencies = _.Dependencies.Select(__ => new { product = __.Id, version = __.VersionSpec }),
          _.Tags,
          _.LastUpdated,
          _.Version,
          _.PackageRegistration.DownloadCount,
          _.ProjectUrl,
          _.LicenseUrl,
          _.IconUrl
        })
        .AsEnumerable()
        .Select(_ => new
        {
          Name = string.IsNullOrEmpty(_.Title) ? _.Id : _.Title,
          Description = _.Description,
          Owners = _.Owners.Select(__ => new { owner = __, url = Url.Link(null, MVC.Users.Profiles(__).GetRouteValueDictionary()) }),
          Authors = _.Authors.Select(__ => new { author = __ }),
          Compatible_versions = _.Dependencies,
          Tags = (_.Tags ?? "").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(__ => new { tag = __ }),
          Last_update = _.LastUpdated,
          Last_version = _.Version,
          Downloads = _.DownloadCount,
          URLs = new { project = _.ProjectUrl, license = _.LicenseUrl, contact = Url.Link(null, MVC.Packages.ContactOwners(_.Id).GetRouteValueDictionary()) },
          Logo = _.IconUrl
        });
    }
  }
}