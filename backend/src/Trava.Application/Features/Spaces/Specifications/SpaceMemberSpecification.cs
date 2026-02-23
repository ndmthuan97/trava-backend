using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Spaces.Specifications;

public class SpaceMemberSpecParam : BaseSpecParam
{
    public Guid? SpaceId { get; set; }
}

public class SpaceMemberSpecification : ISpecification<User>
{
    public Expression<Func<User, bool>> Criteria { get; }
    public Func<IQueryable<User>, IOrderedQueryable<User>>? OrderBy { get; }
    public List<Expression<Func<User, object>>> Includes { get; } = new List<Expression<Func<User, object>>>();
    public Func<IQueryable<User>, IQueryable<User>>? Selector => null;
    public int Skip { get; }
    public int Take { get; }
    
    public SpaceMemberSpecification(SpaceMemberSpecParam param)
    {
        Criteria = BuildCriteria(param);
        OrderBy = BuildOrderBy(param);
        Skip = (param.PageIndex - 1) * param.PageSize;
        Take = param.PageSize;
    }

    private static Expression<Func<User, bool>> BuildCriteria(SpaceMemberSpecParam param)
    {
        return u => (string.IsNullOrWhiteSpace(param.SearchTerm) ||
                    EF.Functions.ILike(u.FullName, $"%{param.SearchTerm}%") ||
                    EF.Functions.ILike(u.Email, $"%{param.SearchTerm}%")) &&
                    (!param.SpaceId.HasValue || 
                     u.SpaceMembers.Any(sm => sm.SpaceId == param.SpaceId) || 
                     u.Spaces.Any(s => s.Id == param.SpaceId));
    }

    private static Func<IQueryable<User>, IOrderedQueryable<User>>? BuildOrderBy(SpaceMemberSpecParam param)
    {
        if (string.IsNullOrWhiteSpace(param.SortBy)) return null;

        bool isDescending = param.SortDirection?.ToLower() == "desc";
        string sortBy = param.SortBy.ToLower();

        return sortBy switch
        {
            "name" => isDescending ? q => q.OrderByDescending(u => u.FullName) : q => q.OrderBy(u => u.FullName),
            "email" => isDescending ? q => q.OrderByDescending(u => u.Email) : q => q.OrderBy(u => u.Email),
            _ => null
        };
    }
}