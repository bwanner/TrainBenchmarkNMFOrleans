using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace ClientRole
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //GlobalConfiguration.Configuration.
            if (!AzureClient.IsInitialized)
            {
                var path = HostingEnvironment.MapPath("~/ClientConfiguration.xml");
                AzureClient.Initialize(path);
            }
        }
    }
}
