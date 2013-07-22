using System;
using System.Web.Mvc;
using GoogleAnalyticsTracker;
using NuGetGallery.Helpers;

namespace NuGetGallery.Filters
{
  public class ActionTrackingAttributeMvc : GoogleAnalyticsTracker.Web.Mvc.ActionTrackingAttribute
  {
    public ActionTrackingAttributeMvc(Tracker tracker, Func<ActionDescriptor, bool> isTrackableAction)
      : base(tracker, isTrackableAction) { }

    public override string BuildCurrentActionUrl(ActionExecutingContext filterContext)
    {
      var request = filterContext.RequestContext.HttpContext.Request;

      return ActionUrl ??
         (request.Url != null ? request.Url.PathAndQuery : "");
    }

    public override void OnTrackingAction(ActionExecutingContext filterContext)
    {
      Tracker.TrackPageView(
        filterContext.RequestContext.HttpContext.Request,
        BuildCurrentActionName(filterContext),
        BuildCurrentActionUrl(filterContext));
    }
  }
}