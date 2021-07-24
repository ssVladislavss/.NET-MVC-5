using MVC_store.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Models.ViewModels.Pages
{
    //Получает от модели все данные и отдаёт их представлению
    //Связи с базой данных не имеет

    public class PageVM
    {
        public PageVM() { }//Если по какой-то причине приложение не сможет получить данные, то будет использовать конструктор по умолчанию
               
        public PageVM(PagesDTO pagesDTO)//Присваиваем полям данные из модели, связанной с базой данных
        {
            ID = pagesDTO.ID;
            Title = pagesDTO.Title;
            Slug = pagesDTO.Slug;
            Body = pagesDTO.Body;
            Sorting = pagesDTO.Sorting;
            HasSidebar = pagesDTO.HasSidebar;
        }
        public int ID { get; set; }

        [Required]//Указываем, что элемент обязателен
        [StringLength(50, MinimumLength = 3)]// Указываем максимальную и минимальную длину имени
        public string Title { get; set; }

        public string Slug { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name = "Sidebar")]
        public bool HasSidebar { get; set; }
    }
}