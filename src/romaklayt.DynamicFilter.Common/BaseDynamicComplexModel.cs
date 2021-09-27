namespace romaklayt.DynamicFilter.Common
{
    public class BaseDynamicComplexModel : BaseDynamicFilterModel
    {
        public string Order { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Select { get; set; }
    }
}