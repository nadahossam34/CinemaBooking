using System;
using System.Collections.Generic;
using System.Text;

using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. تحديد المفتاح المركب (Composite Primary Key) لجدول BookingSeat
            modelBuilder.Entity<BookingSeat>()
                .HasKey(bs => new { bs.BookingId, bs.SeatId });

            // 2. علاقة One-to-One بين Booking و Payment
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. منع مشاكل Multiple Cascade Paths في الـ Foreign Keys
            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Cinema)
                .WithMany()
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Showtime)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. تحديد دقة أرقام الـ Decimal لتجنب التحذيرات
            modelBuilder.Entity<Showtime>()
                .Property(s => s.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BookingSeat>()
                .Property(bs => bs.PricePaid)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SeatType>()
                .Property(st => st.PriceMultiplier)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cinema>()
                .Property(c => c.Latitude)
                .HasColumnType("decimal(9,6)");

            modelBuilder.Entity<Cinema>()
                .Property(c => c.Longitude)
                .HasColumnType("decimal(9,6)");


        }
    }
}
