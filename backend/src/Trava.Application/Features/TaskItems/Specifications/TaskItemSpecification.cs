using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trava.Application.Common.Specifications;
using Trava.Domain.Entities;
using Trava.Domain.Enums;

namespace Trava.Application.Features.TaskItems.Specifications
{
    public class TaskItemSpecParam : BaseSpecParam
    {
        public Guid SpaceId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public List<TaskItemStatus>? Statuses { get; set; }
        public List<TaskItemPriority>? Priorities { get; set; }
    }

    public class TaskItemSpecification : ISpecification<TaskItem>
    {
        public Expression<Func<TaskItem, bool>> Criteria { get; private set; }
        public Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>? OrderBy { get; private set; }
        public List<Expression<Func<TaskItem, object>>> Includes { get; private set; } = new List<Expression<Func<TaskItem, object>>>();
        public Func<IQueryable<TaskItem>, IQueryable<TaskItem>>? Selector => null;
        public int Skip { get; private set; }
        public int Take { get; private set; }

        public TaskItemSpecification(TaskItemSpecParam param)
        {
            Criteria = x => x.SpaceId == param.SpaceId &&
                            (string.IsNullOrWhiteSpace(param.SearchTerm) || EF.Functions.ILike(x.Title, $"%{param.SearchTerm}%")) &&
                            (!param.AssignedUserId.HasValue || x.AssignedUserId == param.AssignedUserId) &&
                            (param.Statuses == null || !param.Statuses.Any() || param.Statuses.Contains(x.Status)) &&
                            (param.Priorities == null || !param.Priorities.Any() || param.Priorities.Contains(x.Priority));

            OrderBy = BuildOrderBy(param);
            Skip = (param.PageIndex - 1) * param.PageSize;
            Take = param.PageSize;
            Includes.Add(x => x.Creator);
        }

        private static Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>? BuildOrderBy(TaskItemSpecParam param)
        {
            if (string.IsNullOrWhiteSpace(param.SortBy)) return q => q.OrderByDescending(x => x.CreatedAt);

            bool isDescending = param.SortDirection?.ToLower() == "desc";
            string sortBy = param.SortBy.ToLower();

            return sortBy switch
            {
                "title" => isDescending ? q => q.OrderByDescending(x => x.Title) : q => q.OrderBy(x => x.Title),
                "priority" => isDescending ? q => q.OrderByDescending(x => x.Priority) : q => q.OrderBy(x => x.Priority),
                "status" => isDescending ? q => q.OrderByDescending(x => x.Status) : q => q.OrderBy(x => x.Status),
                _ => q => q.OrderByDescending(x => x.CreatedAt)
            };
        }
    }
}
