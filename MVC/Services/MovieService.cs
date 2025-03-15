using MVC.Data;
using MVC.Models;

namespace MVC.Services;
public class MovieService
{
    protected readonly AppDbContext _context;

    public MovieService(AppDbContext appDbContext){
        _context = appDbContext;
    }
    public int CalculateRating(int MovieId){
        //
        return 0;
    }

    //public List<string> GetGenre(){ }
    //public List<string> GetCountry(){ }

        

    public void ConnectCreatorToMovie(string userId, int movieId){
        MovieCreator movieCreator = new MovieCreator(userId, movieId);
        _context.MovieCreators.Add(movieCreator);
        _context.SaveChanges();
    }


}
