using System;
using System.Web.Mvc;
using System.Web.Security;
using DotNetCasClient;
using DotNetCasClient.Utils;

namespace NuGetGallery
{
  public class CasAuthenticationController : AuthenticationController
  {
    public CasAuthenticationController(IFormsAuthenticationService formsAuthService, IUserService userService)
      : base(formsAuthService, userService) { }

    public override ActionResult LogOn(string returnUrl)
    {
      if (!User.Identity.IsAuthenticated)
        return Redirect(UrlUtil.ConstructLoginRedirectUrl(CasAuthentication.Gateway, CasAuthentication.Renew));

      return SafeRedirect(returnUrl);
    }

    private const string OldSignOutServiceParameter = "TARGET";
    private const string NewSignOutServiceParameter = "service";

    public override ActionResult LogOff(string returnUrl)
    {
      if (Request.IsAuthenticated)
      {
        var singleSignOutRedirectUrl = new  EnhancedUriBuilder(new Uri(UrlUtil.ConstructSingleSignOutRedirectUrl()));
        var serviceUrl = singleSignOutRedirectUrl.QueryItems[OldSignOutServiceParameter];
        singleSignOutRedirectUrl.QueryItems.Remove(OldSignOutServiceParameter);
        singleSignOutRedirectUrl.QueryItems[NewSignOutServiceParameter] = serviceUrl;

        Roles.DeleteCookie();
        CasAuthentication.ClearAuthCookie();
        return Redirect(singleSignOutRedirectUrl.Uri.AbsoluteUri);
      }
    
      return base.LogOff(returnUrl);
    }
  }
}