﻿using System;
using System.Collections.Generic;
using System.Linq;
using romaklayt.DynamicFilter.Common.Exceptions;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Common;

[Serializable]
public class PageModel<T> : IPageInfo
{
    public PageModel()
    {
    }

    public PageModel(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new PageNumberOutOfRangeException($"The {nameof(pageNumber)} value cannot be less than 1");
        if (pageSize < 1)
            throw new PageNumberOutOfRangeException($"The {nameof(PageSize)} value cannot be less than 1");
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        if (pageNumber > TotalPages && TotalCount > 0) throw new PageNumberOutOfRangeException("Page not found");
        Items = items.ToList();
    }

    public List<T> Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}