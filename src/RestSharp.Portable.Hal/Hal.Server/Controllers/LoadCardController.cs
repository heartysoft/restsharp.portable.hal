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
    public class LoadCardController : ApiController
    {
        public LoadCardForm Get()
        {
            return new LoadCardForm{ Currency = "GBP", Amount = 100M};
        }
    }

    public class LoadCardForm : Representation
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public override string Rel { get { return LinkTemplates.LoadCards.LoadCard.Rel; } }
        public override string Href { get { return LinkTemplates.LoadCards.LoadCard.CreateLink().Href; } }

        protected override void CreateHypermedia()
        {

        }
    }
}
