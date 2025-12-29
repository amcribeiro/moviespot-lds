using MovieSpot.Models;

namespace MovieSpot.Services.Bookings
{
    public interface IBookingService
    {
        /// <summary>
        /// Retrieves all bookings associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose bookings are to be retrieved.</param>
        /// <returns>An enumerable collection of bookings belonging to the specified user.</returns>
        IEnumerable<Booking> GetAllBookingsByUserId(int userId);

        /// <summary>
        /// Retrieves all bookings in the system.
        /// </summary>
        /// <returns>An enumerable collection of all bookings.</returns>
        IEnumerable<Booking> GetAllBookings();

        /// <summary>
        /// Retrieves a specific booking by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <returns>The booking associated with the specified ID.</returns>
        Booking GetBookingById(int id);

        /// <summary>
        /// Creates a new booking in the system.
        /// </summary>
        /// <param name="newBooking">The booking object containing the details for creation.</param>
        /// <returns>The newly created booking object.</returns>
        Booking CreateBooking(Booking newBooking);

        /// <summary>
        /// Creates a new booking along with its associated seats.
        /// Automatically calculates seat prices based on the seat type and session price.
        /// </summary>
        /// <param name="newBooking">The booking object containing the main booking details.</param>
        /// <param name="seatIds">A collection of seat IDs to be included in the booking.</param>
        /// <returns>The newly created booking with associated seats.</returns>
        Booking CreateBookingWithSeats(Booking newBooking, IEnumerable<int> seatIds);

        /// <summary>
        /// Updates an existing booking with new details.
        /// </summary>
        /// <param name="id">The unique identifier of the booking to be updated.</param>
        /// <param name="updatedBooking">The booking object containing the updated details.</param>
        /// <returns>The updated booking object.</returns>
        Booking UpdateBooking(int id, Booking updatedBooking);

        /// <summary>
        /// Sends daily reminder notifications to users with bookings scheduled for the next day.
        /// </summary>
        /// <returns>The number of notifications sent.</returns>
        Task<int> SendDailyRemindersAsync();

        /// <summary>
        /// Serviço responsável por limpar reservas expiradas:
        /// - Marca pagamentos como Expired
        /// - Liberta BookingSeats (lugares)
        /// </summary>
        Task CleanupExpiredBookingsAsync();

        List<PeakHourDto> GetPeakBookingHours();
    }
}
