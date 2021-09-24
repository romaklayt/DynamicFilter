using Newtonsoft.Json.Linq;

namespace romaklayt.DynamicFilter.Parser.Models
{
    public class ObjectConvertInfo
    {
        public object ConvertObject { set; get; }
        public string IncludeProperties { set; get; }
    }
}