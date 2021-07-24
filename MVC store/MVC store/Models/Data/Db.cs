using System.Data.Entity;

namespace MVC_store.Models.Data
{
    public class Db : DbContext
    {
        public DbSet<PagesDTO> Pages { get; set; }// Связь между сущностью и базой данных
        public DbSet<SidebarDTO> Sidebars { get; set; }
        public DbSet<CategoryDTO> Categories { get; set; }
        public DbSet<ProductDTO> Products { get; set; }
        public DbSet<UserDTO> Users { get; set; }
        public DbSet<RoleDTO> Roles { get; set; }
        public DbSet<UserRoleDTO> UserRoles { get; set; }
        public DbSet<OrderDTO> Orders { get; set; }
        public DbSet<OrderDetailsDTO> OrderDetails { get; set; }
    }
}