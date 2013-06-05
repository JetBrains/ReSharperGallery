using System;
using System.Web.Mvc;
using System.Web.Security;
using DotNetCasClient;
using DotNetCasClient.Utils;
using Ninject;

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

      var ticketManager = CasAuthentication.ServiceTicketManager;
      if (ticketManager != null)
      {
        var formsAuthenticationTicket = CasAuthentication.GetFormsAuthenticationTicket();
        if (formsAuthenticationTicket != null)
        {
          var serviceTicket = formsAuthenticationTicket.UserData;
          if (!string.IsNullOrEmpty(serviceTicket))
          {
            var ticket = ticketManager.GetTicket(serviceTicket);
            if (ticket != null)
            {
              User user = null;
              string emailAddress = null;
              var userService = Container.Kernel.Get<IUserService>();
              foreach (var email in ticket.Assertion.Attributes["mail"])
              {
                emailAddress = email;
                user = userService.FindByEmailAddress(emailAddress);
                if (user != null) break;
              }
              if (user == null)
              {
                user = userService.Create(formsAuthenticationTicket.Name, Guid.NewGuid().ToString(), emailAddress);
                userService.ConfirmEmailAddress(user, user.EmailConfirmationToken);
              }
            }
          }
        }
      }

      return SafeRedirect(returnUrl);
    }

    private const string OldSignOutServiceParameter = "TARGET";
    private const string NewSignOutServiceParameter = "service";

    public override ActionResult LogOff(string returnUrl)
    {
      if (Request.IsAuthenticated)
      {
        var singleSignOutRedirectUrl = new  EnhancedUriBuilder(UrlUtil.ConstructSingleSignOutRedirectUrl());
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