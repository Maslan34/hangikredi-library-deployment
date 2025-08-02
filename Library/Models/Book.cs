using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        public int AuthorId { get; set; }

        public Author? Author { get; set; }  

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }



}
