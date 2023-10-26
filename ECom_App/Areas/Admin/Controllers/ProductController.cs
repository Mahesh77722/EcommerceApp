
using ECom.Models.ViewModels;
using ECom.DataAccess.Data;
using ECom.DataAccess.Repository.IRepository;
using ECom.Models;
using ECom.Models.ViewModels;
using ECom.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Ecom.Utility;

namespace ECom_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.ADMIN)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _service;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork service, IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            try
            {
                var products = _service.Product.GetAll(includeProperties: "category").ToList();

                return View(products);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        public IActionResult UpsertCreate()
        {
            try
            {
                ProductVM productVM = new ProductVM()
                {
                    categoryList = _service.Category.GetAll(includeProperties: "category").Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),
                    product = new Product()
                };
                return View(productVM);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Create(ProductVM obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _service.Product.Add(obj.product);
                    _service.Save();
                    TempData["msg"] = "Product " + Alerts.SUCCESS;
                    return RedirectToAction("Index");
                }
                else
                {
                    obj.categoryList = _service.Category.GetAll(includeProperties: "category").Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                    TempData["msgError"] = "Product " + Alerts.FAILSUCCESS;

                }
                return View(obj);
            }
            catch (Exception)
            {
                return View("Error");
            }

        }
        public IActionResult Upsert(int? Id)
        {
            try
            {

                ProductVM productVM = new ProductVM()
                {
                    categoryList = _service.Category.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),
                    product = new Product()
                };
                if (Id != null)
                {
                    productVM.product = _service.Product.FirstOrDefault(x => x.Id == Id, includeProperties: "ProductImages");
                }
                return View(productVM);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (productVM.product.Id == 0)
                    {
                        _service.Product.Add(productVM.product);
                        TempData["msg"] = "Product " + Alerts.SUCCESS;
                    }
                    else
                    {
                        _service.Product.Update(productVM.product);
                        TempData["msg"] = "Product " + Alerts.UPDATE;
                    }
                    _service.Save();
                    string rootPath = _webHostEnvironment.WebRootPath;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var ProductPath = @"Images\Products\Product-" + productVM.product.Id;
                            var finalPath = Path.Combine(rootPath, ProductPath);
                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }

                            using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            ProductImage image = new ProductImage()
                            {
                                ProductId = productVM.product.Id,
                                ImageURL = @"\" + ProductPath + @"\" + fileName
                            };

                            if (productVM.product.ProductImages == null)
                            {
                                productVM.product.ProductImages = new List<ProductImage>();
                            }

                            productVM.product.ProductImages.Add(image);
                        }
                        _service.Product.Update(productVM.product);
                        _service.Save();
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.categoryList = _service.Category.GetAll(includeProperties: "category").Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });
                    TempData["msgError"] = "Product " + Alerts.FAILSUCCESS;

                }
                return View(productVM);
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
                Product? product = _service.Product.FirstOrDefault(x => x.Id == Id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _service.Product.Update(product);
                    _service.Save();
                    TempData["msg"] = "Product " + Alerts.UPDATE;
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
                Product? product = _service.Product.FirstOrDefault(x => x.Id == Id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
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
                Product? product = _service.Product.FirstOrDefault(x => x.Id == Id); //Work only with Any Column
                if (product == null)
                {
                    return NotFound();
                }
                _service.Product.Remove(product);
                _service.Save();
                TempData["msg"] = "Product " + Alerts.DELETE;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public IActionResult DeleteImage(int ImageId)
        {
            try
            {
                int ProductId = 0;
                ProductImage? productImageToBeDelete = _service.ProductImage.FirstOrDefault(x => x.Id == ImageId); //Work only with Any Column
                if (productImageToBeDelete != null)
                {
                    ProductId = productImageToBeDelete.ProductId;

                    if (!string.IsNullOrEmpty(productImageToBeDelete.ImageURL))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, productImageToBeDelete.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }
                _service.ProductImage.Remove(productImageToBeDelete);
                _service.Save();
                TempData["msg"] = "Product Image" + Alerts.DELETE;
                return RedirectToAction(nameof(Upsert), new { Id = ProductId });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        #region APISCALL
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var products = _service.Product.GetAll(includeProperties: "category").ToList();

                return Json(new { data = products });
            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while Get all product" });
            }
        }
        [HttpDelete]
        public IActionResult DeleteProduct(int? Id)
        {
            try
            {
                var productToBeDeleted = _service.Product.FirstOrDefault(u => u.Id == Id);
                if (productToBeDeleted == null)
                {
                    return Json(new { sucess = false, message = "Error while deleting product" });
                }
                //var oldImagePath= Path.Combine(_webHostEnvironment.ContentRootPath, productToBeDeleted.ImgURL.TrimStart('\\'));
                //if (System.IO.File.Exists(oldImagePath))
                //{
                //    System.IO.File.Delete(oldImagePath);
                //}

                var ProductPath = @"Images\Products\Product-" + Id;
                var finalPath = Path.Combine(_webHostEnvironment.WebRootPath, ProductPath);
                if (Directory.Exists(finalPath))
                {
                    string[] filesPath = Directory.GetFiles(finalPath);
                    foreach (string filePath in filesPath)
                    {
                        System.IO.File.Delete(filePath);
                    }
                    Directory.Delete(finalPath);
                }


                _service.Product.Remove(productToBeDeleted);
                _service.Save();
                return Json(new { sucess = true, message = "Product " + Alerts.DELETE });

            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while deleting product" });
            }
        }
        #endregion

    }
}
