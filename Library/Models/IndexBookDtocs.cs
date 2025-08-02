using Microsoft.AspNetCore.Mvc;

namespace Library.Models
{
    public class IndexBookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }

    }

}
