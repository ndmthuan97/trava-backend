using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Application.Common.Models
{
    public class Pagination<T> where T : class
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long Count { get; set; }
        public IReadOnlyCollection<T> Data { get; set; } = [];

        public Pagination() { }

        public Pagination(int pageIndex, int pageSize, int count, IReadOnlyCollection<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }
    }
}