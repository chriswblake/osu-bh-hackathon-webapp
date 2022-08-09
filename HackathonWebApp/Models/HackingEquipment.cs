using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    public class HackingEquipment
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }
        
        [BsonElement("unit")]
        public string Unit { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("url_more_info")]
        public string UrlMoreInformation { get; set; }
    }
}
