namespace MovieSpot.Services.Notifications
{
    public class FcmOptions
    {
        public const string SectionName = "Fcm";
        public string CredentialsPath { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }
}
