using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MVC_store.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        [DisplayName("Номер заказа")]
        public int OrderNumber { get; set; }
        
        [DisplayName("Общая сумма")]
        public decimal Total { get; set; }

        [DisplayName("Заказ")]
        public Dictionary<string, int> ProductsAndQty { get; set; }

        [DisplayName("Дата оформления заказа")]
        public DateTime CreatedAt { get; set; }
    }
}