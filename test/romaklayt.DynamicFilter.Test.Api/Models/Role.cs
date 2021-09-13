using System;

namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class Role
    {
        public Role()
        {
            
        }
        public Role(string name)
        {
            Name = name;
        }

        public Guid Id { get; set; } = new Guid();
        public string Name { get; set; }
    }
}