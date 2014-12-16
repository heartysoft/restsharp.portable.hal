using System.Web.Http;
using Hal.Models;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class CardController : ApiController
    {
        // GET api/dog/5
        public Card Get(int id)
        {
            return new Card() { Number = id };
        }

        public void Post(Card card)
        {
            var temp = card;
        }
    }

    public class Card : Representation
    {

        public int Number { get; set; }
        public string Type { get; set; }

        public override string Rel { get { return LinkTemplates.Cards.Card.Rel; } }
        public override string Href { get { return LinkTemplates.Cards.Card.CreateLink(new { id = Number }).Href; } }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.LoadCards.LoadCard);
        }
    }
}
