using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Controllers.Performans.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }

        public List<string> Possible { get; set; } = new List<string>();
        public bool HasDetails { get; set; }
        public string DefaultTimeType { get; set; }

        public Card()
        {

        }

        //public Card(string id, string text, string value, string type)
        //{
        //    this.Id    = id;
        //    this.Text  = text;
        //    this.Value = value;
        //    this.Type  = type;
        //}
    }
}
