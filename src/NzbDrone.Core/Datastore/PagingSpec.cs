using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;

namespace NzbDrone.Core.Datastore
{
    public class PagingSpec<TModel> where TModel : ModelBase
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public string SortKey { get; set; }
        public SortDirection SortDirection { get; set; }
        public List<TModel> Records { get; set; }
        public List<PagingFilter<TModel>> FilterExpressions { get; set; }

        public PagingSpec()
        {
            FilterExpressions = new List<PagingFilter<TModel>>();
        }
    }

    public enum SortDirection
    {
        Default,
        Ascending,
        Descending
    }

    public abstract class PagingFilter<TModel> where TModel : ModelBase
    {
        public DynamicParameters Parameters { get; private set; }

        protected string _table;
        protected string _id;

        public PagingFilter()
        {
            Parameters = new DynamicParameters();
            _table = TableMapping.Mapper.TableNameMapping(typeof(TModel));
            _id = Guid.NewGuid().ToString().Replace("-", "_");
        }

        public abstract void ApplyToBuilder(SqlBuilder builder);
    }

    public class WhereEqualPagingFilter<TModel> : PagingFilter<TModel> where TModel : ModelBase
    {
        public PropertyInfo Property { get; private set; }

        public WhereEqualPagingFilter(Expression<Func<TModel, object>> property, object values) : base()
        {
            Property = property.GetMemberName();
            Parameters.Add(_id, values);
        }

        public override void ApplyToBuilder(SqlBuilder builder)
        {
            builder.Where($"[{_table}].[{Property.Name}] = @{_id}", Parameters);
        }
    }

    public class WhereInPagingFilter<TModel> : WhereEqualPagingFilter<TModel> where TModel : ModelBase
    {
        public WhereInPagingFilter(Expression<Func<TModel, object>> property, object values) : base(property, values)
        {
        }

        public override void ApplyToBuilder(SqlBuilder builder)
        {
            builder.Where($"[{_table}].[{Property.Name}] IN @{_id}", Parameters);
        }

    }
}
