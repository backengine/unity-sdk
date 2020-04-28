using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BE.Models
{
    public class RequestData
    {
        public List<string> Filters { get; private set; }
        public Dictionary<string, object> Fields { get; private set; }
        public int Page { get; private set; } = 0;
        public int PageSize { get; private set; } = 0;

        public Dictionary<string, SortType> Sorts { get; set; }

        public ConditionBase Condition { get; set; }

        public RequestData()
        {
        }
        public RequestData Where(QueryableFunc callback)
        {
            var builder = new ConditionBuilder();
            var value = callback(builder);
            if (Condition == null)
            {
                Condition = value;
            }
            else
            {
                var v = Condition;
                Condition = v & value;
            }
            return this;
        }
        /// <summary>
        /// Add Request Filter
        /// </summary>
        /// <param name="fieldNames">list name of fields want to get, split by "," </param>
        public RequestData GetFields(string fieldNames)
        {
            if (Filters == null) Filters = new List<string>();
            Filters.AddRange(fieldNames.Split(','));
            return this;
        }

        /// <summary>
        /// Add Request Filter
        /// </summary>
        /// <param name="fieldNames">name of fields want to get the reference object, split by ","</param>
        public RequestData GetRefs(string fieldNames)
        {
            if (Filters == null) Filters = new List<string>();
            foreach (string st in fieldNames.Split(','))
            {
                Filters.Add("*" + st);
            }
            return this;
        }

        /// <summary>
        /// Add field to request
        /// </summary>
        /// <param name="fieldName">Name of field</param>
        /// <param name="value">value of field</param>
        public RequestData SetValue(string fieldName, object value)
        {
            if (Fields == null) Fields = new Dictionary<string, object>();
            Fields.Add(fieldName, value);
            return this;
        }

        /// <summary>
        /// Set page and pageSize for select request
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="pageSize">page size</param>
        public RequestData Take(int page, int pageSize)
        {
            this.Page = page;
            this.PageSize = pageSize;
            return this;
        }

        public RequestData Sort(string fieldName, SortType sortType)
        {
            if (Sorts == null) Sorts = new Dictionary<string, SortType>();
            Sorts.Add(fieldName, sortType);
            return this;
        }

    }
    public static class RequestHelper
    {
        public static RequestData Where<T>(this RequestData source, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = new ConditionExpressionBulder<T>(predicate);
            var value = builder.ToCondition();
            if (source.Condition == null)
            {
                source.Condition = value;
            }
            else
            {
                var v = source.Condition;
                source.Condition = v & value;
            }
            return source;
        }
    }
}
