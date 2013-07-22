using System.Net;
using System.Net.Http;
using System.Web;
using GoogleAnalyticsTracker;

namespace NuGetGallery.Helpers
{
  public static class TrackerHelper
  {
    public static void TrackPageView(this Tracker tracker, HttpRequestBase request, string pageTitle, string pageUrl)
    {
      tracker.Hostname = request.Url != null ? request.Url.Host : Dns.GetHostName();
      tracker.UserAgent = request.UserAgent;
      tracker.Language = request.UserLanguages != null ? string.Join(";", request.UserLanguages) : "";
      tracker.TrackPageView(pageTitle, pageUrl);
    }

    public static void TrackPageView(this Tracker tracker, HttpRequestMessage request, string pageTitle, string pageUrl)
    {
      tracker.Hostname = request.RequestUri != null ? request.RequestUri.Host : Dns.GetHostName();
      tracker.UserAgent = string.Join(" ", request.Headers.UserAgent);
      tracker.Language = request.Headers.AcceptLanguage != null ? string.Join(";", request.Headers.AcceptLanguage) : "";
      tracker.TrackPageView(pageTitle, pageUrl);
    }
  }
}