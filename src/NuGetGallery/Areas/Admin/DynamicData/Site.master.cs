using System;
using System.Net;
using System.Web.UI;
using Ninject;
using NuGetGallery;
using NuGetGallery.Configuration;

namespace NuGetGallery.Areas.Admin.DynamicData
{
    public partial class Site : MasterPage
    {
        protected IAppConfiguration Config;

        protected override void OnInit(EventArgs e)
        {
            // Cheap and easy DI. Not too clean :)
            Config = NuGetGallery.Container.Kernel.Get<AppConfiguration>();

            if (!Page.User.Identity.IsAuthenticated)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.End();
            }

            if (!Request.IsLocal && !Page.User.IsAdministrator())
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.End();
            }

            base.OnInit(e);
        }
    }
}