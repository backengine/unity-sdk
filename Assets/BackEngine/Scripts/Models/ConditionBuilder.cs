using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.Models
{
    public  class ConditionBuilder
    {
        public ConditionBuilder()
        {
        }

        public ConditionField this[string fieldName]
        {
            get
            {
                return new ConditionField(fieldName);
            }
        }
    }
    public class ConditionField
    {
        private string fieldName;
       public ConditionField(string name)
        {
            fieldName = name;
        }
        public Condition Equals( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Equals);
            return condition;
        }
        public Condition Greater( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Greater);
            return condition;
        }
        public Condition GreaterEquals( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.GreaterEquals);
            return condition;
        }
        public Condition In<T>( List<T> value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.In);
            return condition;
        }

        public Condition LessThan( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Lessthan);
            return condition;
        }
        public Condition LessThanEquals( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.LessthanEquals);
            return condition;
        }
        public Condition NotEquals( object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.NotEquals);
            return condition;
        }
        public Condition NotIn<T>( List<T> value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.NotIn);
            return condition;
        }
    }
    public delegate ConditionBase QueryableFunc(ConditionBuilder builder);
}
