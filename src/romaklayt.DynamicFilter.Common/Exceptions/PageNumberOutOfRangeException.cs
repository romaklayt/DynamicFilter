using System;

namespace romaklayt.DynamicFilter.Common.Exceptions;

[Serializable]
public class PageNumberOutOfRangeException : Exception
{
    public PageNumberOutOfRangeException(string message) : base(message)
    {
    }
}