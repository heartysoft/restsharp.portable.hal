using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Hal.Models;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class RegisterCardHolderController : ApiController
    {
        public RegistrationForm Get()
        {
            return new RegistrationForm()  {Id = -1, Name = "Enter name here"};
        }

        public HttpResponseMessage Post(RegistrationForm form)
        {
            //Create new card holder

            var newCardHolder = new CardHolder() {Id = form.Id, Name = form.Name};

            var response = Request.CreateResponse(HttpStatusCode.Created, new CardHolderRepresentation()
            {
                AnotherCard = new AnotherCard() {IdAgain = "lala"},
                Name = form.Name,
                Id = form.Id
            });

            response.Headers.Location = LinkTemplates.CardHolders.CardHolder.CreateUri(new {id = newCardHolder.Id});

            return response;
        }
    }


    public class RegistrationForm : Representation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string Rel { get { return LinkTemplates.RegisterCardHolder.Register.Rel; } }
        public override string Href { get { return LinkTemplates.RegisterCardHolder.Register.CreateLink().Href; } }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.RegisterCardHolder.Register);
        }
    }
}
