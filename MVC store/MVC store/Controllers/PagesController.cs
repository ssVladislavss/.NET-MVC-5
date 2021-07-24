using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Controllers
{
    public class PagesController : Controller
    {
        //
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            //Получаю или устанавливаю краткий заголовок (SLUG)
            if (page == "")
                page = "home";

            //Объявляю модель модель и класс DTO
            PageVM model;
            PagesDTO dto;

            using (Db db = new Db())
            {
                //Проверяю доступна ли текущая страница
                if (!db.Pages.Any(x => x.Slug.Equals(page)))//Если в базе не будет совпадений, то переадресовываю пользователя на главную страницу
                    return RedirectToAction("Index", new { page = ""});
            }

            using (Db db = new Db())
            {
                //Получаю контекст данных страницы
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //Устанавливаю заголовок страницы (TITLE)
            ViewBag.PageTitle = dto.Title;

            //Проверяю есть ли боковая панель (SIDEBAR)
            if(dto.HasSidebar == true)            
                ViewBag.Sidebar = "Yes";            
            else            
                ViewBag.Sidebar = "No";

            //Заполняю модель данными
            model = new PageVM(dto);

            //Возвращаю представление с моделью
            return View(model);
        }


        //Метод для создания динамического меню
        public ActionResult PagesMenuPartial()
        {
            //Объявляю List типа PageVM
            List<PageVM> pageVMList;

            using (Db db = new Db())
            {
                //Получаю все страницы, кроме home
                pageVMList = db.Pages.ToArray()
                                     .OrderBy(x => x.Sorting)
                                     .Where(x => x.Slug != "home")
                                     .Select(x => new PageVM(x))
                                     .ToList();
            }


            //Возвращаю частичное представление и List с данными
            return PartialView("_PagesMenuPartial", pageVMList);
        }



        //Метод для создания Sidebar
        public ActionResult SidebarPartial(int? id)
        {
            //Объявляю модель SidebarVM
            SidebarVM model;
           
            using (Db db = new Db())
            {
                if(id != null)
                {                    
                    SidebarDTO siddto = db.Sidebars.FirstOrDefault(x => x.PageId == id);
                    if(siddto != null)            
                       model = new SidebarVM(siddto);
                    else
                    {
                        SidebarDTO dto = db.Sidebars.Find(1);

                        model = new SidebarVM(dto);
                    }                  
                }
                else
                {
                   //Инициализирую модель данными
                   SidebarDTO dto = db.Sidebars.Find(1);
                   
                   model = new SidebarVM(dto);
                }                          
            }
            
            //Возвращаю модель в частичное представление
            return PartialView("_SidebarPartial", model);                        
        }
    }
}