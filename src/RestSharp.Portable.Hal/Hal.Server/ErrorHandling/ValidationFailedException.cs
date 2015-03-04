using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Hal.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hal.ErrorHandling
{
    public class ValidationFailedException : Exception
    {
        private readonly IEnumerable<ValidationError> _errors;

        public ValidationFailedException(string message, IEnumerable<ValidationError> errors) : base(message)
        {
            _errors = errors;
        }

        public void SetResponse(HttpActionExecutedContext context)
        {
            //TODO: Find a way to content negitiate this. It seems it DOESN'T use the json hal media type formatter for some reason, hence stringifying here.
            context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest);
            context.Response.Content = new StringContent(getJson());
            context.Response.ReasonPhrase = "Validation Failed";
        }

        private string getJson()
        {
            return JsonConvert.SerializeObject(new ValidationFailureDto(Message, _errors.ToArray()), new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
        }

        public class ValidationFailureDto
        {
            public string Type { get { return "validation"; } }
            public string Message { get; private set; }
            public Dictionary<string, List<string>> Errors { get; private set; }

            public ValidationFailureDto(string message, IEnumerable<ValidationError> errors)
            {
                Message = message;
                Errors = errors.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Select(y => y.Message).ToList());
            }
        }
    }
}