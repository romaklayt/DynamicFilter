namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class Zip
    {
        public Zip(int number, string country)
        {
            Number = number;
            Country = country;
        }

        public string Country { get; set; }
        public int Number { get; set; }
    }
}