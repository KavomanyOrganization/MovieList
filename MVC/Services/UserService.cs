using MVC.Data;

namespace MVC.Services;
public class UserService
{
    protected readonly AppDbContext _context;

    public UserService(AppDbContext appDbContext){
        _context = appDbContext;
    }

}
