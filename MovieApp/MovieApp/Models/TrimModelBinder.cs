using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;

namespace System.Web.Mvc
{
    public class TrimModelBinder : IModelBinder
    {
        public object BindModel(System.Web.Mvc.ControllerContext controllerContext,
        ModelBindingContext bindingContext)
        {
            ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (result == null || result.AttemptedValue == null)
                return null;
            else if (result.AttemptedValue == string.Empty)
                return string.Empty;
            return result.AttemptedValue.Trim();
        }
    }
}