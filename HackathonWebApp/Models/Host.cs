using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class Host
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [BsonElement("display_name")]
        public string DisplayName { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("organization")]
        public string Organization { get; set; }
    }
}
