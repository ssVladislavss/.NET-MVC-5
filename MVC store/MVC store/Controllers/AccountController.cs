using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Account;
using MVC_store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVC_store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }



        // GET: Account/create-account
        [ActionName("create-account")]//Меняю имя вызова метода, при вызове по этому имени, будет вызываться этот метод
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }



        // POST: Account/create-account
        [ActionName("create-account")]//Меняю имя вызова метода, при вызове по этому имени, будет вызываться этот метод
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Проверяю модель на валидность
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            //Проверяю правильность полученного пароля
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароли не совпадают.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Проверяю имя на уникальность, если такое уже есть, то вывожу сообщение, что имя занято
                if(db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Имя пользователя: {model.Username}, уже занято");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Создаю экземплар класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    //Присваиваю данные с пришедшей модели
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password
                };

                //Добавляю все данные в базу данных
                db.Users.Add(userDTO);

                //Сохраняю данные в базе
                db.SaveChanges();

                //Добавляю роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2//Роль обычных пользователей находится под ID = 2
                };
                db.UserRoles.Add(userRoleDTO);

                //Сохраняю роль в базе
                db.SaveChanges();
            }

            //Записываю сообщение в TempData
            TempData["SM"] = "Регистрация прошла успешно.";

            //Переадресовываю пользователя на страницу профиля
            return RedirectToAction("Login");
        }


        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Подтверждаю, что пользователь не авторизован
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))//Если авторизован, то переадресовываю на его профиль
                return RedirectToAction("user-profile");

            //Возвращаю представление
            return View();
        }



        //Метод для входа в аккаунт
        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Проверяю пришедшую модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            //Проверяю пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())
            {
                if(db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))                
                    isValid = true;

                if(!isValid)
                {
                    ModelState.AddModelError("", "Неправильное имя пользователя или пароль.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));//Переадресация будет происходит по пути по умолчанию, этот путь прописан в файле Web.Config
                }               
            }                
        }


        //Метод для выхода из аккаунта
        //GET:/account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();//Выходит из аккаунта и чистит Cookie
            return RedirectToAction("Login");
        }



        //Метод для вывода информации о пользователе, если он авторизован
        //GET:/account/Logout
        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Получаю имя пользователя
            string userName = User.Identity.Name;

            //Объявляю модель 
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Получаю пользователя из базы данных
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Заполняю модель данными из базы данных(UserDTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            //Возвращаю частичное представление с моделью
            return PartialView("_UserNavPartial", model);
        }



        //Метод для отображения профиля пользователя
        //GET:/account/user-profile
        [Authorize]
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //Получаю имя пользователя
            string userName = User.Identity.Name;

            //Объявляю модель
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Получаю пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                //Инициализирую модель данными
                model = new UserProfileVM(dto);

            }

            //Возвращаю представление с моделью
            return View("UserProfile", model);
        }




        //Метод для изменения профиля пользователя
        //POST:/account/user-profile/model
        [Authorize]
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;//Переменная нужна, чтобы понять менялось ли поле UserName

            //Проверяю модель на валидность
            if (!ModelState.IsValid)
                return View("UserProfile",model);

            //Проверяю пароль (если пользователь его меняет)
            if(!string.IsNullOrWhiteSpace(model.Password))
            {
                if(!model.Password.Equals(model.ConfirmPassword))//Если есть различия, добавляю ошибку
                {
                    ModelState.AddModelError("", "Пароли не совпадают.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Получаю имя пользователя
                string userName = User.Identity.Name;//Берётся из файлов Cookie

                //Проверяю, сменилось ли имя пользователя
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }
                    
                //Проверяю имя на уникальность
                if(db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))//Ишу всё кроме пришедшей модели и сопоставляю имена
                {
                    ModelState.AddModelError("", $"Имя: {model.Username} уже занято.");
                    model.Username = "";
                    return View(model);
                }

                //Изменяю контекст данных(UserDTO)
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;                
                dto.Username = model.Username;

                if(!string.IsNullOrWhiteSpace(model.Password))
                {
                    if (!dto.Password.Equals(model.Password))
                        dto.Password = model.Password;                   
                }

                //Сохраняю изменения
                db.SaveChanges();

            }

            //Устанавливаю сообщение в TempData
            TempData["SM"] = "Ваш профиль успешно изменён.";

            if (!userNameIsChanged)
                //Возвращаю представление с моделью
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");


        }



        //Метод для показа заказов пользователю
        //GET:/account/Orders
        [Authorize(Roles = "User")]
        [HttpGet]
        public ActionResult Orders()
        {
            //Инициализирую модель OrdersForUserVM
            List<OrdersForUserVM> model = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //Получаю ID пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                //Инициализирую модель OrderVM
                List<OrderVM> orderVM = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                

                //Перебираю список товаров в OrderVM
                foreach (var item in orderVM)
                {
                    //Инициализирую словарь товаров (DICTIONARY)
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Объявляю переменную конечной суммы
                    decimal total = 0m;

                    //Инициализирую модель OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetails = db.OrderDetails.Where(x => x.OrderId == item.OrderId).ToList();

                    //Перебираю список OrderDetailsDTO
                    foreach (var item2 in orderDetails)
                    {
                        //Получаю товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == item2.ProductId);

                        //Получаю цену товара
                        decimal price = product.Price;

                        //Получаю имя товара
                        string productName = product.Name;

                        //Добавляю товар в словарь
                        productsAndQty.Add(productName, item2.Quantity);

                        //Получаю конечную стоимость товара
                        total += price * item2.Quantity;

                    }

                    //Добавляю полученные данные в модель OrdersForUserVM
                    model.Add(new OrdersForUserVM()
                    {
                        OrderNumber = item.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = item.CreatedAt
                    });                   
                }
            }

            //Возвращаю представление с моделью
            return View(model);
        }
    }
}