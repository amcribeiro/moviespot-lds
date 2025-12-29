namespace MovieSpot.Models
{
    public class Invoice
    {
        public string BookingId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime PaymentDate { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime SessionStart { get; set; }
        public string CinemaHall { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "MovieSpot";
        public string CompanyAddress { get; set; } = "Rua do Cinema, 123, Lisboa";
        public string CompanyNIF { get; set; } = "PT509999999";
        public List<Seat> Seats { get; set; } = new();
    }
}