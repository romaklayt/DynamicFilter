using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace romaklayt.DynamicFilter.Binder
{
    public class DynamicFilterBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType.FullName.Contains("DynamicFilter"))
                return new BinderTypeModelBinder(typeof(DynamicFilterBinder));

            return null;
        }
    }
}