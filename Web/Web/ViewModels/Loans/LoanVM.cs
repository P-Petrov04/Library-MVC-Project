using Common.Entities;

namespace Web.ViewModels.Loans
{
    public class LoanVM
    {
        public LoanVM()
        {
            Items = new List<Loan>();
        }
        public IEnumerable<Loan>? Items { get; set; }
    }
}
