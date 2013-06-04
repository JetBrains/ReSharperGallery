using System;
using System.Web;
using DotNetCasClient;

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

    public override void SignOut()
    {
      // Delete the "LoggedIn" cookie
      HttpContext context = HttpContext.Current;
      var cookie = context.Request.Cookies[CookieName];
      if (cookie != null)
      {
        cookie.Expires = DateTime.Now.AddDays(-1d);
        context.Response.Cookies.Add(cookie);
      }

      base.SignOut();
      CasAuthentication.SingleSignOut();
    }
  }
}