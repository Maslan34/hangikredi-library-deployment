using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Biography { get; set; }

        // Relationship: One To Many
        [JsonIgnore]
        public ICollection<Book>? Books { get; set; }
    }
}
