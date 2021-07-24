using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVC_store.Models.Data
{
    //Поля должны быть заданы в той же последовательности, что и в базе данных

    [Table("tblSidebar")]//указываем к какой таблице относится модель
    public class SidebarDTO
    {
        [Key]//Указываем, что это первичный ключ
        public int Id { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public int PageId { get; set; }
    }
}