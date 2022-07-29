using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TrailerDownloader.Models.DTOs
{
    [DataContract]
    public class TMDBInfoResDto
    {
        [DataMember(Name = "results")]
        public IEnumerable<TMDBMovieDto> Movies { get; set; }
    }

    [DataContract]
    public class TMDBMovieDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
        
        [DataMember(Name = "poster_path")]
        public string PosterPath { get; set; }
    }
}