namespace romaklayt.DynamicFilter.Common
{
    public class BaseDynamicFilter
    {
        public string Filter { get; set; }
        public string Order { get; set; }
        public string Select { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool AsNoTracking { get; set; } = true;
    }
}