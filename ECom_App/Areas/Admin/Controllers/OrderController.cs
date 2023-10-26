using Ecom.Utility;
using ECom.DataAccess.Repository.IRepository;
using ECom.Models;
using ECom.Models.ViewModels;
using ECom.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECom_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private IUnitOfWork _service;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _service.OrderHeader.FirstOrDefault(x => x.Id == orderId, includeProperties: "applicationUser"),
                OrderDetails = _service.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "product")
            };

            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.EMPLOYEE)]
        public IActionResult UpdateOrderDetails()
        {
            var dbOrderHeader = _service.OrderHeader.FirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id);
            dbOrderHeader.Name = OrderVM.OrderHeader.Name;
            dbOrderHeader.State = OrderVM.OrderHeader.State;
            dbOrderHeader.City = OrderVM.OrderHeader.City;
            dbOrderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            dbOrderHeader.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            dbOrderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                dbOrderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                dbOrderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _service.OrderHeader.Update(dbOrderHeader);
            _service.Save();
            TempData["msg"] = TempData["msg"] = "Order summary " + Alerts.UPDATE;
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.EMPLOYEE)]
        public IActionResult PayNow()
        {
            OrderVM.OrderHeader = _service.OrderHeader.FirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "applicationUser");
            OrderVM.OrderDetails = _service.OrderDetail.GetAll(x => x.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "product");

            var domain = "https://localhost:7011/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var items in OrderVM.OrderDetails)
            {
                var sessionLineItems = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(items.Price * 100),
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
            _service.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _service.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderId)
        {
            OrderHeader order = _service.OrderHeader.FirstOrDefault(x => x.Id == orderId, includeProperties: "applicationUser");
            if (order.PaymentStatus == PaymentStatus.DELAYEDPAYMENT)
            {
                var sessionSerivce = new SessionService();
                Session session = sessionSerivce.Get(order.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _service.OrderHeader.UpdateStripePaymentId(orderId, session.Id, session.PaymentIntentId);
                    _service.OrderHeader.UpdateStatus(orderId, order.OrderStatus, PaymentStatus.APPROVED);
                    _service.Save();
                }
            }
            return View(orderId);
        }




        [HttpPost]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.EMPLOYEE)]
        public IActionResult StartProcessing()
        {
            _service.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatus.INPROCESSS);
            _service.Save();
            TempData["msg"] = TempData["msg"] = "Order In Process" + Alerts.ORDERSUCCESS;
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.EMPLOYEE)]
        public IActionResult ShipOrder()
        {
            var dbOrderHeader = _service.OrderHeader.FirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id);
            dbOrderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            dbOrderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            dbOrderHeader.OrderStatus = OrderStatus.SHIPPED;
            dbOrderHeader.ShippingDate = DateTime.Now;
            if (dbOrderHeader.PaymentStatus == PaymentStatus.DELAYEDPAYMENT)
            {
                dbOrderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _service.OrderHeader.Update(dbOrderHeader);
            _service.Save();
            TempData["msg"] = TempData["msg"] = "Order Shipped " + Alerts.ORDERSUCCESS;
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = Roles.ADMIN + "," + Roles.EMPLOYEE)]
        public IActionResult CancelOrder()
        {
            var dbOrderHeader = _service.OrderHeader.FirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id);
            if (dbOrderHeader.PaymentStatus == PaymentStatus.APPROVED)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = dbOrderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(option);
                _service.OrderHeader.UpdateStatus(dbOrderHeader.Id, OrderStatus.CANCELLED, OrderStatus.REFUNDED);
            }
            else
            {
                _service.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, OrderStatus.CANCELLED);
            }
            _service.Save();
            TempData["msg"] = TempData["msg"] = "Order Cancelled " + Alerts.ORDERSUCCESS;
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        #region APISCALL
        [HttpGet]
        public IActionResult GetAll(string Status)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeader;
                if (User.IsInRole(Roles.ADMIN) || User.IsInRole(Roles.EMPLOYEE))
                {
                    orderHeader = _service.OrderHeader.GetAll(includeProperties: "applicationUser").ToList();
                }
                else
                {
                    var identity = (ClaimsIdentity)User.Identity;
                    var userId = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    orderHeader = _service.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "applicationUser").ToList();
                }
                switch (Status)
                {
                    case "inprocess":
                        orderHeader = orderHeader.Where(x => x.OrderStatus == OrderStatus.INPROCESSS);
                        break;
                    case "pending":
                        orderHeader = orderHeader.Where(x => x.PaymentStatus == PaymentStatus.PENDING);
                        break;
                    case "completed":
                        orderHeader = orderHeader.Where(x => x.OrderStatus == OrderStatus.SHIPPED);
                        break;
                    case "approved":
                        orderHeader = orderHeader.Where(x => x.OrderStatus == OrderStatus.APPROVED);
                        break;
                    default:
                        break;
                }

                return Json(new { data = orderHeader });
            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while Get all product" });
            }
        }
        #endregion

    }
}
