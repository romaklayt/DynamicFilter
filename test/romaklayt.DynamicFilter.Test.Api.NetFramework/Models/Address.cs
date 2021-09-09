namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Models
{
    public class Address
    {
        public Address(string street, int? number, Zip zip)
        {
            Street = street;
            Number = number;
            Zip = zip;
        }

        public string Street { get; set; }
        public int? Number { get; set; }
        public Zip Zip { get; }
    }
}