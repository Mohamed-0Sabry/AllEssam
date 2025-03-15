using AlIssam.DataAccessLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlIssam.DataAccessLayer.Entities
{
    public class Payment
    {

        public int Id { get; set; }
        //public EnPaymentMethod PaymentMethod  { get; set; }
        public string PaymentMethod_Ar{ get; set; }
        public string PaymentMethod_En { get; set; }
        public decimal Payment_Amount { get; set; }
        public DateTime Payment_Date { get; set; }
        public string OrderId { get; set; }
        public Order Order { get; set; }

    }
}
