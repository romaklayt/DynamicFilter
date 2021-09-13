using System;

namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class Address
    {
        public Address()
        {
        }

        public Address(string street, int? number, Zip zip)
        {
            Street = street;
            Number = number;
            Zip = zip;
        }

        public Guid Id { get; set; } = new();
        public string Street { get; set; }
        public int? Number { get; set; }
        public Zip Zip { get; set; }
    }
}