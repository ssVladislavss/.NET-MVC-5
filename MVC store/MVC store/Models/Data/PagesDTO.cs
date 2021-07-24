using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVC_store.Models.Data
{
    //Поля должны быть заданы в той же последовательности, что и в базе данных

    [Table("tblPages")]//указываем к какой таблице относится модель
    public class PagesDTO
    {
        [Key]//Указываем, что это первичный ключ
        public int ID { get; set; }


        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSidebar { get; set; }
    }
}