
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MVC.Data
{
    public class AppDbContext: IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options): base(options){
        }
    }
}