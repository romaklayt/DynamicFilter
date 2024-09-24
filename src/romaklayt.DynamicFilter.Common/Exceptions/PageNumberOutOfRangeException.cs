using System;

namespace romaklayt.DynamicFilter.Common.Exceptions;

[Serializable]
public class PageNumberOutOfRangeException(string message) : Exception(message);