using System.Collections.Generic;
using System.Web.Http;
using Hal.Models;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class CardHoldersController : ApiController
    {
        // GET api/cardholders/5
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

        // POST api/cardholders
        public void Post([FromBody]string value)
        {
            var temp = 123;
        }

        // PUT api/cardholders/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/cardholders/5
        public void Delete(int id)
        {
        }

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
        public override string Href { get { return LinkTemplates.CardHolders.CardHolder.CreateLink().Href; } }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.CardHolders.CardHolder);
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
