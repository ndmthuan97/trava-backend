using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.Users.Specifications;

public class UserSpecParam : BaseSpecParam
{
    public Role? Role { get; set; }
    public UserStatus? Status { get; set; }
}

public class UserSpecification : ISpecification<User>
{
    public Expression<Func<User, bool>> Criteria { get; private set; }
    public Func<IQueryable<User>, IOrderedQueryable<User>>? OrderBy { get; private set; }

    public List<Expression<Func<User, object>>> Includes { get; private set; } =
        new List<Expression<Func<User, object>>>();
    public Func<IQueryable<User>, IQueryable<User>>? Selector => null;
    public int Skip { get; private set; }
    public int Take { get; private set; }

    public UserSpecification(UserSpecParam param)
    {
        Criteria = BuildCriteria(param);
        OrderBy = BuildOrderBy(param);
        Skip = (param.PageIndex - 1) * param.PageSize;
        Take = param.PageSize;
    }

    private static Expression<Func<User, bool>> BuildCriteria(UserSpecParam param)
    {
        return u => (string.IsNullOrWhiteSpace(param.SearchTerm) ||
                    EF.Functions.ILike(u.FullName ?? string.Empty, $"%{param.SearchTerm}%") ||
                    EF.Functions.ILike(u.Email ?? string.Empty, $"%{param.SearchTerm}%")) &&
                    (!param.Role.HasValue || u.Role == param.Role.Value) &&
                    (!param.Status.HasValue || u.Status == param.Status.Value);
    }

    private static Func<IQueryable<User>, IOrderedQueryable<User>>? BuildOrderBy(UserSpecParam param)
    {
        if (string.IsNullOrWhiteSpace(param.SortBy)) return null;

        bool isDescending = param.SortDirection?.ToLower() == "desc";
        string sortedBy = param.SortBy.ToLower();

        return sortedBy switch
        {
            "name" => isDescending ? u => u.OrderByDescending(x => x.FullName) :
                u => u.OrderBy(x => x.FullName),
            _ => null
        };
    }
}