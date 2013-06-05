using System;
using System.Web;
using DotNetCasClient;
using DotNetCasClient.Utils;

namespace NuGetGallery
{
  public class CasAuthenticationService : FormsAuthenticationService
  {
    public CasAuthenticationService(IConfiguration configuration) : base(configuration)
    {
    }

    protected override string CookieName
    {
      get { return base.CookieName + "_CAS"; }
    }

    protected override void ClearAuthCookie()
    {
      // Delete the "LoggedIn" cookie
      HttpContext context = HttpContext.Current;
      var cookie = context.Request.Cookies[CookieName];
      if (cookie != null)
      {
        cookie.Expires = DateTime.Now.AddDays(-1d);
        context.Response.Cookies.Add(cookie);
      }

      CasAuthentication.ClearAuthCookie();
      base.ClearAuthCookie();
    }

    private const string OldSignOutServiceParameter = "TARGET";
    private const string NewSignOutServiceParameter = "service";

    public override void SignOut()
    {
      HttpContext context = HttpContext.Current;
      if (context.Request.IsAuthenticated)
      {
        var singleSignOutRedirectUrl = new  EnhancedUriBuilder(UrlUtil.ConstructSingleSignOutRedirectUrl());
        var returnUrl = singleSignOutRedirectUrl.QueryItems[OldSignOutServiceParameter];
        singleSignOutRedirectUrl.QueryItems.Remove(OldSignOutServiceParameter);
        singleSignOutRedirectUrl.QueryItems[NewSignOutServiceParameter] = returnUrl;

        ClearAuthCookie();
        context.Response.Redirect(singleSignOutRedirectUrl.Uri.AbsoluteUri, true);
        return;
      }

      base.SignOut();
    }
  }
}