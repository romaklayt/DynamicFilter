namespace romaklayt.DynamicFilter.Common
{
    public class BaseDynamicFilter : BaseCountDynamicFilter
    {
        public string Order { get; set; }
        public string Select { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}