using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVC_store.Models.Data
{
    [Table("tblUserRoles")]
    public class UserRoleDTO
    {
        [Key, Column(Order = 0)]//Указываю к какому столбцу принадлежит этот ключ
        public int UserId { get; set; }
        [Key, Column(Order = 1)]//Указываю к какому столбцу принадлежит этот ключ
        public int RoleId { get; set; }


        //Свойства для связывания
        [ForeignKey("UserId")]
        public virtual UserDTO User { get; set; }

        [ForeignKey("RoleId")]
        public virtual RoleDTO Role { get; set; }
    }
}