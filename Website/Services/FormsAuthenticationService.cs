using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;

namespace NuGetGallery
{
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        private readonly IConfiguration _configuration;

        public FormsAuthenticationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private const string ForceSSLCookieName = "ForceSSL";

        protected virtual string CookieName
        {
          get { return FormsAuthentication.FormsCookieName; }
        }

        public HttpCookie GetAuthCookie()
        {
          var request = HttpContext.Current.Request;
          return request.Cookies[CookieName];
        }

        public void SetAuthCookie(
            string userName,
            bool createPersistentCookie,
            IEnumerable<string> roles)
        {
            string formattedRoles = String.Empty;
            if (roles.AnySafe())
            {
                formattedRoles = String.Join("|", roles);
            }

            HttpContext context = HttpContext.Current;

            var originalCookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie);
            var originalTicket = FormsAuthentication.Decrypt(originalCookie.Value);
            var ticket = originalTicket == null ?
              new FormsAuthenticationTicket(
                version: 1,
                name: userName,
                issueDate: DateTime.UtcNow,
                expiration: DateTime.UtcNow.AddMinutes(30),
                isPersistent: createPersistentCookie,
                userData: formattedRoles
                ) :
              new FormsAuthenticationTicket(
                version: originalTicket.Version,
                name: originalTicket.Name,
                issueDate: originalTicket.IssueDate,
                expiration: originalTicket.Expiration,
                isPersistent: originalTicket.IsPersistent,
                userData: formattedRoles,
                cookiePath: originalTicket.CookiePath
                );
            
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var formsCookie = originalCookie;
            formsCookie.Name = CookieName;
            formsCookie.Value = encryptedTicket;
            context.Response.Cookies.Add(formsCookie);

            if (_configuration.RequireSSL)
            {
                // Drop a second cookie indicating that the user is logged in via SSL (no secret data, just tells us to redirect them to SSL)
                context.Response.Cookies.Add(new HttpCookie(ForceSSLCookieName, "true"));
            }
        }

        protected virtual void ClearAuthCookie()
        {
          // Delete the "LoggedIn" cookie
          HttpContext context = HttpContext.Current;
          var cookie = context.Request.Cookies[ForceSSLCookieName];
          if (cookie != null)
          {
            cookie.Expires = DateTime.Now.AddDays(-1d);
            context.Response.Cookies.Add(cookie);
          }
        }

        public virtual void SignOut()
        {
            FormsAuthentication.SignOut();
            ClearAuthCookie();
        }


        public bool ShouldForceSSL(HttpContextBase context)
        {
            var cookie = context.Request.Cookies[ForceSSLCookieName];
            
            bool value;
            if (cookie != null && Boolean.TryParse(cookie.Value, out value))
            {
                return value;
            }
            
            return false;
        }
    }
}