using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using CacheCow.Server;
using Hal.Controllers;
using Hal.ErrorHandling;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using WebApi.Hal;

[assembly: OwinStartup(typeof(Hal.Startup))]

namespace Hal
{
    public class Startup
    {
        /// <summary>
        /// Enable for testing, or hosting without setting cache in global.asax.cs
        /// </summary>
        protected virtual bool RegisterCachingHandler {get { return false; }}
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            var jsonHalMediaTypeFormatter = new JsonHalMediaTypeFormatter();
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xhtml"));
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/hal+json"));
            jsonHalMediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            jsonHalMediaTypeFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            jsonHalMediaTypeFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            config.Formatters.Add(jsonHalMediaTypeFormatter);
            config.Formatters.Add(new XmlHalMediaTypeFormatter());

            //config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Include;

            if (RegisterCachingHandler)
            {
                config.MessageHandlers.Add(new CachingHandler(config));
            }

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            config.Filters.Add(new ValidationFilter());

            app.UseWebApi(config);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
