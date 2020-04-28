using System;
using System.Collections.Generic;

namespace BE.Models
{


    /// <summary>
    /// Used to define conditions when making a request
    /// </summary>
    public class Condition : ConditionBase
    {

        public Type[] acceptTypes = { typeof(string),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(List<string>),
            typeof(List<int>),
            typeof(List<float>),
            typeof(List<double>),
            typeof(BEExpression)
        };

        /// <summary>
        /// Condition constructor
        /// </summary>
        /// <param name="fieldName">Name of the conditional column</param>
        /// <param name="value">Value of the condition should be number, string or List<object></param>
        /// <param name="conditionType">Type of condition</param>
        public Condition(string fieldName, object value, ConditionType conditionType = ConditionType.Equals)
            : base(conditionType)
        {
            if (Array.Exists(acceptTypes, type => type == value.GetType()))
            {
                this.FieldName = fieldName;
                this.Value = value;
            }
            else
            {
                Exception exception = new Exception("Type of condition value wrongs. See acceptTypes");
                throw exception;
            }
        }
        public string FieldName { get; }

        public static OrCondition operator |(Condition condition1, Condition condition2)
        {
            OrCondition or = new OrCondition(condition1, condition2);
            return or;
        }
        public static AndCondition operator &(Condition condition1, Condition condition2)
        {
            AndCondition a = new AndCondition(condition1, condition2);
            return a;
        }
    }
    public class ConditionGroup : ConditionBase
    {
        public ConditionGroup(ConditionType type, params ConditionBase[] conditions) : base(type)
        {
            Value = new List<ConditionBase>(conditions);
        }
        public void Add(ConditionBase condition) { ((List<ConditionBase>)Value).Add(condition); }
        public void AddRange(ConditionBase[] conditions) { ((List<ConditionBase>)Value).AddRange(conditions); }
        public List<ConditionBase> GetConditions()
        {
            return (List<ConditionBase>)Value;
        }
    }
    public class AndCondition : ConditionGroup
    {
        public AndCondition(params ConditionBase[] conditions) : base(ConditionType.And, conditions)
        {
        }
    }
    public class OrCondition : ConditionGroup
    {
        public OrCondition(params ConditionBase[] conditions) : base(ConditionType.Or, conditions)
        {
        }

    }
    public class ConditionBase
    {
        public object Value { get; protected set; }
        public ConditionBase(ConditionType type)
        {
            ConditionType = type;
        }
        public ConditionType ConditionType { get; private set; }
        public static ConditionBase operator |(ConditionBase condition1, ConditionBase condition2)
        {
            if (condition1.ConditionType == ConditionType.Or)
            {
                List<ConditionBase> list;
                if (condition2.ConditionType == ConditionType.Or)
                {
                    list = new List<ConditionBase>(((OrCondition)condition1).GetConditions());
                    list.AddRange(((OrCondition)condition1).GetConditions());
                    return new OrCondition(list.ToArray());
                }
                list = new List<ConditionBase>(((OrCondition)condition1).GetConditions());
                list.Add(condition2);
                return new OrCondition(list.ToArray());
            }
            else if (condition2.ConditionType == ConditionType.Or)
            {

                var list = new List<ConditionBase>();
                list.Add(condition1);
                list.AddRange(((OrCondition)condition2).GetConditions());
                return new OrCondition(list.ToArray());
            }
            return new OrCondition(condition1, condition2);
        }
        public static AndCondition operator &(ConditionBase condition1, ConditionBase condition2)
        {
            if (condition1.ConditionType == ConditionType.And)
            {
                List<ConditionBase> list;
                if (condition2.ConditionType == ConditionType.And)
                {
                    list = new List<ConditionBase>(((AndCondition)condition1).GetConditions());
                    list.AddRange(((AndCondition)condition1).GetConditions());
                    return new AndCondition(list.ToArray());
                }
                list = new List<ConditionBase>(((AndCondition)condition1).GetConditions());
                list.Add(condition2);
                return new AndCondition(list.ToArray());
            }
            else if (condition2.ConditionType == ConditionType.Or)
            {

                var list = new List<ConditionBase>();
                list.Add(condition1);
                list.AddRange(((AndCondition)condition2).GetConditions());
                return new AndCondition(list.ToArray());
            }
            return new AndCondition(condition1, condition2);
        }
    }

    public class BEExpression
    {
        public BEExpression(BEExpressionValue left, BEExpressionValue right, Operator op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
        public BEExpressionValue Left { get; set; }
        public BEExpressionValue Right { get; set; }
        public Operator Operator { get; set; }
    }
    public class BEExpressionValue
    {
        public BEExpressionValueType Type { get; set; }
        public object Value { get; set; }
    }
    public enum BEExpressionValueType
    {
        Expression,Field,Constant
    }
    public enum Operator
    {
        Add, Subtract, Multiply,Divide,Modulo,Power
    }
}
