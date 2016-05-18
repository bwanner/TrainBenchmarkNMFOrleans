using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.ApplicationInsights;
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

    public class AiExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {//or reuse instance (recommended!). see note above 
                var ai = new TelemetryClient();
                ai.TrackException(context.Exception);
            }
            base.Log(context);
        }
    }
}
