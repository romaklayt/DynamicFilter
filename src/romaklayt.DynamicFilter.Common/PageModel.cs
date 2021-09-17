using System;
using System.Collections.Generic;

namespace romaklayt.DynamicFilter.Common
{
    public class PageModel<T>
    {
        public PageModel(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (pageNumber > TotalPages) throw new IndexOutOfRangeException("Page not found");
            Items = items;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public List<T> Items { get; set; }
    }
}