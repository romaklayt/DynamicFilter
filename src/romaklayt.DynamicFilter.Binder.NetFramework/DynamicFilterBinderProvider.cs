using System;
using System.Web.Mvc;

namespace romaklayt.DynamicFilter.Binder.NetFramework
{
    public class DynamicFilterBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));

            if (modelType.FullName != null && modelType.FullName.Contains("DynamicFilterModel"))
                return new DynamicFilterBinder();

            return null;
        }
    }
}