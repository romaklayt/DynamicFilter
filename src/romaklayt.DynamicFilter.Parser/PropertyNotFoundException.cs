using System;

namespace romaklayt.DynamicFilter.Parser
{
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(string propertyName, string className) : base($"Property {propertyName} was not found on {className}")
        {

        }
    }
}