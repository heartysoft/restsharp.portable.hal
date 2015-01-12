using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CacheCow.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApi.Hal;

namespace Hal
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var jsonHalMediaTypeFormatter = new JsonHalMediaTypeFormatter();
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/hal+json"));
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            jsonHalMediaTypeFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            jsonHalMediaTypeFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            GlobalConfiguration.Configuration.Formatters.Add(jsonHalMediaTypeFormatter);
            GlobalConfiguration.Configuration.Formatters.Add(new XmlHalMediaTypeFormatter());

            //GlobalConfiguration.Configuration.MessageHandlers.Add(new CachingHandler(GlobalConfiguration.Configuration));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

}
