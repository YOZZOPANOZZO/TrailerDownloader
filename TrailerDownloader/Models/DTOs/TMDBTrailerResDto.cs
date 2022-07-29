using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TrailerDownloader.Models.DTOs
{
    [DataContract]
    public class TMDBTrailerResDto
    {
        [DataMember(Name = "results")]
        public IEnumerable<TMDBTrilerDto> Trailers { get; set; }
    }

    public class TMDBTrilerDto
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }
        
        [DataMember(Name = "site")]
        public string Site { get; set; }
        
        [DataMember(Name = "type")] 
        public string Type { get; set; }
    }
}