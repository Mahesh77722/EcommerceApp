using ECom.DataAccess.Repository.IRepository;
using ECom.Models;
using ECom.Models.ViewModels;
using ECom.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Security.Claims;

namespace ECom_App.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _service;
        [BindProperty]
        public ShoppingCartVM cartVM { get; set; }

        public CartController(IUnitOfWork service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM shoppingCartVM = new()
            {
                shoppingCartList = _service.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product"),
                orderHeader = new()
            };
            IEnumerable<ProductImage> productImages = _service.ProductImage.GetAll();

            foreach (var cart in shoppingCartVM.shoppingCartList)
            {
                cart.product.ProductImages = productImages.Where(x => x.ProductId == cart.Id).ToList();
                cart.price = GetPriceBaseOnQuantity(cart);
                shoppingCartVM.orderHeader.OrderTotal += (cart.price * cart.Count);
            }
            return View(shoppingCartVM);
        }
        public IActionResult Plus(int CartId)
        {
            var dbCart = _service.ShoppingCart.FirstOrDefault(x => x.Id == CartId);
            dbCart.Count += 1;
            _service.ShoppingCart.Update(dbCart);
            _service.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int CartId)
        {
            var dbCart = _service.ShoppingCart.FirstOrDefault(x => x.Id == CartId);
            dbCart.Count -= 1;
            if (dbCart.Count > 0)
            {
                _service.ShoppingCart.Update(dbCart);
                _service.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(Remove), new { CartId = CartId });

            }
        }
        public IActionResult Remove(int CartId)
        {
            var dbCart = _service.ShoppingCart.FirstOrDefault(x => x.Id == CartId);
            dbCart.Count -= 1;

            _service.ShoppingCart.Remove(dbCart);
            HttpContext.Session.SetInt32(Alerts.CARTSESSION, _service.ShoppingCart.GetAll(x => x.ApplicationUserId == dbCart.ApplicationUserId).Count() - 1);
            _service.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM shoppingCartVM = new()
            {
                shoppingCartList = _service.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product"),
                orderHeader = new()
            };
            shoppingCartVM.orderHeader.applicationUser = _service.ApplicationUser.FirstOrDefault(x => x.Id == userId);
            shoppingCartVM.orderHeader.Name = shoppingCartVM.orderHeader.applicationUser.Name;
            shoppingCartVM.orderHeader.PhoneNumber = shoppingCartVM.orderHeader.applicationUser.PhoneNumber;
            shoppingCartVM.orderHeader.StreetAddress = shoppingCartVM.orderHeader.applicationUser.StreetAddress;
            shoppingCartVM.orderHeader.City = shoppingCartVM.orderHeader.applicationUser.City;
            shoppingCartVM.orderHeader.State = shoppingCartVM.orderHeader.applicationUser.State;
            shoppingCartVM.orderHeader.PostalCode = shoppingCartVM.orderHeader.applicationUser.PostalCode;
            foreach (var cart in shoppingCartVM.shoppingCartList)
            {
                cart.price = GetPriceBaseOnQuantity(cart);
                shoppingCartVM.orderHeader.OrderTotal += (cart.price * cart.Count);
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName(nameof(Summary))]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cartVM.shoppingCartList = _service.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "product");
            cartVM.orderHeader.OrderDate = DateTime.Now;
            cartVM.orderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _service.ApplicationUser.FirstOrDefault(x => x.Id == userId);
            foreach (var cart in cartVM.shoppingCartList)
            {
                cart.price = GetPriceBaseOnQuantity(cart);
                cartVM.orderHeader.OrderTotal += (cart.price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                cartVM.orderHeader.PaymentStatus = PaymentStatus.PENDING;
                cartVM.orderHeader.OrderStatus = OrderStatus.PENDING;
            }
            else
            {
                cartVM.orderHeader.PaymentStatus = PaymentStatus.DELAYEDPAYMENT;
                cartVM.orderHeader.OrderStatus = OrderStatus.APPROVED;
            }

            _service.OrderHeader.Add(cartVM.orderHeader);
            _service.Save();

            foreach (var cart in cartVM.shoppingCartList)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = cartVM.orderHeader.Id,
                    Price = cart.price,
                    Count = cart.Count
                };

                _service.OrderDetail.Add(orderDetails);
                _service.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?orderId={cartVM.orderHeader.Id}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var items in cartVM.shoppingCartList)
                {
                    var sessionLineItems = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(items.price * 100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = items.product.Title
                            }
                        },
                        Quantity = items.Count
                    };

                    options.LineItems.Add(sessionLineItems);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _service.OrderHeader.UpdateStripePaymentId(cartVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                _service.Save();
                Response.Headers.Add("Location", session.Url);
                HttpContext.Session.Clear();

                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { orderId = cartVM.orderHeader.Id });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            OrderHeader order = _service.OrderHeader.FirstOrDefault(x => x.Id == orderId, includeProperties: "applicationUser");
            if (order.PaymentStatus != PaymentStatus.DELAYEDPAYMENT)
            {
                var sessionSerivce = new SessionService();
                Session session = sessionSerivce.Get(order.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _service.OrderHeader.UpdateStripePaymentId(orderId, session.Id, session.PaymentIntentId);
                    _service.OrderHeader.UpdateStatus(orderId, OrderStatus.APPROVED, PaymentStatus.APPROVED);
                    _service.Save();
                }
            }
            List<ShoppingCart> carts = _service.ShoppingCart.GetAll(x => x.ApplicationUserId == order.ApplicationUserId).ToList();
            _service.ShoppingCart.RemoveRange(carts);
            _service.Save();

            return View(orderId);
        }
        private double GetPriceBaseOnQuantity(ShoppingCart shoppingCart)
        {

            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.product.Price50;
                }
                else
                {
                    return shoppingCart.product.Price100;
                }
            }
        }
    }
}
