using System;
using System.Web.Mvc;
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
  }
}