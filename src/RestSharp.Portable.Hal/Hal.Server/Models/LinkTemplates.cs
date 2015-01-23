using WebApi.Hal;

namespace Hal.Models
{
    public static class LinkTemplates
    {
        public static class CardHolders
        {
            public static Link AllCardHolders { get { return new Link("cardholders", "/api/cardholders"); } }
            public static Link CardHolder { get { return new Link("cardholder", "/api/cardholders/{id}"); } }
        }

        public static class UpdateCardHolder
        {
            public static Link Update { get { return new Link("updatecardholder", "/api/updatecardholder");} }
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
            public static Link LoadCard { get { return new Link("loadcard", "/api/loadCard"); } }
        }

        public static class SearchLinks
        {
            public static Link Search { get { return new Link("search", "/api/search?&name={name}&id={id}&cardNumber={cardNumber}");} }
        }
    }
}