namespace romaklayt.DynamicFilter.Test.Api.Models
{
    public class Role
    {
        public Role(int? id, string name)
        {
            Id = id;
            Name = name;
        }

        public int? Id { get; set; }
        public string Name { get; set; }
    }
}