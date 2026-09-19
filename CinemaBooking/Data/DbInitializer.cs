using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            try
            {
                context.Database.EnsureCreated();

                // Seed Cinemas if none exist
                if (!context.Cinemas.Any())
                {
                    var mallOfEgypt = new Cinema
                    {
                        Name = "StarLight Mall of Egypt",
                        City = "Giza",
                        Address = "Gate 4, Level 2, Mall of Egypt, Wahat Road, 6th of October City, Giza",
                        Latitude = 29.9723m,
                        Longitude = 31.0189m
                    };

                    var newCairo = new Cinema
                    {
                        Name = "StarLight New Cairo",
                        City = "New Cairo",
                        Address = "The Promenade, Cairo Festival City Mall, Ring Road, New Cairo",
                        Latitude = 30.0298m,
                        Longitude = 31.4087m
                    };

                    var alexandria = new Cinema
                    {
                        Name = "StarLight Alexandria",
                        City = "Alexandria",
                        Address = "San Stefano Grand Plaza, 3rd Floor, El-Geish Road, Alexandria",
                        Latitude = 31.2443m,
                        Longitude = 29.9686m
                    };

                    context.Cinemas.AddRange(mallOfEgypt, newCairo, alexandria);
                    context.SaveChanges();

                    // Seed Halls for the cinemas
                    var halls = new List<Hall>
                    {
                        new Hall { Name = "IMAX Laser Auditorium 1", CinemaId = mallOfEgypt.Id, SeatCount = 280 },
                        new Hall { Name = "Dolby Atmos Hall 2", CinemaId = mallOfEgypt.Id, SeatCount = 200 },
                        new Hall { Name = "VIP Luxe Lounge 1", CinemaId = newCairo.Id, SeatCount = 120 },
                        new Hall { Name = "Dual 4K Laser Hall 2", CinemaId = newCairo.Id, SeatCount = 220 },
                        new Hall { Name = "Sea View Premiere Hall", CinemaId = alexandria.Id, SeatCount = 160 }
                    };
                    context.Halls.AddRange(halls);
                    context.SaveChanges();

                    // Seed sample movies
                    if (!context.Movies.Any())
                    {
                        var dune = new Movie
                        {
                            Title = "Dune: Part Two",
                            Genre = "Sci-Fi / Adventure",
                            DurationMinutes = 166,
                            Description = "Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.",
                            Rating = "PG-13",
                            PosterUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?auto=format&fit=crop&w=600&q=80",
                            TrailerUrl = "https://www.youtube.com/watch?v=Way9Dexny3w",
                            ReleaseStatus = "Now Showing",
                            TmdbId = 693134
                        };

                        var interstellar = new Movie
                        {
                            Title = "Interstellar: 10th Anniversary IMAX",
                            Genre = "Sci-Fi / Drama",
                            DurationMinutes = 169,
                            Description = "When Earth becomes uninhabitable in the future, a farmer and ex-NASA pilot is tasked to pilot a spacecraft along with a team of researchers to find a new planet for humans.",
                            Rating = "PG-13",
                            PosterUrl = "https://images.unsplash.com/photo-1506703719100-a0f3a48c0f86?auto=format&fit=crop&w=600&q=80",
                            TrailerUrl = "https://www.youtube.com/watch?v=zSWdZVtXT7E",
                            ReleaseStatus = "Now Showing",
                            TmdbId = 157336
                        };

                        var gladiator = new Movie
                        {
                            Title = "Gladiator II",
                            Genre = "Action / Epic",
                            DurationMinutes = 148,
                            Description = "Years after witnessing the death of the revered hero Maximus at the hands of his uncle, Lucius must enter the Colosseum after his home is conquered by tyrannical emperors.",
                            Rating = "R",
                            PosterUrl = "https://images.unsplash.com/photo-1579783902614-a3fb3927b675?auto=format&fit=crop&w=600&q=80",
                            TrailerUrl = "https://www.youtube.com/watch?v=4rgYUipGJNo",
                            ReleaseStatus = "Now Showing",
                            TmdbId = 558449
                        };

                        context.Movies.AddRange(dune, interstellar, gladiator);
                        context.SaveChanges();

                        // Seed showtimes linking movies and cinemas
                        var today = DateOnly.FromDateTime(DateTime.Today);
                        var showtimes = new List<Showtime>
                        {
                            new Showtime { MovieId = dune.Id, CinemaId = mallOfEgypt.Id, HallId = halls[0].Id, Date = today, Time = new TimeOnly(14, 0), BasePrice = 180m },
                            new Showtime { MovieId = dune.Id, CinemaId = mallOfEgypt.Id, HallId = halls[0].Id, Date = today, Time = new TimeOnly(18, 30), BasePrice = 220m },
                            new Showtime { MovieId = interstellar.Id, CinemaId = newCairo.Id, HallId = halls[2].Id, Date = today, Time = new TimeOnly(16, 15), BasePrice = 200m },
                            new Showtime { MovieId = gladiator.Id, CinemaId = alexandria.Id, HallId = halls[4].Id, Date = today, Time = new TimeOnly(19, 0), BasePrice = 190m }
                        };
                        context.Showtimes.AddRange(showtimes);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and allow graceful operation
                Console.WriteLine($"[DbInitializer] Notice: {ex.Message}");
            }
        }
    }
}
