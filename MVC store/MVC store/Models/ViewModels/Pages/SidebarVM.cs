using MVC_store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Models.ViewModels.Pages
{
    //Получает от модели все данные и отдаёт их представлению
    //Связи с базой данных не имеет
    public class SidebarVM
    {
        public SidebarVM() { }//Если по какой-то причине приложение не сможет получить данные, то будет использовать конструктор по умолчанию
        public SidebarVM(SidebarDTO dto)//Присваиваем полям данные из модели, связанной с базой данных
        {
            Id = dto.Id;
            Body = dto.Body;
            PageId = dto.PageId;
        }
        public int Id { get; set; }

        [AllowHtml]//Разрешаю Html теги
        public string Body { get; set; }

        public int PageId { get; set; }
    }
}