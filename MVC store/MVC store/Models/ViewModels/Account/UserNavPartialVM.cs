using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_store.Models.ViewModels.Account
{
    public class UserNavPartialVM//Модель для частичного представления, будет выводится в меню
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}