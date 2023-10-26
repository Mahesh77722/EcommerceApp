using ECom_Razor.Data;
using ECom_Razor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECom_Razor.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        public Category category { get; set; }
        private readonly RazorDbContext _db;

        public DeleteModel(RazorDbContext db)
        {
            _db = db;
        }
        public void OnGet(int? Id)
        {
            if (Id != null || Id != 0)
            {
                category = _db.Categories.FirstOrDefault(c => c.Id == Id);
            }
        }
        public IActionResult OnPost()
        {
            try
            {
                category =_db.Categories.FirstOrDefault(x=>x.Id == category.Id);
                if (category != null)
                {
                    _db.Categories.Remove(category);
                    _db.SaveChanges();
                    TempData["msg"] = Alert.DELETE;

                }
            }
            catch (Exception)
            {
                throw;
            }
            return RedirectToPage("Index");

        }
    }
}
