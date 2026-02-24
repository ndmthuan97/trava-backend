using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.SpaceInvitations.Specifications
{
    public class InvitationSpecParam : BaseSpecParam
    {
        public Guid? InvitedUserId { get; set; }
        public InvitationStatus? Status { get; set; }
    }

    public class InvitationSpecification : ISpecification<SpaceInvitation>
    {
        public Expression<Func<SpaceInvitation, bool>> Criteria { get; private set; }
        public Func<IQueryable<SpaceInvitation>, IOrderedQueryable<SpaceInvitation>>? OrderBy { get; private set; }
        public List<Expression<Func<SpaceInvitation, object>>> Includes { get; private set; } = new List<Expression<Func<SpaceInvitation, object>>>();
        public Func<IQueryable<SpaceInvitation>, IQueryable<SpaceInvitation>>? Selector => null;
        public int Skip { get; private set; }
        public int Take { get; private set; }

        public InvitationSpecification(InvitationSpecParam param)
        {
            Criteria = BuildCriteria(param);
            OrderBy = q => q.OrderByDescending(x => x.Id); // Default sort by newest? Wait, Id is Guid. Maybe create dynamic sort if needed.
            Skip = (param.PageIndex - 1) * param.PageSize;
            Take = param.PageSize;
            Includes.Add(x => x.Space);
        }

        private static Expression<Func<SpaceInvitation, bool>> BuildCriteria(InvitationSpecParam param)
        {
            return x => (!param.InvitedUserId.HasValue || x.InvitedUserId == param.InvitedUserId.Value) &&
                        (!param.Status.HasValue || x.Status == param.Status.Value) &&
                        (string.IsNullOrWhiteSpace(param.SearchTerm) || EF.Functions.ILike(x.Space.Name, $"%{param.SearchTerm}%"));
        }
    }
}
