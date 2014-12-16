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
    public class UpdateCardHolderController : ApiController
    {
        public UpdateCardHolderForm Get()
        {
            return new UpdateCardHolderForm();
        }

        public void Post(UpdateCardHolderForm form)
        {
            var temp = form;
        }
    }

    public class UpdateCardHolderForm : Representation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string Rel { get { return LinkTemplates.UpdateCardHolder.Update.Rel; } }
        public override string Href { get { return LinkTemplates.UpdateCardHolder.Update.CreateLink().Href; } }

        protected override void CreateHypermedia()
        {
            
        }
    }
    
}
