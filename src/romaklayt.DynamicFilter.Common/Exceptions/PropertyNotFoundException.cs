using System;

namespace romaklayt.DynamicFilter.Common.Exceptions;

[Serializable]
public class PropertyNotFoundException(string propertyName, string className) : Exception($"Property {propertyName} was not found on {className}");