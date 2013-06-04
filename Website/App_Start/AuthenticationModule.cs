using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using NuGetGallery;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AuthenticationModule), "Start")]

namespace NuGetGallery
{
    public class AuthenticationModule : IHttpModule
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

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(AuthenticationModule));
        }

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = _formsAuthenticationService.GetAuthCookie();
            if (authCookie == null) return;
            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            if (authTicket == null) return;
            var context = HttpContext.Current;
            var identity = context.Request.IsAuthenticated ? context.User.Identity : new GenericIdentity(authTicket.Name);
            var roles = authTicket.UserData.Split(new [] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var user = new GenericPrincipal(identity, roles);
            context.User = Thread.CurrentPrincipal = user;
        }
    }
}