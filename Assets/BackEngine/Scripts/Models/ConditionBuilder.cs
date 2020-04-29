using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BE.Models
{
    public class ConditionBuilder
    {
        public ConditionField this[string key]
        {
            get
            {
                return new ConditionField(key);
            }
        }
    }
    public class ConditionExpressionBulder<T> : ExpressionVisitor
    {
        LambdaExpression predicate;
        Type type;
        public ConditionExpressionBulder(LambdaExpression predicate)
        {
            type = typeof(T);
            this.predicate = predicate;
        }
        Stack<ConditionBase> currentNodes = new Stack<ConditionBase>();
        Stack<ExpressionType> parentXExpressionType = new Stack<ExpressionType>();
        ConditionBase condition;
        Stack<BEExpressionValue> expressionPairs = new Stack<BEExpressionValue>();
        public ConditionBase ToCondition()
        {
            Visit(predicate.Body);
            return condition;
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Not)
            {
                //return;
            }
            Visit(node.Operand);
            return node;
        }
        private void MakeCallExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Call)
            {
                if (expressionPairs.Count > 0)
                {
                    var v1 = expressionPairs.Pop();
                    var v2 = expressionPairs.Pop();
                    var etype1 = v1.GetType();
                    var etype2 = v2.GetType();
                    if (etype1.IsGenericType && etype1.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        string fieldName = v2.ToString();
                        object value = v1;
                        Condition c = new Condition(fieldName, value, parentXExpressionType.Count > 0 && parentXExpressionType.Peek() != ExpressionType.Not ? ConditionType.In : ConditionType.NotIn);
                        if (condition == null)
                        {
                            condition = c;
                        }
                        if (currentNodes.Count > 0)
                        {
                            ((ConditionGroup)currentNodes.Peek()).Add(c);
                        }
                        return;
                    }
                    if (etype2.IsGenericType && etype2.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        string fieldName = v1.ToString();
                        object value = v2;
                        Condition c = new Condition(fieldName, value, parentXExpressionType.Count > 0 && parentXExpressionType.Peek() != ExpressionType.Not ? ConditionType.In : ConditionType.NotIn);
                        if (condition == null)
                        {
                            condition = c;
                        }
                        if (currentNodes.Count > 0)
                        {
                            ((ConditionGroup)currentNodes.Peek()).Add(c);
                        }
                        return;
                    }
                }
            }
        }
        private ConditionBase CreateCondition(ConditionType type)
        {
            var expression2 = expressionPairs.Pop();
            var expression1 = expressionPairs.Pop();
            if (expression1.Type == ExpressionValueType.Field)
            {
                return new Condition(expression1.Value.ToString(), expression2.Value, type);
            }
            if (expression2.Type == ExpressionValueType.Field)
            {
                if (type == ConditionType.Greater)
                {
                    type = ConditionType.Lessthan;
                }
                else if (type == ConditionType.Lessthan)
                {
                    type = ConditionType.Greater;
                }
                else if (type == ConditionType.GreaterEquals)
                {
                    type = ConditionType.LessthanEquals;
                }
                else if (type == ConditionType.LessthanEquals)
                {
                    type = ConditionType.GreaterEquals;
                }
                return new Condition(expression2.Value.ToString(), expression1.Value, type);
            }
            Operator op=Operator.Equals;
            switch (type)
            {
                case ConditionType.Equals:
                    op = Operator.Equals;
                    break;
                case ConditionType.Greater:
                    op = Operator.Greater;
                    break;
                case ConditionType.GreaterEquals:
                    op = Operator.GreaterEquals;
                    break;
                case ConditionType.Lessthan:
                    op = Operator.Lessthan;
                    break;
                case ConditionType.LessthanEquals:
                    op = Operator.Lessthan;
                    break;
                case ConditionType.NotEquals:
                    op = Operator.NotEquals;
                    break;
            }
            return new ExpressionCondition(new BEExpression(expression1, expression2, op));
        }
        private BEExpression CreateExpression(Operator op)
        {
            var expression2 = expressionPairs.Pop();
            var expression1 = expressionPairs.Pop();
            return new BEExpression(expression1, expression2, op);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            parentXExpressionType.Push(node.NodeType);
            if (node.NodeType == ExpressionType.And || node.NodeType == ExpressionType.AndAlso)
            {
                AndCondition and = new AndCondition();
                if (condition == null)
                {
                    condition = and;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(and);
                }
                currentNodes.Push(and);
                Visit(node.Left);
                MakeCallExpression(node.Left);
                Visit(node.Right);
                MakeCallExpression(node.Right);
                currentNodes.Pop();
            }
            else if (node.NodeType == ExpressionType.Or || node.NodeType == ExpressionType.OrElse)
            {
                OrCondition or = new OrCondition();
                if (condition == null)
                {
                    condition = or;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(or);
                }
                currentNodes.Push(or);
                Visit(node.Left);
                MakeCallExpression(node.Left);
                Visit(node.Right);
                MakeCallExpression(node.Right);
                currentNodes.Pop();
            }
            else if (node.NodeType == ExpressionType.LessThan)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(ConditionType.Lessthan);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.LessThanOrEqual)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(ConditionType.LessthanEquals);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.GreaterThan)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(ConditionType.Greater);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(ConditionType.GreaterEquals);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.Equal)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(parentXExpressionType.Count > 0 && parentXExpressionType.Peek() != ExpressionType.Not ? ConditionType.Equals : ConditionType.NotEquals);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.NotEqual)
            {
                Visit(node.Left);
                Visit(node.Right);
                var c = CreateCondition(parentXExpressionType.Count > 0 && parentXExpressionType.Peek() != ExpressionType.Not ? ConditionType.NotEquals : ConditionType.Equals);
                if (condition == null)
                {
                    condition = c;
                }
                if (currentNodes.Count > 0)
                {
                    ((ConditionGroup)currentNodes.Peek()).Add(c);
                }
            }
            else if (node.NodeType == ExpressionType.Add)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Add);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            else if (node.NodeType == ExpressionType.Subtract)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Subtract);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            else if (node.NodeType == ExpressionType.Multiply)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Multiply);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            else if (node.NodeType == ExpressionType.Divide)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Divide);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            else if (node.NodeType == ExpressionType.Modulo)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Modulo);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            else if (node.NodeType == ExpressionType.Power)
            {
                Visit(node.Left);
                Visit(node.Right);
                BEExpression expression = CreateExpression(Operator.Power);
                expressionPairs.Push(new BEExpressionValue() { Type = ExpressionValueType.Expression, Value = expression });
            }
            parentXExpressionType.Pop();
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {

            expressionPairs.Push(new BEExpressionValue() { Value = node.Value, Type = ExpressionValueType.Constant });
            return node;
        }



        protected override Expression VisitMember(MemberExpression node)
        {

            if (node.Expression.NodeType == ExpressionType.Constant || node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                if (node.Expression.Type.Equals(type))
                {
                    expressionPairs.Push(new BEExpressionValue() { Value = node.Member.Name, Type = ExpressionValueType.Field });
                }
                else
                {
                    //Visit(node.Expression);
                    var objectMember = Expression.Convert(node, typeof(object));

                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                    var getter = getterLambda.Compile();

                    expressionPairs.Push(new BEExpressionValue { Value = getter(), Type = ExpressionValueType.Constant });
                }
            }
            else if (node.Expression.Type.Equals(type))
            {
                expressionPairs.Push(new BEExpressionValue() { Value = node.Member.Name, Type = ExpressionValueType.Field });
            }

            return node;
        }

    }
    public class ConditionField
    {
        private string fieldName;
        public ConditionField(string name)
        {
            fieldName = name;
        }
        public Condition Equals(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Equals);
            return condition;
        }
        public Condition Greater(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Greater);
            return condition;
        }
        public Condition GreaterEquals(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.GreaterEquals);
            return condition;
        }
        public Condition In<T>(List<T> value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.In);
            return condition;
        }

        public Condition LessThan(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.Lessthan);
            return condition;
        }
        public Condition LessThanEquals(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.LessthanEquals);
            return condition;
        }
        public Condition NotEquals(object value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.NotEquals);
            return condition;
        }
        public Condition NotIn<T>(List<T> value)
        {
            Condition condition = new Condition(fieldName, value, ConditionType.NotIn);
            return condition;
        }
    }
    public delegate ConditionBase QueryableFunc(ConditionBuilder predicate);
}
