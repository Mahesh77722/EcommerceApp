using ECom_Razor.Data;
using ECom_Razor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECom_Razor.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly RazorDbContext _db;
        public List<Category> CategoryList { get; set; }
        public IndexModel(RazorDbContext dbContext)
        {
                _db = dbContext;
        }
        public void OnGet()
        {
            CategoryList = _db.Categories.ToList();
        }
    }
}
