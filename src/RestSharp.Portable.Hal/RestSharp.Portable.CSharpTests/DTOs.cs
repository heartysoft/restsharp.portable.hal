using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.CSharpTests
{
    public class RegistrationForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Card
    {
        public string IdAgain { get; set; }
    }

    public class CardHolderDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Card AnotherCard { get; set; }
    }
}
