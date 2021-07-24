using MVC_store.Models.Data;
using MVC_store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Объявляю List типа CartVM
            var cart = Session[User.Identity.Name] as List<CartVM> ?? new List<CartVM>();//Если сессия пуста, то создаю новую сессию

            //Проверию пустая корзина или нет
            if(cart.Count == 0 || Session[User.Identity.Name] == null)
            {
                ViewBag.Message = "Ваша корзина пуста.";
                return View();
            }

            //Складываю сумму и записываю в ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Возвращаю List в представление
            return View(cart);
        }



        //Реализую частичное представление
        // GET: Cart/CartPartial
        public ActionResult CartPartial()
        {
            //Объявляю модель CartVM
            CartVM model = new CartVM();

            //Объявляю переменную количества товаров
            int qty = 0;

            //Объявляю переменную цены продукта
            decimal price = 0m;//Всегда для типпа decimal в конце должна присутствовать буква m

            //Проверяю сессию корзины
            if(Session[User.Identity.Name] != null)
            {
                //Получаю общее количество и цену, если в корзине, что-то есть
                var list = (List<CartVM>)Session[User.Identity.Name];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                //Или устанавливаю количество и цену 0
                model.Quantity = 0;
                model.Price = 0m;
            }

            //Возвращаю частичное представление с моделью
            return PartialView("_CartPartial", model);
        }



        //Метод добавления товаров в корзину
        // GET: Cart/AddToCartPartial/id
        public ActionResult AddToCartPartial(int id)
        {
            //Объявляю List типа CartVM
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM> ?? new List<CartVM>();

            //Объявляю модель CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //Получаю продукт по ID
                ProductDTO dto = db.Products.Find(id);

                //Проверяю находится ли такой товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //Если нет, то добавляю этот товар
                if(productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        Name = dto.Name,
                        ProductId = dto.Id,
                        Quantity = 1,
                        Price = dto.Price,
                        Image = dto.ImageName
                    }); 
                }
                else
                {
                    //Если да, то добавляю еденицу товара
                    productInCart.Quantity++;
                }
            }

            //Получаю общее количество товаров, цену и добавляю данные в модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Сохраняю состояние корзины в сессию
            Session[User.Identity.Name] = cart;

            //Возвращаю частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }



        //Метод инкрементирует товар в корзине
        // GET: Cart/IncrementProduct/int productId
        public JsonResult IncrementProduct(int productId)
        {
            //Объявляю List типа CartVM
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM>;

            using (Db db = new Db())
            {
                //Получаю CartVM из List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Добавляю количество
                model.Quantity++;

                //Сохраняю необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                //Возвращаю JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);//Этим аргументом говорю, что можно передавать этот результат
            }
                
        }


        //Метод декремент
        // GET: Cart/DecrementProduct/int productId
        public ActionResult DecrementProduct(int productId)
        {
            //Объявляю List типа CartVM
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM>;

            using (Db db = new Db())
            {
                //Получаю CartVM из List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Уменьшаю количество
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //Сохраняю необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                //Возвращаю JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);//Этим аргументом говорю, что можно передавать этот результат
            }
        }



        //Метод удаления товара из корзины
        // GET: Cart/RemoveProduct/int productId
        public void RemoveProduct(int productId)
        {
            //Объявляю List типа CartVM
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаю CartVM из List
                 CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }



        //Метод для корректной отправки данных в PayPal
        // GET: Cart/PaypalPartial
        public ActionResult PaypalPartial()
        {
            //Получаю лист товаров в корзине
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM>;

            //Возвращаю частичное представление вместе с List
            return PartialView(cart);
        }


        //Метод для корректной отправки данных в PayPal
        // POST: Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Получаю List с товарами в корзине
            List<CartVM> cart = Session[User.Identity.Name] as List<CartVM>;

            //Получаю имя пользователя
            string userName = User.Identity.Name;

            //Объявляю переменную для OrderId и инициализирую
            int orderId = 0;

            using (Db db = new Db())
            {
                //Объявляю модель OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //Получаю ID пользователя
                var q = db.Users.FirstOrDefault(x => x.Username == userName);//Нашёл пользователя в базе
                int userId = q.Id;

                //Заполняю модель OrderDTO и сохраняю её
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                //Получаю OrderId
                orderId = orderDTO.OrderId;

                //Объявляю модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //Добавляю данные в модель OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }

            }

            //Отправляю письмо о заказе на почту администратора
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("40534dc56944d0", "6aee1a57735f41"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "admin@example.com", "New order", $"You have a new order. Order number {orderId}");

            //Обнуляю сессию, если не обнулять сессию при следующем заказе будут серьёзные проблемы
            Session[User.Identity.Name] = null;
           
        }
    }
}