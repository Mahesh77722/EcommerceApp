
using ECom.DataAccess.Repository.IRepository;
using ECom.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECom_App.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _service;

        public ShoppingCartViewComponent(IUnitOfWork service)
        {
            _service = service;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(Alerts.CARTSESSION) == null)
                {
                    HttpContext.Session.SetInt32(Alerts.CARTSESSION, _service.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(Alerts.CARTSESSION));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
