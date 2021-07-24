using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }


        // GET: Shop/CategoryMenuPartial
        public ActionResult CategoryMenuPartial()
        {
            //Объявляю модель List типа CategoryVM
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Инициализирую медель данными
                categoryVMList = db.Categories.ToArray()
                                              .OrderBy(x => x.Sorting)
                                              .Select(x => new CategoryVM(x))
                                              .ToList();
            }

            //Возвращаю частичное представление с моделью
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }



        // GET: Shop/Category/name
        public ActionResult Category(string name)
        {
            //Объявляю List типа ProductVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Получаю ID категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();//Сначала получаю саму категорию
                int catID = categoryDTO.Id;//Получаю ID

                //Инициализирую List данными из базы данных
                productVMList = db.Products.ToArray()
                                           .Where(x => x.CategoryId == catID)
                                           .Select(x => new ProductVM(x))
                                           .ToList();

                //Получаю имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catID).FirstOrDefault();

                //Проверяю на null
                if(productCat == null)//Если productCat == null, то имя получаю другим способом
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }

            }

            //Возвращаю представление с моделью
            return View(productVMList);
        }



        // GET: Shop/product-details/name
        [ActionName("product-details")]//Так как метод нельзя назвать так, в аннотации указываю куда обращаться
        public ActionResult ProductDetails(string name)
        {
            //Объявляю модели ProductVM и ProductDTO
            ProductDTO dto;
            ProductVM model;

            //Инициализирую ID продукта
            int id = 0;

            using (Db db = new Db())
            {
                //Проверяю доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                    return RedirectToAction("Index", "Shop");

                //Инициализирую модель ProductDTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Получаю ID
                id = dto.Id;

                //Инициализирую модель ProductVM данными
                model = new ProductVM(dto);

            }
            //Получаю изображения из галерии
            model.GaleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));

            //Возвращаю представление с моделью
            return View("ProductDetails",model);
        }
    }
}