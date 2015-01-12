using System.Collections.Generic;
using System.Web.Http;
using Hal.Attributes;
using Hal.Models;
using Newtonsoft.Json;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class CardHoldersController : ApiController
    {
        [CacheClient(Duration = 30)]
        public CardHolderRepresentation Get(int id)
        {
            var rep = new CardHolderRepresentation
            {
                Id = id,
                Name = "Customer Number" + id,
                Card = new Card() { Number = 101, Type = "mastercard" },
                AnotherCard = new AnotherCard() { IdAgain = "again"}
            };

            return rep;
        }

        public void Post([FromBody]string value)
        {
            var temp = 123;
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }

        [CacheClient(Duration = 30)]
        public CardHoldersRepresentation Get()
        {
            var cardHolders = new CardHoldersRepresentation();
            cardHolders.Links.Add(LinkTemplates.RegisterCardHolder.Register);

            return cardHolders;
        }
    }

    public class CardHoldersRepresentation : Representation
    {
        public override string Href
        {
            get { return LinkTemplates.CardHolders.AllCardHolders.Href; }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.CardHolders.CardHolder);
        }
    }

    public class CardHolderRepresentation : Representation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public AnotherCard AnotherCard { get; set; }

        public Card Card { get; set; }

        public override string Rel { get { return LinkTemplates.CardHolders.CardHolder.Rel; } }
        public override string Href { get { return LinkTemplates.CardHolders.CardHolder.CreateLink(new {id = Id}).Href; } }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.UpdateCardHolder.Update);
        }
    }

    public class CardHolder
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AnotherCard
    {
        public string IdAgain { get; set; }
    }
}
