using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadavBookShopDataLoader.Models
{

    public class Book
    {
        [JsonProperty("@id")]
        public string Id { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string genre { get; set; }
        public string price { get; set; }
        public string publish_date { get; set; }
        public string description { get; set; }
    }
}
