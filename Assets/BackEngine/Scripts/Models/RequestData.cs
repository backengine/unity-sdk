using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
        public  RequestData Where(QueryableFunc callback)
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

        public RequestData Sort(string fieldName,  SortType sortType = SortType.Asc)
        {
            if (Sorts == null) Sorts = new Dictionary<string, SortType>();
            Sorts.Add(fieldName, sortType);
            return this;
        }

    }

    public class RequestData<T>: RequestData where T : class
    {
        
        public RequestData()
        {
        }
        public  RequestData<T> Where( Expression<Func<T, bool>> predicate)
        {
            var builder = new ConditionExpressionBulder<T>(predicate);
            var value = builder.ToCondition();
            if (this.Condition == null)
            {
                this.Condition = value;
            }
            else
            {
                var v = this.Condition;
                this.Condition = v & value;
            }
            return this;
        }
        /// <summary>
        /// Add Request Filter
        /// </summary>
        /// <param name="fieldNames">list name of fields want to get, split by "," </param>
        public RequestData<T> GetField<P>(Expression<Func<T,P>> expression)
        {
            string propertyName = GetMemberName(expression);
            GetFields(propertyName);
            return this;
        }
        private string GetMemberName<P>(Expression<Func<T, P>> expression)
        {
            MemberInfo memberInfo;
            if (expression.Body is UnaryExpression)
            {
                memberInfo = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member;
            }
            else
            {
                memberInfo = ((MemberExpression)expression.Body).Member;
            }
            if (memberInfo is PropertyInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (Type.GetTypeCode(propertyInfo.PropertyType) == TypeCode.Object)
                {
                    throw new Exception("This is not a table field!");
                }
            }
            else if (memberInfo is FieldInfo)
            {
                var propertyInfo = memberInfo as FieldInfo;
                if (Type.GetTypeCode(propertyInfo.FieldType) == TypeCode.Object)
                {
                    throw new Exception("This is not a table field!");
                }
            }
            var attr = memberInfo.GetCustomAttribute(typeof(ColumnAttribute));
            if (attr != null)
            {
                return ((ColumnAttribute)attr).Name;
            }
            return memberInfo.Name;
        }
        /// <summary>
        /// Add Request Filter
        /// </summary>
        /// <param name="fieldNames">name of fields want to get the reference object, split by ","</param>
        public RequestData<T> GetRefs<P>(Expression<Func<T,P>> expression)
        {
            string propertyName = GetMemberName(expression);
            GetRefs(propertyName);
            return this;
        }
        /// <summary>
        /// Set page and pageSize for select request
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="pageSize">page size</param>
        public new RequestData<T> Take(int page, int pageSize)
        {
            base.Take(page, pageSize);
            return this;
        }
        public RequestData Sort<P>(Expression<Func<T, P>> expression, SortType sortType=SortType.Asc)
        {
            string propertyName = GetMemberName(expression);
            Sort(propertyName, sortType);
            return this;
        }

    }
}
