using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using MVC.Models;

namespace MVC.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using (var context = serviceProvider.GetRequiredService<AppDbContext>())
        {
            // Check if database already has data
            if (context.Movies.Any())
            {
                Console.WriteLine("Database already has data. Seeding skipped.");
                return;
            }

            // Add Genres
            var genres = new List<Genre>
            {
                new Genre("Fantasy"),
                new Genre("Adventure"),
                new Genre("Family"),
                new Genre("Magic"),
                new Genre("Mystery")
            };
            
            await context.Genres.AddRangeAsync(genres);
            await context.SaveChangesAsync();
            
            // Add Countries
            var countries = new List<Country>
            {
                new Country("United Kingdom"),
                new Country("United States"),
                new Country("France"),
                new Country("Germany")
            };
            
            await context.Countries.AddRangeAsync(countries);
            await context.SaveChangesAsync();

            // Add Users
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Create Admin User
            var adminUser = new User
            {
                UserName = "admin1",
                Email = "admin1@gmail.com",
                EmailConfirmed = true
            };
            Env.Load();

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, Env.GetString("ADMIN_PASSWORD"));
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create Regular User
            var regularUser = new User
            {
                UserName = "user1",
                Email = "user1@gmail.com",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(regularUser.Email) == null)
            {
                await userManager.CreateAsync(regularUser, "User-1");
                await userManager.AddToRoleAsync(regularUser, "User");
            }

            // Add Movies with all necessary relationships
            var movies = new List<Movie>
            {
                new Movie(
                    cover: "https://f.woowoowoowoo.net/resize/186x286/28/70/2870c14e5c015c1e5479332812e2c488/2870c14e5c015c1e5479332812e2c488.jpg",
                    title: "Harry Potter and the Philosopher's Stone",
                    year: 2001,
                    duration: 152,
                    director: "Chris Columbus",
                    description: "Harry Potter has lived under the stairs in his aunt and uncle's house his whole life. But on his 11th birthday, he finds he is a powerful wizard -- with an area waiting for him at the Hogwarts School of Witchcraft and Wizardry. Since he learns to exploit his new found abilities with the assistance of the kindly headmaster of the school, Harry uncovers the truth about his parents' deaths -- and also about."
                ),
                new Movie(
                    cover: "https://f.woowoowoowoo.net/resize/186x286/0d/2d/0d2d398324656f5ce9a2e12740ba7101/0d2d398324656f5ce9a2e12740ba7101.jpg",
                    title: "Harry Potter and the Chamber of Secrets",
                    year: 2002,
                    duration: 161,
                    director: "Chris Columbus",
                    description: "Ignoring dangers Harry returns to Hogwarts to investigate -- aided by Hermione and Ron -- a chain of strikes."
                ),
                new Movie(
                    cover: "https://f.woowoowoowoo.net/resize/186x286/81/79/817909c57756952292bf3b1062eb86ca/817909c57756952292bf3b1062eb86ca.jpg",
                    title: "Harry Potter and the Prisoner of Azkaban",
                    year: 2004,
                    duration: 141,
                    director: "Alfonso Cuarón",
                    description: "Ron, harry and Hermione go back to Hogwarts. Harry comes face to face with danger yet again, this time at the form of escaped convict turns into sympathetic Professor Lupin for assistance."
                ),
                new Movie(
                    cover: "https://f.woowoowoowoo.net/resize/186x286/ff/1c/ff1cf7fcffe173fe357427bf7d3e9426/ff1cf7fcffe173fe357427bf7d3e9426.jpg",
                    title: "Harry Potter and the Goblet of Fire",
                    year: 2005,
                    duration: 157,
                    director: "Mike Newell",
                    description: "Harry faces the evil Lord Voldemort, competes at the Triwizard Tournament that is dangerous and starts his fourth year at Hogwarts. Ron and Hermione help Harry manage the pressure -- but Voldemort lurks, anticipating his chance to destroy all and Harry that he stands for."
                ),
                new Movie(
                    cover: "https://f.woowoowoowoo.net/resize/186x286/5e/7a/5e7a85977bb11f6441d9fa81b27ee4ef/5e7a85977bb11f6441d9fa81b27ee4ef.jpg",
                    title: "Harry Potter and the Order of the Phoenix",
                    year: 2007,
                    duration: 138,
                    director: "David Yates",
                    description: "Returning to get his fifth year old at Hogwarts, Harry is astonished to discover his warnings about the return of Lord Voldemort have been ignored. Harry takes things into their hands, practice a little set of students -- dubbed'Dumbledore's Army' -- to defend themselves."
                )
            };

            // Set some initial ratings
            movies[0].Rating = 8.8;
            movies[1].Rating = 9.3;
            movies[2].Rating = 8.9;
            movies[3].Rating = 9.0;
            movies[4].Rating = 8.7;

            await context.Movies.AddRangeAsync(movies);
            await context.SaveChangesAsync();

            // Add MovieGenres relationships
            var movieGenres = new List<MovieGenre>
            {
                // Harry Potter 1: Fantasy, Adventure, Family, Magic
                new MovieGenre(movies[0].Id, genres[0].Id), // Fantasy
                new MovieGenre(movies[0].Id, genres[1].Id), // Adventure
                new MovieGenre(movies[0].Id, genres[2].Id), // Family
                new MovieGenre(movies[0].Id, genres[3].Id), // Magic

                // Harry Potter 2: Fantasy, Adventure, Mystery
                new MovieGenre(movies[1].Id, genres[0].Id), // Fantasy
                new MovieGenre(movies[1].Id, genres[1].Id), // Adventure
                new MovieGenre(movies[1].Id, genres[4].Id), // Mystery

                // Harry Potter 3: Fantasy, Adventure, Mystery
                new MovieGenre(movies[2].Id, genres[0].Id), // Fantasy
                new MovieGenre(movies[2].Id, genres[1].Id), // Adventure
                new MovieGenre(movies[2].Id, genres[4].Id), // Mystery

                // Harry Potter 4: Fantasy, Adventure, Magic
                new MovieGenre(movies[3].Id, genres[0].Id), // Fantasy
                new MovieGenre(movies[3].Id, genres[1].Id), // Adventure
                new MovieGenre(movies[3].Id, genres[3].Id), // Magic

                // Harry Potter 5: Fantasy, Adventure, Magic
                new MovieGenre(movies[4].Id, genres[0].Id), // Fantasy
                new MovieGenre(movies[4].Id, genres[1].Id), // Adventure
                new MovieGenre(movies[4].Id, genres[3].Id)  // Magic
            };

            await context.MovieGenres.AddRangeAsync(movieGenres);

            // Add MovieCountry relationships
            var movieCountries = new List<MovieCountry>
            {
                // All Harry Potter movies: UK, USA
                new MovieCountry(movies[0].Id, countries[0].Id), // UK
                new MovieCountry(movies[0].Id, countries[1].Id), // USA

                new MovieCountry(movies[1].Id, countries[0].Id), // UK
                new MovieCountry(movies[1].Id, countries[1].Id), // USA

                new MovieCountry(movies[2].Id, countries[0].Id), // UK
                new MovieCountry(movies[2].Id, countries[1].Id), // USA

                new MovieCountry(movies[3].Id, countries[0].Id), // UK
                new MovieCountry(movies[3].Id, countries[1].Id), // USA

                new MovieCountry(movies[4].Id, countries[0].Id), // UK
                new MovieCountry(movies[4].Id, countries[1].Id)  // USA
            };

            await context.MovieCountries.AddRangeAsync(movieCountries);

            // Make the regular user the creator of all movies
            var regularUserId = (await userManager.FindByEmailAsync(regularUser.Email)).Id;
            var movieCreators = movies.Select(m => new MovieCreator(regularUserId, m.Id)).ToList();

            await context.MovieCreators.AddRangeAsync(movieCreators);

            // Add some user movie ratings
            var userMovies = new List<UserMovie>
            {
                // Admin user ratings
                new UserMovie { UserId = adminUser.Id, MovieId = movies[0].Id, Rating = 9 },
                new UserMovie { UserId = adminUser.Id, MovieId = movies[1].Id, Rating = 9 },
                new UserMovie { UserId = adminUser.Id, MovieId = movies[2].Id, Rating = 8 },
                
                // Regular user ratings
                new UserMovie { UserId = regularUser.Id, MovieId = movies[0].Id, Rating = 8 },
                new UserMovie { UserId = regularUser.Id, MovieId = movies[3].Id, Rating = 9 },
                new UserMovie { UserId = regularUser.Id, MovieId = movies[4].Id, Rating = 9 }
            };

            await context.UserMovies.AddRangeAsync(userMovies);
            
            await context.SaveChangesAsync();
            Console.WriteLine("Database successfully seeded with initial data.");
        }
    }
}
