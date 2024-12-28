using Common.Entities;

namespace Web.ViewModels.Books
{
    public class AddBookReview
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Authors { get; set; }
        public List<Review> Reviews { get; set; }
    }

}
