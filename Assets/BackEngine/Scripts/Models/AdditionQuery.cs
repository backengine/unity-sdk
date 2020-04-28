using System;
using System.Collections.Generic;

namespace BE.Models
{
    public enum QueryType { Insert, Update, Delete }

    public class AdditionQuery 
    {
        public string Schema { get; }

        public QueryType QueryType { get; }

        public List<string> FieldsFromQuery { get; private set; }

        public Dictionary<string, object> AdditionFields { get; private set; }
        public ConditionBase Condition { get; set; }
        public AdditionQuery(string schema, QueryType queryType)
        {
            this.Schema = schema;
            this.QueryType = queryType;
        }
        public AdditionQuery Where(QueryableFunc callBack)
        {
            var value = (callBack(new ConditionBuilder()));
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
        public void AddFieldFromQuery(string fieldName)
        {
            if (FieldsFromQuery == null) FieldsFromQuery = new List<string>();
            FieldsFromQuery.Add(fieldName);
        }
        public void AddAdditionField(string fieldName, object value)
        {
            if (AdditionFields == null) AdditionFields = new Dictionary<string, object>();
            AdditionFields.Add(fieldName, value);
        }

    }

}
