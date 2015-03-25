using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Hal.ErrorHandling;
using Hal.Models;
using Microsoft.Owin;
using Newtonsoft.Json;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class WithErrorController : ApiController
    {
        public WithErrorRepresentation Get()
        {
            return new WithErrorRepresentation();
        }

        public WithErrorRepresentation Get(int id)
        {
            if (id == 1)
            {
                throw new MyBusinessLogicException("My business logic exception message");
            }

            throw new ValidationFailedException("Overall message", 
                new[]
                {
                    new ValidationError("Name", "Your name is a bit weird. Are you sure it's Yoda?"),
                    new ValidationError("Name", "Your name must be more than 4 characters...mwahhahahha....evil evil...lolz"),
                    new ValidationError("Age", "Yeah, right. You ain't 350 and I know it.")
                });
        }


        public HttpResponseMessage Post(SomeData data)
        {
            if (data.Id == 1)
            {
                throw new MyBusinessLogicException("My business logic exception message");
            }

            throw new ValidationFailedException("Overall message",
                new[]
                {
                    new ValidationError("Name", "Your name is a bit weird. Are you sure it's Yoda?"),
                    new ValidationError("Name", "Your name must be more than 4 characters...mwahhahahha....evil evil...lolz"),
                    new ValidationError("Age", "Yeah, right. You ain't 350 and I know it.")
                });
        }


        public class MyBusinessLogicException : Exception
        {
            public MyBusinessLogicException(string message) : base(message)
            {
            }            
        }
    }

    public class WithErrorRepresentation : Representation
    {
        public override string Rel { get { return LinkTemplates.WithErrorLinks.Root.Rel; } }
        public override string Href { get { return LinkTemplates.WithErrorLinks.Root.CreateLink().Href; } }

        protected override void CreateHypermedia()
        {
            this.Links.Add(LinkTemplates.WithErrorLinks.Details);
        }
    }

    public class SomeData
    {
        public int Id { get; set; }
    }
}
