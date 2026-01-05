using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Trava.Application.Common.Models;

namespace Trava.Application.Common.Mappings
{
    public class PaginationMapping<TSource, TDestination> : ITypeConverter<Pagination<TSource>, Pagination<TDestination>> where TSource : class where TDestination : class
    {
        public Pagination<TDestination> Convert(
        Pagination<TSource> source,
        Pagination<TDestination> destination,
        ResolutionContext context)
        {
            var mappedData = source.Data
                .Select(item => context.Mapper.Map<TDestination>(item))
                .ToList();

            return new Pagination<TDestination>
            {
                PageIndex = source.PageIndex,
                PageSize = source.PageSize,
                Count = source.Count,
                Data = mappedData
            };
        }
    }
}