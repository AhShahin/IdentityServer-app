namespace WebClient.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string TelephoneCell { get; set; }
        public string TelephoneHome { get; set; }
        public DateTime DoB { get; set; }
        public string KnownAs { get; set; }
        public string Type { get; set; }
        public int NumberOfLogons { get; set; }
        public DateTime LastActive { get; set; }

    }
}
