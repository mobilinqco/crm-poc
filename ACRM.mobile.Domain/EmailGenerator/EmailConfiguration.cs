namespace ACRM.mobile.Domain.EmailGenerator
{
    public class EmailConfiguration
    {
        public string SmtpClient { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
