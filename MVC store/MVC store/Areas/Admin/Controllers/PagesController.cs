using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Объявляем список для представления (PageVM)
            List<PageVM> pageList;

            //Инициализируем список данными из базы данных
            using(Db db = new Db())//открыли using, чтобы подключение с базой данных закрылось после инициализации
            {
                pageList = db.Pages//присваиваем листу объекты из базы данных
                             .ToArray()//вносим их в массив
                             .OrderBy(x => x.Sorting)//сортируем
                             .Select(x => new PageVM(x))//выбираем все объекты и инициализируем поля класса PageVM
                             .ToList();// добавляем в лист
            }

            //Возвращаем список в представление
            return View(pageList);
        }



       
        //Метод для добавления
        //Для быстрого создания метода пишем (mvc + 2 раза TAB)

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            using (Db db = new Db())
            {
                //Объявляем переменную для краткого описания (Slug)
                string slug;

                //Инициализировать класс PagesDTO
                PagesDTO dto = new PagesDTO();

                //Присваиваю заголовок модели
                dto.Title = model.Title.ToUpper();

                //Проверяю есть ли краткое описание, если нет, то присваиваю
                if (string.IsNullOrWhiteSpace(model.Slug))
                    slug = model.Title.Replace(" ", "-").ToLower();
                else
                    slug = model.Slug.Replace(" ", "-").ToLower();

                //Проверяю заголовок и краткое описание на уникальность
                if(db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That TITLE already exist.");
                    return View(model);
                }
                else if(db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That SLUG already exist.");
                    return View(model);
                }

                //Присваиваю оставшиеся значения модели PagesDTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;// Присваиваю 100, чтобы эта модель сразу поместилась в конец списка в базе данных

                //Сохраняю модель в базу данных
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            //Передаю сообщение пользователю через TempData
            TempData["SM"] = "You have added a new page!";

            //Переадресовываю пользователя на метод Index
            return RedirectToAction("Index");
           
        }




        //Метод для внесения изменений
        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Объявляю модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаю данные страницы 
                PagesDTO dto = db.Pages.Find(id);

                //Проверяю доступна ли страница
                if (dto == null)
                    return Content("The page does not exist.");//Возвращаю строку, в которой говорю, что страница недоступна

                //Инициализирую модель данными из базы данных
                model = new PageVM(dto);
            }
                //Возвращаю представление с моделью
                return View(model);
        }
        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверяю модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            using (Db db = new Db())
            {
                //Получаем Id страницы
                int id = model.ID;

                //Объявляю переменную для краткого описания (Slug)
                string slug = "home";

                //Получаю страницу по Id
                PagesDTO dto = db.Pages.Find(id);

                //Присваиваю название из полученной модели в DTO
                dto.Title = model.Title;

                //Проверяю краткое описание, если не заполнено, то заполняю его
                if(model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                        slug = model.Title.Replace(" ", "-").ToLower();

                    else
                        slug = model.Slug.Replace(" ", "-").ToLower();

                }
                
                //Проверяю заголовок и краткое описание на уникальность
                if(db.Pages.Where(x => x.ID != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That TITLE already exist.");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.ID != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That SLUG already exist.");
                    return View(model);
                }

                //Присваиваю оставшиеся значения в DTO

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Сохраняю изменения в базу данных
                db.SaveChanges();
            }
            //Оповещаю пользователя о успешности через TempData
            TempData["SM"] = "Страница успешно обновлена.";

            //Переадресовываю пользователя на страницу Index
            return RedirectToAction("EditPage");

                
        }



        //Показ всех данных модели на странице
        // GET: Admin/Pages/PageDetails/id
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //Объявляю модель данных PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаю страницу из базы
                PagesDTO dto = db.Pages.Find(id);

                //Проверяю доступна ли страница
                if (dto == null)
                    return Content("The page does not exist.");

                //Присваиваю модели информацию из базы данных
                model = new PageVM(dto);
            }
            //Возвращаю модель в представление
            return View(model);
        }


        //Метод для удаления
        // GET: Admin/Pages/DeletePage/id
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Получение страницы
                PagesDTO dto = db.Pages.Find(id);

                //Удаление страницы
                db.Pages.Remove(dto);

                //Сохраняю изменения в базе данных
                db.SaveChanges();
            }
            //Сообщаю пользователю об успешности
            TempData["VMD"] = "Страница успешно удалена.";

            //Переадресовываю на страницу Index
            return RedirectToAction("Index");
        }



        //Создаём метод сортировки
        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //Реализую начальный счётчик
                int count = 1;//отсчёт начинается с 1, так как home имеет индекс 0, я трогать не буду

                //Инициализируем модель данных
                PagesDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach(var pd in id)
                {
                    dto = db.Pages.Find(pd);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }


        // GET: Admin/Pages/EditSidebar/id
        [HttpGet]
        public ActionResult EditSidebar(int id)
        {
            //Объявляю модель
            SidebarVM model;
                                      
            using (Db db = new Db())
            {           
                //Проверяю есть ли у этой страницы SIDEBAR
                SidebarDTO pageSidebar = db.Sidebars.Where(x => x.PageId == id).FirstOrDefault();  

                if (pageSidebar != null)
                {
                    model = new SidebarVM(pageSidebar);
                    return View(model);                    
                }                           
            }

            model = new SidebarVM() { PageId = id};

            return View(model);
        }


        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            //Объявляю модель DTO
            SidebarDTO dto;

            //Получаю ID Sidebar
            int id = model.Id;

            using (Db db = new Db())
            {
                //Проверяю есть ли данная модель в базе
                dto = db.Sidebars.FirstOrDefault(x => x.PageId == id);

                if(dto != null)
                {
                    //Присваиваю данные в тело (свойство Body)
                    dto.Body = model.Body;

                    //Сохраняю изменения
                    db.SaveChanges();

                    //Оповещаю об успешности
                    TempData["SM"] = "Боковая панель успешно изменена.";

                    //Переадресовываю пользователя
                    return RedirectToAction("EditSidebar");
                }

                //Если она null
                dto = new SidebarDTO() { Body = model.Body, PageId = model.PageId };

                db.Sidebars.Add(dto);

                db.SaveChanges();

                //Оповещаю об успешности
                TempData["SM"] = "Боковая панель успешно добавлена.";

                //Переадресовываю пользователя
                return RedirectToAction("EditSidebar");
            }

        }
    }
}