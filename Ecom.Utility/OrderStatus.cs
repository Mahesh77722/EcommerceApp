using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.Utility
{
    public static class OrderStatus
    {
        public static string PENDING = "Pending";
        public static string APPROVED = "Approved";
        public static string INPROCESSS = "InProcess";
        public static string SHIPPED = "Shipped";
        public static string CANCELLED = "Cancelled";
        public static string REFUNDED = "Refunded";
    }
    public static class PaymentStatus
    {
        public static string PENDING = "Pending";
        public static string APPROVED = "Approved";
        public static string DELAYEDPAYMENT = "ApprovedForDelayedPayment";
        public static string REJECTED = "Rejected";
    }
}
