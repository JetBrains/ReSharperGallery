using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuGetGallery.Filters
{
  public class GoogleAnalyticsMeasurementProtocol : ActionFilterAttribute
  {
    private static readonly Uri GA_ENDPOINT = new Uri("https://www.google-analytics.com/collect");

    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
      var gaPropertyId = ConfigurationManager.AppSettings["Gallery.GoogleAnalyticsPropertyId"];
      if (string.IsNullOrEmpty(gaPropertyId))
        return;
      var webClient = new WebClient
      {
        Headers =
        {
          { HttpRequestHeader.UserAgent, filterContext.HttpContext.Request.UserAgent },
          { "X-Forwarded-For", filterContext.HttpContext.Request.UserHostAddress }
        }
      };

      var baseParameters = new NameValueCollection
      {
        { "v",   "1" },
        { "tid", gaPropertyId },
        { "cid", filterContext.HttpContext.Request.Headers["X-NuGet-ApiKey"] ?? filterContext.HttpContext.Request.Headers["NuGet-ApiKey"]  ?? Guid.NewGuid().ToString() },
        { "ni",  "1" },
      };
      
      var viewParameters = new NameValueCollection(baseParameters)
      {
        { "t",  "pageview" },
        { "dl", filterContext.HttpContext.Request.RawUrl },
      };
      webClient.UploadValuesAsync(GA_ENDPOINT, viewParameters);

      var packageId = string.Join(",", 
        new [] { filterContext.RouteData.Values["id"], filterContext.RouteData.Values["version"] }
          .Select(_ => _ != null)
          .Select(_ => Convert.ToString(_)));

      var eventParameters = new NameValueCollection(baseParameters)
      {
        { "t",  "event" },
        { "ec", "NuGet" },
        { "ea", filterContext.HttpContext.Request.Headers["NuGet-Operation"] ?? "Unknown" },
        { "ev", packageId },
      };
      webClient.UploadValuesAsync(GA_ENDPOINT, eventParameters);
    }
  }
}