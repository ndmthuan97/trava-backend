using System.Linq.Expressions;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Application.Features.Spaces.Specifications;
using Trava.Domain.Entities;

namespace Trava.Application.Features.Users.Specifications;

public class UserSpecParam : BaseSpecParam
{

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
        return u => string.IsNullOrWhiteSpace(param.SearchTerm) ||
                    EF.Functions.ILike(u.FullName, $"%{param.SearchTerm}%") ||
                    EF.Functions.ILike(u.Email, $"%{param.SearchTerm}%");
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