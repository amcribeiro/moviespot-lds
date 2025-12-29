using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MovieSpot.Data;
using MovieSpot.Models;
using Xunit;

using ReviewModel = MovieSpot.Models.Review;

namespace MovieSpot.Tests.Services.Reviews
{
    public class ReviewServiceTest
    {
        private static ApplicationDbContext NewCtx()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private static void SeedBookingOnly(ApplicationDbContext ctx, int bookingId = 100, int userId = 1, int sessionId = 1)
        {
            if (!ctx.User.Any(u => u.Id == userId))
                ctx.User.Add(new User { Id = userId, Name = $"User{userId}", Email = $"u{userId}@x.com" });

            if (!ctx.Movie.Any(m => m.Id == 10))
                ctx.Movie.Add(new Movie
                {
                    Id = 10,
                    Title = "Matrix",
                    Description = "Desc",
                    Duration = 120,
                    ReleaseDate = DateTime.UtcNow.Date,
                    Language = "en",
                    Country = "US"
                });

            if (!ctx.Cinema.Any(c => c.Id == 1))
                ctx.Cinema.Add(new Cinema
                {
                    Id = 1,
                    Name = "CineX",
                    Street = "S",
                    City = "C",
                    State = "ST",
                    ZipCode = "0000-000",
                    Country = "PT",
                    Latitude = 0,
                    Longitude = 0,
                    CreatedAt = DateTime.UtcNow
                });

            if (!ctx.CinemaHall.Any(ch => ch.Id == 1))
                ctx.CinemaHall.Add(new CinemaHall { Id = 1, Name = "Sala 1", CinemaId = 1, CreatedAt = DateTime.UtcNow });

            if (!ctx.Session.Any(s => s.Id == sessionId))
                ctx.Session.Add(new Session
                {
                    Id = sessionId,
                    MovieId = 10,
                    CinemaHallId = 1,
                    CreatedBy = userId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddHours(2),
                    Price = 5
                });

            if (!ctx.Booking.Any(b => b.Id == bookingId))
                ctx.Booking.Add(new Booking
                {
                    Id = bookingId,
                    UserId = userId,
                    SessionId = sessionId,
                    BookingDate = DateTime.UtcNow,
                    TotalAmount = 10,
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            ctx.SaveChanges();
        }

        private static ReviewModel MakeReview(int bookingId, int rating = 4, string comment = "Bom!")
            => new ReviewModel
            {
                BookingId = bookingId,
                Rating = rating,
                Comment = comment
            };

        [Fact]
        public void GetAllReviews_WhenEmpty_ThrowsInvalidOperation()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<InvalidOperationException>(() => sut.GetAllReviews());
        }

        [Fact]
        public void GetAllReviews_ReturnsAll_WithBookingIncluded()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);
            SeedBookingOnly(ctx, bookingId: 2, userId: 2, sessionId: 2);

            ctx.Review.AddRange(
                new ReviewModel { Id = 10, BookingId = 1, Rating = 5, Comment = "Top", ReviewDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ReviewModel { Id = 11, BookingId = 2, Rating = 3, Comment = "Ok", ReviewDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);
            var list = sut.GetAllReviews().ToList();

            Assert.Equal(2, list.Count);
            Assert.All(list, r => Assert.NotNull(r.Booking));
            Assert.Contains(list, r => r.Id == 10 && r.BookingId == 1);
            Assert.Contains(list, r => r.Id == 11 && r.BookingId == 2);
        }

        [Fact]
        public void GetAllReviewsByUserId_IdLessOrEqualZero_Throws()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetAllReviewsByUserId(0));
        }

