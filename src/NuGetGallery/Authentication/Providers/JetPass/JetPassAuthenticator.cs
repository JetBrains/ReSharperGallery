using System.Web.Mvc;
using JetBrains.Owin.Security.JetPass;
using NuGetGallery.Configuration;
using Owin;

namespace NuGetGallery.Authentication.Providers.JetPass
{
    public class JetPassAuthenticator : Authenticator<JetPassAuthenticatorConfiguration>
    {
        public static readonly string DefaultAuthenticationType = "JetPass";

        protected override void AttachToOwinApp(ConfigurationService config, IAppBuilder app)
        {
            var options = new JetPassAuthenticationOptions();
            Config.ApplyToOwinSecurityOptions(options);
            app.UseJetPassAuthentication(options);
        }

        public override AuthenticatorUI GetUI()
        {
            return new AuthenticatorUI(
                "Sign in with a JetBrains Account",
                "JetBrains Account",
                "JetBrains Account")
                {
                    IconCssClass = "nucon-jb-w"
                };
        }

        public override ActionResult Challenge(string redirectUrl)
        {
            return new ChallengeResult(BaseConfig.AuthenticationType, redirectUrl);
        }
    }
}