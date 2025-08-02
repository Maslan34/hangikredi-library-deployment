using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        // Relationship: OneToMany
        [JsonIgnore]
        public ICollection<Book>? Books { get; set; }
    }
}
