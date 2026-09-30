using Newtonsoft.Json;

namespace Dto
{
    public class AribaActionRequestDto
    {
        [JsonProperty("actionableType")]
        public string ActionableType { get; set; } = "Task";  

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; } 

        [JsonProperty("actionName")]
        public string ActionName { get; set; }  

        [JsonProperty("options")]
        public AribaActionOptions Options { get; set; } = new AribaActionOptions();
    }

    public class AribaActionOptions
    {
        [JsonProperty("comment")]
        public string Comment { get; set; } 
    }
}
