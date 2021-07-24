using MVC_store.Areas.Admin.Models.ViewModels.Shop;
using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Shop;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MVC_store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляю модель типа List
            List<CategoryVM> CategoryList;
            
            using (Db db = new Db())
            {
                //Инициализирую модель данными
                CategoryList = db.Categories//присваиваем листу объекты из базы данных
                                 .ToArray()//вносим их в массив
                                 .OrderBy(x => x.Sorting)//сортируем
                                 .Select(x => new CategoryVM(x))//выбираем все объекты и инициализируем поля класса CategoryVM
                                 .ToList();//добавляем в лист
            }

            //Возвращаю List в представление
            return View(CategoryList);
        }



        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Объявляю переменную ID
            string id;

            using (Db db = new Db())
            {
                //Проверяю имя категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //Инициализирую модель CategoryDTO
                CategoryDTO dto = new CategoryDTO();

                //Заполняю модель DTO данными с базы данных
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Сохраняю изменения в базу
                db.Categories.Add(dto);
                db.SaveChanges();

                //Получаю ID для возврата в представление
                id = dto.Id.ToString();
            }

            //Возвращаю ID в представление
            return id;
        }



        //Создаём метод сортировки
        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Реализую начальный счётчик
                int count = 1;//отсчёт начинается с 1, так как home имеет индекс 0, я трогать не буду

                //Инициализируем модель данных
                CategoryDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }



        //Метод для удаления
        // GET: Admin/Shop/DeleteCategory/id
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получение модели категории
                CategoryDTO dto = db.Categories.Find(id);

                //Удаление категории
                db.Categories.Remove(dto);

                //Сохраняю изменения в базе данных
                db.SaveChanges();
            }
            //Сообщаю пользователю об успешности
            TempData["VMD"] = "Категория успешно удалена.";

            //Переадресовываю на страницу Index
            return RedirectToAction("Categories");
        }



        //Метод для переименования категорий
        // POST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)//параметры придут из script
        {
            using (Db db = new Db())
            {
                //Проверяю имя категории на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //Получаю данные из базы данных
                CategoryDTO dto = db.Categories.Find(id);

                //Редактирую данные
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Сохраняю изменения
                db.SaveChanges();
            }

            //Возвращаю слово
            return "ok";

        }



        //Метод добавления товаров
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявляю модель данных ProductVM
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                //Добавляю в эту модель категории из базы данных
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "id", dataTextField: "Name");
            }

            //Возвращаю модель в представление
            return View(model);
        }



        //Метод добавления товаров
        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Проверяю модель на валидность
            if (!ModelState.IsValid)//Если модель не валидна, заполняю выпадающий список всеми категориями и отправляю в представление модель обратно, если не заполню, будет exception
            {
                using (Db db = new Db())
                {
                    ModelState.AddModelError("", "Некорректные данные.");
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                }
                return View(model);
            }
            
            using (Db db = new Db())
            {
                //Проверяю имя продукта на уникальность
                if(db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Это имя продукта занято");//Добавили сообщение об ошибке
                    return View(model);
                }
            }

            //Объявляю переменную productId
            int id;
           
            using (Db db = new Db())
            {
                //Инициализирую и сохраняю в базу данных модель
                ProductDTO product = new ProductDTO();

                //Добавляю данные в базу с пришедшей модели
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catdto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);//Ищу в базе категорию, к которой принадлежит продукт по ID

                product.CategoryName = catdto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;//Получаю ID добавленного продукта
            }

            //Добавляю сообщению пользователю в TempData
            TempData["SM"] = "Вы успешно добавили новый продукт.";

            #region Upload Image
            //Создаю ссылки директорий для сохранения картинок
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");//Первый путь

            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());//Второй путь

            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");//Thumbs - уменьшенная копия картинки
           
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");

            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Проверяю наличие директорий (если нет, то создаю директорию)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Проверяю, был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                //Получаю расширение файла
                string ext = file.ContentType.ToLower();

                //Проверяю расширение загружаемого файла
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Изображение не было загружено, неверное расширение.");
                        return View(model);
                    }
                }


                //Объявляю переменную с именем изображения
                string imageName = file.FileName;

                if (!imageName.Equals(":"))
                {
                    //Сохраняю имя изображения в модель ProductDTO
                    using (Db db = new Db())
                    {
                        ProductDTO dto = db.Products.Find(id);

                        //Дополняю модель данными
                        dto.ImageName = imageName;

                        db.SaveChanges();
                    }

                    //Назначаю пути к оригинальному и уменьшенному изображению
                    var path = string.Format($"{pathString2}\\{imageName}");

                    var path2 = string.Format($"{pathString3}\\{imageName}");

                    //Сохраняю оригинальное изображение
                    file.SaveAs(path);

                    //Создаю и сохраняю уменьшенную копию
                    WebImage img = new WebImage(file.InputStream);//Создали уменьшенную версию картинки
                    img.Resize(200, 200).Crop(1, 1);//Метод Resize(200, 200) при уменьшении изображения оставляет полоски слева и сверху,Crop(1, 1) этот метод обрезает изображение со всех сторон по 1 пикселю, тем самым обирает полоски
                    img.Save(path2);

                    //Переадресовываю пользователя
                    return RedirectToAction("AddProduct");
                }
                else
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Изображение не было загружено, имя содержит некорректные символы.");
                        return View(model);
                    }
                }

                
            }
            #endregion

            //Переадресовываю пользователя
            return RedirectToAction("AddProduct");
        }


        //Метод списка товаров
        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Объявляю List типа ProductVM
            List<ProductVM> ListOfProductsVM;

            //Устанавливаю номер страницы
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Инициализирую List и заполняю его данными из базы данных
                ListOfProductsVM = db.Products.ToArray()
                                              .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                              .Select(x => new ProductVM(x))
                                              .ToList();

                //Заполняю категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Устанавливаю выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }

            //Устанавливаю постраничную навигацию
            var onePageOfProducts = ListOfProductsVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //Возвращаю представление с моделью
            return View(ListOfProductsVM);
        }



        //Метод редактирования продуктов
        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Объявляю модель ProductVM
            ProductVM model;

            using (Db db = new Db())
            {
                //Получаю продукт
                ProductDTO dto = db.Products.Find(id);

                //Проверяю доступен ли продукт
                if (dto == null)
                    return Content("That PRODUCT does not exist.");

                //Инициализирую модель данными
                model = new ProductVM(dto);

                //Создаю список категорий
                model.Categories = new SelectList(db.Categories.ToList(),"Id", "Name");

                //Получаю все изображения из галереи
                model.GaleryImages = Directory
                     .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                     .Select(fn => Path.GetFileName(fn));

            }

            //Возвращаю модель в представление
            return View(model);
        }



        //Метод редактирования продуктов
        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Получаю ID продукта
            int id = model.Id;

            using (Db db = new Db())
            {
                //Заполняю список категориями и изображениями
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GaleryImages = Directory
                     .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                     .Select(fn => Path.GetFileName(fn));

            //Проверяю модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            using (Db db = new Db())
            {
                //Проверяю имя продукта на уникальность
                if(db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Это имя уже занято. Попробуйте использовать другое.");
                    return View(model);
                }
            }

            using (Db db = new Db())
            {
                //Обновляю продукт в базе данных
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catdto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catdto.Name;

                db.SaveChanges();
            }

            //Устанавливаю сообщение пользователю об успешности операции в TempData
            TempData["SM"] = "Обновление прошло успешно.";


            //Реализую логику обработки изображений 
            #region Image Upload
            //Проверяю загружен ли файл
            if(file != null && file.ContentLength > 0)
            {
                //Получаю расширение файла
                string ext = file.ContentType.ToLower();

                //Проверяю расширение файла
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {                        
                        ModelState.AddModelError("", "Изображение не было загружено, неверное расширение.");
                        return View(model);
                    }
                }

                //Устанавливаю пути для загрузки файла
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());//Первый путь
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");//Thumbs - уменьшенная копия картинки

                //Проверяю есть ли уже загруженные файлы в директориях, если да, то удаляю
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                //Удаляю элементы в директориях
                foreach (var file2 in di1.GetFiles())// В DirectoryInfo нет энумератора, поэтому пользуемся методом, который возвращает список файлов в этой директории
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //Сохраняю файлы
                string imageName = file.FileName;//Сохраняю имя полученного файла

                using (Db db = new Db())
                {
                    //Инициализирую модель ProductDTO
                    ProductDTO dto = db.Products.Find(id);

                    //Присваиваю имя картинки
                    dto.ImageName = imageName;

                    //Сохраняю изменения в базе данных
                    db.SaveChanges();
                }

                //Сохраняю оригинал и превью версии (уменьшенные)
                //Назначаю пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString1}\\{imageName}");

                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Сохраняю оригинальное изображение
                file.SaveAs(path);

                //Создаю и сохраняю уменьшенную копию
                WebImage img = new WebImage(file.InputStream);//Создали уменьшенную версию картинки
                img.Resize(200, 200).Crop(1, 1);//Метод Resize(200, 200) при уменьшении изображения оставляет полоски слева и сверху,Crop(1, 1) этот метод обрезает изображение со всех сторон по 1 пикселю, тем самым обирает полоски
                img.Save(path2);
            }

            #endregion

            //Переадресовываю пользователя на страницу с продуктами
            return RedirectToAction("EditProduct");
        }


        //Метод удаления продукта
        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                //Удаляю продукт из базы данных
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            //Удаляю директорию товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));            
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString/*Удалит основной каталог*/, true/*Удалит все подкаталоги*/);

            //Переадресовываю пользователя на страницу с продуктами
            return RedirectToAction("Products");
        }


        //Метод для добавления изображений в галерею
        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Перебираю все полученные файлы из представления
            foreach (string fileName in Request.Files/*Request.Files - стандартный метод .NET, цикл будет перебирать все полученные файлы*/)
            {
                //Инициализирую файлы
                HttpPostedFileBase file = Request.Files[fileName];//Перебирает все по имени и добавляет в file
                
                //Проверяю на null
                if(file != null && file.ContentLength > 0)
                {
                    //Назначаю все пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Назначаю пути изображений
                    var path1 = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    //Сохранию оригинальные и уменьшенные копии
                    file.SaveAs(path1);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);//Метод Resize(200, 200) при уменьшении изображения оставляет полоски слева и сверху,Crop(1, 1) этот метод обрезает изображение со всех сторон по 1 пикселю, тем самым обирает полоски
                    img.Save(path2);
                }
            }
        }


        //Метод для удаления изображений из галереи
        // POST: Admin/Shop/DeleteImage/id/imageName
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            //Проверяю доступно ли изображение по заданному пути
            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }




        //Метод для вывода всех заказов для администратора
        // GET: Admin/Shop/Orders
        [HttpGet]
        public ActionResult Orders()
        {
            //Инициализирую модель OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                //Инициализирую List типа OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираю данные модели OrderVM
                foreach (var order in orders)
                {
                    //Инициализирую словарь товаров (ProductsAndQty)
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    //Создаю переменную для общей суммы
                    decimal total = 0m;

                    //Инициализирую List типа OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Получаю имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string userName = user.Username;

                    //Перебираю список товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Получаю товар, который относится к пользователю
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаю цену этого товара
                        decimal price = product.Price;

                        //Получаю название товара
                        string productName = product.Name;

                        //Добавляю товар в словарь
                        productAndQty.Add(productName, orderDetails.Quantity);

                        //Получаю полную стоимость всех товаров пользователя
                        total += orderDetails.Quantity * price;

                    }

                    //Добавляю данные в модель OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UsetName = userName,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });  

                }
            }

            //Возвращаю представление с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }
}