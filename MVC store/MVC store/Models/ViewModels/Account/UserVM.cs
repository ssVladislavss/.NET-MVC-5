using MVC_store.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_store.Models.ViewModels.Account
{
    public class UserVM
    {
        public UserVM() { }                
        public UserVM(UserDTO dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            EmailAdress = dto.EmailAdress;
            Username = dto.Username;
            Password = dto.Password;
        }

        public int Id { get; set; }

        [Required]
        [DisplayName("Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email")]
        public string EmailAdress { get; set; }

        [Required]
        [DisplayName("User Name")]
        public string Username { get; set; }

        [Required]
        //[DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DisplayName("Confirm password")]
        //[DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}