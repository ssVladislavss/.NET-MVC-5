using MVC_store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVC_store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //ћетод обработки запросов аутентификации
        protected void Application_AuthenticateRequest()//¬ этом классе все методы должны быть protected, чтобы не было проблем с безопасностью, будет отрабатывать при каждом новом запросе пользовател¤
        {
            //ѕровер¤ю, авторизован ли пользователь
            if (User == null)
                return;//≈сли имени нет, то просто прекращаю работу метода

            //ѕолучаю им¤ пользовател¤
            string userName = Context.User.Identity.Name;

            //ќбъ¤вл¤ю массив ролей (декларирую)
            string[] roles = null;//ќб¤зательно сразу присваиваю значение массиву


            using (Db db = new Db())
            {
                //«аполн¤ю массив рол¤ми
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                if (dto == null)//он может равн¤тьс¤ null, если пользователи изменил поле UserName, а в файлах Cookie осталось старое им¤
                    return;
                
                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();//¬ношу в массив, потому что у пользовател¤ может быть несколько ролей
                
            }

            //—оздаю объект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //ќбъ¤вл¤ю и инициализирую данными Context.User
            Context.User = newUserObj;

            //Ѕлагодар¤ этому методу приложение будет корректно получать информацию о рол¤х пользователей
        }
    }
}
