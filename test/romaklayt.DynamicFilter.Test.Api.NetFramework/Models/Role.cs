namespace romaklayt.DynamicFilter.Test.Api.NetFramework.Models
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