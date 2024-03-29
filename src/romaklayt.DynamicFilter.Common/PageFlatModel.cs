﻿using System;
using System.Collections.Generic;
using romaklayt.DynamicFilter.Common.Exceptions;
using romaklayt.DynamicFilter.Common.Interfaces;

namespace romaklayt.DynamicFilter.Common;

[Serializable]
public class PageFlatModel<T> : List<T>, IPageInfo
{
    public PageFlatModel()
    {
    }

    public PageFlatModel(List<T> items, int count, int pageNumber, int pageSize)
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
        AddRange(items);
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}