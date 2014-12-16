using WebApi.Hal;

namespace Hal.Models
{
    public static class LinkTemplates
    {
        public static class CardHolders
        {
            public static Link AllCardHolders { get { return new Link("allCardHolders", "/api/cardholders/"); } }
            public static Link CardHolder { get { return new Link("cardHolder", "/api/CardHolders/{id}"); } }
        }

        public static class UpdateCardHolder
        {
            public static Link Update { get { return new Link("updateCardHolder", "/api/updatecardholder");} }
        }

        public static class RegisterCardHolder
        {
            public static Link Register { get { return new Link("register", "/api/registercardholder");} }
        }

        public static class Cards
        {
            public static Link Card { get { return new Link("card", "/api/card/{id}");} }
        }

        public static class LoadCards
        {
            public static Link LoadCard { get { return new Link("loadCard", "/api/loadCard"); } }
        }
    }
}