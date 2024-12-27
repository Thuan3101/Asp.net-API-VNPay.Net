using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNPAY.NET.Models
{
    public class Payment
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IpAddress { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsSuccess { get; set; }
        public string TransactionStatus { get; set; }
    }

}
