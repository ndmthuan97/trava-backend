using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Specifications
{
    public class SpaceSpecParam : BaseSpecParam
    {
        public Guid? UserId { get; set; }
    }

    public class SpaceSpecification : ISpecification<Space>
    {
        public Expression<Func<Space, bool>> Criteria { get; private set; }
        public Func<IQueryable<Space>, IOrderedQueryable<Space>>? OrderBy { get; private set; }
        public List<Expression<Func<Space, object>>> Includes { get; private set; } = new List<Expression<Func<Space, object>>>();
        public Func<IQueryable<Space>, IQueryable<Space>>? Selector => null;
        public int Skip { get; private set; }
        public int Take { get; private set; }

        public SpaceSpecification(SpaceSpecParam param)
        {
            Criteria = BuildCriteria(param);
            OrderBy = BuildOrderBy(param);
            Skip = (param.PageIndex - 1) * param.PageSize;
            Take = param.PageSize;
            Includes.Add(x => x.Members);
        }

        private static Expression<Func<Space, bool>> BuildCriteria(SpaceSpecParam param)
        {
            return s => (string.IsNullOrWhiteSpace(param.SearchTerm) ||
                        EF.Functions.ILike(s.Name, $"%{param.SearchTerm}%")) &&
                        (!param.UserId.HasValue || (s.CreatedBy == param.UserId.Value || s.Members.Any(sm => sm.UserId == param.UserId.Value)));

        }
        private static Func<IQueryable<Space>, IOrderedQueryable<Space>>? BuildOrderBy(SpaceSpecParam param)
        {
            if (string.IsNullOrWhiteSpace(param.SortBy)) return null;

            bool isDescending = param.SortDirection?.ToLower() == "desc";
            string sortBy = param.SortBy.ToLower();

            return sortBy switch
            {
                "name" => isDescending ? q => q.OrderByDescending(x => x.Name) :
                    q => q.OrderBy(x => x.Name),

                "spacetype" => isDescending ? q => q.OrderByDescending(x => x.SpaceType) :
                    q => q.OrderBy(x => x.SpaceType),

                _ => null
            };
        }
    }
}