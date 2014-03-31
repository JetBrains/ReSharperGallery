using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Owin.Security.JetPass;
using Microsoft.Owin.Security;

namespace NuGetGallery.Authentication.Providers.JetPass
{
    public class JetPassAuthenticatorConfiguration : AuthenticatorConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RootUri { get; set; }

        public JetPassAuthenticatorConfiguration()
        {
            AuthenticationType = JetPassAuthenticator.DefaultAuthenticationType;
        }

        public override void ApplyToOwinSecurityOptions(AuthenticationOptions options)
        {
            base.ApplyToOwinSecurityOptions(options);

            var opts = options as JetPassAuthenticationOptions;
            if (opts != null)
            {
                if (String.IsNullOrEmpty(ClientId))
                {
                    throw new ConfigurationErrorsException(String.Format(
                        CultureInfo.CurrentCulture,
                        Strings.MissingRequiredConfigurationValue,
                        "Auth.JetPass.ClientId"));
                }

                opts.ClientId = ClientId;
                opts.Scope = new []{ ClientId };

                if (String.IsNullOrEmpty(ClientSecret))
                {
                    throw new ConfigurationErrorsException(String.Format(
                        CultureInfo.CurrentCulture,
                        Strings.MissingRequiredConfigurationValue,
                        "Auth.JetPass.ClientSecret"));
                }

                opts.ClientSecret = ClientSecret;

                if (!String.IsNullOrEmpty(RootUri))
                {
                  opts.Endpoints = new JetPassAuthenticationOptions.JetPassAuthenticationEndpoints(new Uri(RootUri));
                }
            }
        }
    }
}
