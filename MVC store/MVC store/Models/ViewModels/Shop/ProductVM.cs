using MVC_store.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_store.Models.ViewModels.Shop
{
    public class ProductVM
    {
        public ProductVM() { }
        public ProductVM(ProductDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Slug = dto.Slug;
            Description = dto.Description;
            Price = dto.Price;
            CategoryName = dto.CategoryName;
            CategoryId = dto.CategoryId;
            ImageName = dto.ImageName;
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }

        [Required]
        public string Description { get; set; }
        public decimal Price { get; set; }

                
        public string CategoryName { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [DisplayName("Image")]
        public string ImageName { get; set; }

        //Один из способов передавать в представление несколько моделей
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<string> GaleryImages { get; set; }

        
    }
}