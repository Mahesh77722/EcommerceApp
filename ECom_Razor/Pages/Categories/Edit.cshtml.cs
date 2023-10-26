using ECom_Razor.Data;
using ECom_Razor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECom_Razor.Pages.Categories
{
    [BindProperties]
    public class EditModel : PageModel
    {
        public Category Category { get; set; }
        private readonly RazorDbContext _db;

        public EditModel(RazorDbContext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            try
            {
                if (id != 0 || id != null)
                {
                    Category = _db.Categories.FirstOrDefault(c => c.Id == id);
                }
            }
            catch (Exception ex)
            {
            }

        }
        public IActionResult OnPost()
        {
            try
            {
                if (Category != null)
                {
                    _db.Categories.Update(Category);
                    _db.SaveChanges();
                    TempData["msg"] = Alert.UPDATE;

                }

            }
            catch (Exception ex)
            {
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}
