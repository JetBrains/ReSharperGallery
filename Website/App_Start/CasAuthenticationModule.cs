using System;
using System.Linq;
using System.Web;
using DotNetCasClient;
using Ninject;

namespace NuGetGallery
{
    public class CasAuthenticationModule : IHttpModule
    {
        private IFormsAuthenticationService _formsAuthenticationService;

        public void Init(HttpApplication context)
        {
            _formsAuthenticationService = Container.Kernel.Get<IFormsAuthenticationService>();
            context.AuthenticateRequest += OnAuthenticateRequest;
        }

        public void Dispose()
        {
        }

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
          if (!HttpContext.Current.Request.IsAuthenticated) return;
          var authCookie = _formsAuthenticationService.GetAuthCookie();
          if (authCookie != null) return;
          var ticketManager = CasAuthentication.ServiceTicketManager;
          if (ticketManager == null) return;
          var formsAuthenticationTicket = CasAuthentication.GetFormsAuthenticationTicket();
          if (formsAuthenticationTicket == null) return;
          var serviceTicket = formsAuthenticationTicket.UserData;
          if (string.IsNullOrEmpty(serviceTicket)) return;
          var ticket = ticketManager.GetTicket(serviceTicket);
          if (ticket == null) return;
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
          _formsAuthenticationService.SetAuthCookie(user.Username, true, user.Roles.Select(_ => _.Name));
        }
    }
}