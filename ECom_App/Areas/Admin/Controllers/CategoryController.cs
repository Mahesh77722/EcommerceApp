
using ECom.DataAccess.Repository.IRepository;
using ECom.DataAccess.Data;
using ECom.Models;
using ECom.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;
using Ecom.Utility;

namespace ECom_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =Roles.ADMIN)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _Unit;
        public CategoryController(IUnitOfWork Unit)
        {
            _Unit = Unit;
        }
        public IActionResult Index()
        {
            try
            {
                var categories = _Unit.Category.GetAll().ToList();
                return View(categories);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        public IActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult Create(Category newCategory)
        {
            try
            {
                //if (newCategory.Name == newCategory.DisplayOrder.ToString())
                //{
                //    ModelState.AddModelError("name", "Display Order and Name cannot be same");
                //}
                //if (newCategory.Name!=null && newCategory.Name.ToLower() == "test")
                //{
                //    ModelState.AddModelError("", "test cannot be added in Name field");
                //}
                if (ModelState.IsValid)
                {
                    _Unit.Category.Add(newCategory);
                    _Unit.Save();
                    TempData["msg"] = "Category "+Alerts.SUCCESS;
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        public IActionResult Edit(int? Id)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                //Category? category = _context.Categories.Find(Id); //Work only with Primary Key
                Category? category = _Unit.Category.FirstOrDefault(x => x.Id == Id); //Work only with Any Column
                //Category? category2 = _context.Categories.Where(x => x.Id == Id).FirstOrDefault(); //Work only with Any Column but we can write add calculation
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult Edit(Category newCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _Unit.Category.Update(newCategory);
                    _Unit.Save();
                    TempData["msg"] = "Category "+Alerts.UPDATE;
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public IActionResult Delete(int? Id)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                //Category? category = _Category.Categories.Find(Id); //Work only with Primary Key
                Category? category = _Unit.Category.FirstOrDefault(x => x.Id == Id); //Work only with Any Column
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }
                Category? category = _Unit.Category.FirstOrDefault(x => x.Id == Id); //Work only with Any Column
                if (category == null)
                {
                    return NotFound();
                }
                _Unit.Category.Remove(category);
                _Unit.Save();
                TempData["msg"] = "Category "+Alerts.DELETE;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
    }
}
