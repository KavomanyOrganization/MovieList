using MVC.Data;
using MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;
public class UserService
{
    protected readonly AppDbContext _context;

    public UserService(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public void ConnectCreatorToMovie(string userId, int movieId){
        MovieCreator movieCreator = new MovieCreator(userId, movieId);
        _context.MovieCreators.Add(movieCreator);
        _context.SaveChanges();
    }


}
