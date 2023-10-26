using ECom_Razor.Data;
using ECom_Razor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECom_Razor.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly RazorDbContext _db;
        [BindProperty]
        public Category Category { get; set; }

        public CreateModel(RazorDbContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
             
        }
        public IActionResult OnPost(Category category)
        {
            _db.Categories.Add(category);
            _db.SaveChanges();
            TempData["msg"] =Alert.SUCCESS;
            return RedirectToPage("Index");
        }
    }
}