        [Fact]
        public void GetAllReviewsByUserId_WhenNone_ThrowsInvalidOperation()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1, userId: 5);
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<InvalidOperationException>(() => sut.GetAllReviewsByUserId(5));
        }

        [Fact]
        public void GetAllReviewsByUserId_ReturnsOnlyThatUsersReviews()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1, userId: 1);
            SeedBookingOnly(ctx, bookingId: 2, userId: 2, sessionId: 2);

            ctx.Review.AddRange(
                new ReviewModel { Id = 10, BookingId = 1, Rating = 5, Comment = "User1", ReviewDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ReviewModel { Id = 11, BookingId = 2, Rating = 2, Comment = "User2", ReviewDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);
            var list = sut.GetAllReviewsByUserId(1).ToList();

            Assert.Single(list);
            Assert.Equal(10, list[0].Id);
            Assert.Equal(1, list[0].Booking!.UserId);
        }

        [Fact]
        public void GetReviewById_IdLessOrEqualZero_Throws()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetReviewById(0));
        }

        [Fact]
        public void GetReviewById_NotFound_ThrowsKeyNotFound()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<KeyNotFoundException>(() => sut.GetReviewById(999));
        }

        [Fact]
        public void GetReviewById_ReturnsReview_WithBookingIncluded()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);
            ctx.Review.Add(new ReviewModel { Id = 10, BookingId = 1, Rating = 4, Comment = "Ok", ReviewDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);
            var r = sut.GetReviewById(10);

            Assert.NotNull(r);
            Assert.Equal(1, r.BookingId);
            Assert.NotNull(r.Booking);
        }

        [Fact]
        public void CreateReview_Null_ThrowsArgumentNull()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<ArgumentNullException>(() => sut.CreateReview(null!));
        }

        [Fact]
        public void CreateReview_BookingDoesNotExist_ThrowsKeyNotFound()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            var review = MakeReview(bookingId: 999);
            Assert.Throws<KeyNotFoundException>(() => sut.CreateReview(review));
        }

        [Fact]
        public void CreateReview_BookingAlreadyHasReview_ThrowsInvalidOperation()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);

            ctx.Review.Add(new ReviewModel
            {
                Id = 10,
                BookingId = 1,
                Rating = 5,
                Comment = "Primeira",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            var ex = Assert.Throws<InvalidOperationException>(() => sut.CreateReview(MakeReview(1, 3, "Segunda")));
            Assert.Contains("apenas uma review", ex.Message);
        }

        [Fact]
        public void CreateReview_SetsTimestamps_AndPersists()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            var before = DateTime.UtcNow;
            var created = sut.CreateReview(MakeReview(1, 5, "Excelente"));

            Assert.True(created.Id > 0);
            Assert.True(created.ReviewDate != default);
            Assert.True(created.CreatedAt >= before && created.CreatedAt <= DateTime.UtcNow);
            Assert.Equal(created.CreatedAt, created.UpdatedAt);
            Assert.Equal(1, ctx.Review.Count());
        }

        [Fact]
        public void UpdateReview_IdLessOrEqualZero_Throws()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.UpdateReview(0, new ReviewModel()));
        }

        [Fact]
        public void UpdateReview_NullUpdated_Throws()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<ArgumentNullException>(() => sut.UpdateReview(1, null!));
        }

        [Fact]
        public void UpdateReview_NotFound_ThrowsKeyNotFound()
        {
            using var ctx = NewCtx();
            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            Assert.Throws<KeyNotFoundException>(() => sut.UpdateReview(123, new ReviewModel()));
        }

        [Fact]
        public void UpdateReview_AttemptToChangeBooking_ThrowsInvalidOperation()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);
            SeedBookingOnly(ctx, bookingId: 2, userId: 2, sessionId: 2);

            ctx.Review.Add(new ReviewModel
            {
                Id = 10,
                BookingId = 1,
                Rating = 2,
                Comment = "meh",
                ReviewDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            var updated = new ReviewModel { BookingId = 2, Rating = 5, Comment = "trocar booking" };
            Assert.Throws<InvalidOperationException>(() => sut.UpdateReview(10, updated));
        }

        [Fact]
        public void UpdateReview_ChangesFields_AndUpdatesTimestamp()
        {
            using var ctx = NewCtx();
            SeedBookingOnly(ctx, bookingId: 1);

            var createdAt = DateTime.UtcNow.AddMinutes(-5);
            var updatedAtBefore = DateTime.UtcNow.AddMinutes(-5);

            ctx.Review.Add(new ReviewModel
            {
                Id = 10,
                BookingId = 1,
                Rating = 2,
                Comment = "meh",
                ReviewDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = createdAt,
                UpdatedAt = updatedAtBefore
            });
            ctx.SaveChanges();

            var sut = new MovieSpot.Services.Reviews.ReviewService(ctx);

            var patch = new ReviewModel
            {
                BookingId = 1,
                Rating = 4,
                Comment = "Agora melhor",
                ReviewDate = DateTime.UtcNow
            };

            var result = sut.UpdateReview(10, patch);

            Assert.Equal(4, result.Rating);
            Assert.Equal("Agora melhor", result.Comment);
            Assert.True(result.UpdatedAt > updatedAtBefore);
            Assert.Equal(createdAt, result.CreatedAt);
            Assert.Equal(1, result.BookingId);
        }
    }
}
