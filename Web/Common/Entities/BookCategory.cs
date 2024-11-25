namespace Common.Entities
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public int CategoryId { get; set; }


        public Book? Books { get; set; }
        public Category? Categories { get; set; }
    }
}
