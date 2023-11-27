using System.Text.Json.Serialization;

namespace SharedLibrary
{
    public class Driver
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
