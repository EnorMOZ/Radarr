using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Datastore
{
    public static class SqlBuilderExtensions
    {
        private const DbType EnumerableMultiParameter = (DbType)(-1);

        private class QueryDetails
        {
            public QueryDetails()
            {
                Parameters = new DynamicParameters();
            }

            public string Table { get; set; }
            public string Field { get; set; }
            public string ParameterName { get; set; }
            public DynamicParameters Parameters { get; set; }

            public string AddParameter(object value, DbType? dbType = null)
            {
                var name = Guid.NewGuid().ToString().Replace("-", "_");
                Parameters.Add(name, value, dbType);
                return name;
            }
        }

        private static QueryDetails GetQueryDetails<TModel>(Expression<Func<TModel, object>> property, object value, DbType? dbType = null)
        {
            var result = new QueryDetails
            {
                Table = TableMapping.Mapper.TableNameMapping(typeof(TModel)),
                Field = property.GetMemberName().Name
            };

            result.ParameterName = result.AddParameter(value, dbType);

            return result;
        }

        public static SqlBuilder SelectAll(this SqlBuilder builder)
        {
            return builder.Select("*");
        }

        public static SqlBuilder SelectCount(this SqlBuilder builder)
        {
            return builder.Select("COUNT(*)");
        }

        public static SqlBuilder WhereEqual<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" = @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder WhereNotEqual<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" != @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder WhereGreaterThanOrEqualTo<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" >= @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder WhereLessThan<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" < @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder WhereIn<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value, EnumerableMultiParameter);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" IN @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder OrWhereIn<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value, EnumerableMultiParameter);

            return builder.OrWhere($"\"{details.Table}\".\"{details.Field}\" IN @{details.ParameterName}", details.Parameters);
        }

        public static SqlBuilder WhereSubstringOf<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"instr(@{details.ParameterName}, \"{details.Table}\".\"{details.Field}\")", details.Parameters);
        }

        public static SqlBuilder WhereContains<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object value)
        {
            var details = GetQueryDetails(property, value);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" LIKE '%' || @{details.ParameterName} || '%'", details.Parameters);
        }

        public static SqlBuilder WhereBetween<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object lower, object upper)
        {
            var details = GetQueryDetails(property, lower);
            var lowerName = details.ParameterName;
            var upperName = details.AddParameter(upper);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" BETWEEN @{lowerName} AND @{upperName}", details.Parameters);
        }

        public static SqlBuilder OrWhereBetween<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property, object lower, object upper)
        {
            var details = GetQueryDetails(property, lower);
            var lowerName = details.ParameterName;
            var upperName = details.AddParameter(upper);

            return builder.OrWhere($"\"{details.Table}\".\"{details.Field}\" BETWEEN @{lowerName} AND @{upperName}", details.Parameters);
        }

        public static SqlBuilder WhereNull<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property)
        {
            var details = GetQueryDetails(property, null);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" IS NULL");
        }

        public static SqlBuilder WhereNotNull<TModel>(this SqlBuilder builder, Expression<Func<TModel, object>> property)
        {
            var details = GetQueryDetails(property, null);

            return builder.Where($"\"{details.Table}\".\"{details.Field}\" IS NOT NULL");
        }

        public static void LogQuery(this SqlBuilder.Template template)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("==== Begin Query Trace ====");
            sb.AppendLine();
            sb.AppendLine("QUERY TEXT:");
            sb.AppendLine(template.RawSql);
            sb.AppendLine();
            sb.AppendLine("PARAMETERS:");
            foreach (var p in ((DynamicParameters)template.Parameters).ToDictionary())
            {
                object val = (p.Value != null && p.Value is string) ? string.Format("\"{0}\"", p.Value) : p.Value;
                sb.AppendFormat("{0} = [{1}]", p.Key, val.ToJson() ?? "NULL").AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("==== End Query Trace ====");
            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }

        private static Dictionary<string, object> ToDictionary(this DynamicParameters dynamicParams)
        {
            var argsDictionary = new Dictionary<string, object>();
            var iLookup = (SqlMapper.IParameterLookup) dynamicParams;

            foreach (var paramName in dynamicParams.ParameterNames)
            {
                var value = iLookup[paramName];
                argsDictionary.Add(paramName, value);
            }

            var templates = dynamicParams.GetType().GetField("templates", BindingFlags.NonPublic | BindingFlags.Instance);
            if (templates != null)
            {
                var list = templates.GetValue(dynamicParams) as List<Object>;
                if (list != null)
                {
                    foreach (var objProps in list.Select(obj => obj.GetPropertyValuePairs().ToList()))
                    {
                        objProps.ForEach(p => argsDictionary.Add(p.Key, p.Value));
                    }
                }
            }

            return argsDictionary;
        }

        private static Dictionary<string, object> GetPropertyValuePairs(this object obj, String[] hidden = null)
        {
            var type = obj.GetType();
            var pairs = hidden == null
                ? type.GetProperties()
                .DistinctBy(propertyInfo => propertyInfo.Name)
                .ToDictionary(
                    propertyInfo => propertyInfo.Name,
                    propertyInfo => propertyInfo.GetValue(obj, null))
                : type.GetProperties()
                .Where(it => !hidden.Contains(it.Name))
                .DistinctBy(propertyInfo => propertyInfo.Name)
                .ToDictionary(
                    propertyInfo => propertyInfo.Name,
                    propertyInfo => propertyInfo.GetValue(obj, null));
            return pairs;
        }
    }
}
