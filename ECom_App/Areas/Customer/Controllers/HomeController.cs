using ECom.DataAccess.Repository.IRepository;
using ECom.DataAccess.Repository.Repository;
using ECom.Models;
using ECom.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ECom_App.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _service;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _service = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _service.Product.GetAll(includeProperties: "category,ProductImages");

            return View(products);
        }
        public IActionResult detail(int id)
        {
            ShoppingCart cart = new()
            {
                product = _service.Product.FirstOrDefault(w => w.Id == id, includeProperties: "category,ProductImages"),
                Count = 1,
                ProductId = id
            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult detail(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;
            ShoppingCart dbCart = _service.ShoppingCart.FirstOrDefault(x => x.ApplicationUserId == userId && x.ProductId == shoppingCart.ProductId);
            if (dbCart != null)
            {
                dbCart.Count += shoppingCart.Count;
                _service.ShoppingCart.Update(dbCart);
                _service.Save();
                TempData["msg"] = "Shopping cart " + Alerts.UPDATE;

            }
            else
            {
                _service.ShoppingCart.Add(shoppingCart);
                _service.Save();
                HttpContext.Session.SetInt32(Alerts.CARTSESSION, _service.ShoppingCart.GetAll(x => x.ApplicationUserId == userId).Count());
                TempData["msg"] = "Shopping cart " + Alerts.SUCCESS;

            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
