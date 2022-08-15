/*
 * ExpressionVisitor.cs
 * Based on: http://msdn.microsoft.com/en-us/library/bb882521%28v=VS.90%29.aspx
 */

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace LuaRuntime.Executors.ExpressionTrees
{

    public abstract class ExpressionVisitor
    {

        protected ExpressionVisitor()
        {
        }

        protected virtual void Visit(Expression exp)
        {
            if (exp == null)
            {
                return;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    VisitUnary((UnaryExpression)exp);
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    VisitBinary((BinaryExpression)exp);
                    break;
                case ExpressionType.TypeIs:
                    VisitTypeIs((TypeBinaryExpression)exp);
                    break;
                case ExpressionType.Conditional:
                    VisitConditional((ConditionalExpression)exp);
                    break;
                case ExpressionType.Constant:
                    VisitConstant((ConstantExpression)exp);
                    break;
                case ExpressionType.Parameter:
                    VisitParameter((ParameterExpression)exp);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)exp);
                    break;
                case ExpressionType.Call:
                    VisitMethodCall((MethodCallExpression)exp);
                    break;
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)exp);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)exp);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    VisitNewArray((NewArrayExpression)exp);
                    break;
                case ExpressionType.Invoke:
                    VisitInvocation((InvocationExpression)exp);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)exp);
                    break;
                case ExpressionType.ListInit:
                    VisitListInit((ListInitExpression)exp);
                    break;
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual void VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    VisitMemberAssignment((MemberAssignment)binding);
                    break;
                case MemberBindingType.MemberBinding:
                    VisitMemberMemberBinding((MemberMemberBinding)binding);
                    break;
                case MemberBindingType.ListBinding:
                    VisitMemberListBinding((MemberListBinding)binding);
                    break;
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
            throw new NotImplementedException();
        }

        protected virtual void VisitElementInitializer(ElementInit initializer)
        {
            VisitExpressionList(initializer.Arguments);
            throw new NotImplementedException();
        }

        protected virtual void VisitUnary(UnaryExpression u)
        {
            Visit(u.Operand);
            throw new NotImplementedException();
        }

        protected virtual void VisitBinary(BinaryExpression b)
        {
            Visit(b.Left);
            Visit(b.Right);
            Visit(b.Conversion);
            throw new NotImplementedException();
        }

        protected virtual void VisitTypeIs(TypeBinaryExpression b)
        {
            Visit(b.Expression);
            throw new NotImplementedException();
        }

        protected virtual void VisitConstant(ConstantExpression c)
        {
            throw new NotImplementedException();
        }

        protected virtual void VisitConditional(ConditionalExpression c)
        {
            Visit(c.Test);

            Visit(c.IfTrue);
            Visit(c.IfFalse);
            throw new NotImplementedException();
        }

        protected virtual void VisitParameter(ParameterExpression p)
        {
            throw new NotImplementedException();
        }

        protected virtual void VisitMemberAccess(MemberExpression m)
        {
            Visit(m.Expression);
            throw new NotImplementedException();
        }

        protected virtual void VisitMethodCall(MethodCallExpression m)
        {
            Visit(m.Object);
            VisitExpressionList(m.Arguments);
            throw new NotImplementedException();
        }

        protected virtual void VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Visit(original[i]);
            }

            throw new NotImplementedException();
        }

        protected virtual void VisitMemberAssignment(MemberAssignment assignment)
        {
            Visit(assignment.Expression);
            throw new NotImplementedException();
        }

        protected virtual void VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            VisitBindingList(binding.Bindings);
            throw new NotImplementedException();
        }

        protected virtual void VisitMemberListBinding(MemberListBinding binding)
        {
            VisitElementInitializerList(binding.Initializers);
            throw new NotImplementedException();
        }

        protected virtual void VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitBinding(original[i]);
            }

            throw new NotImplementedException();
        }

        protected virtual void VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            for (int i = 0, n = original.Count; i < n; i++)
            {
                VisitElementInitializer(original[i]);
            }

            throw new NotImplementedException();
        }

        protected virtual void VisitLambda(LambdaExpression lambda)
        {
            Visit(lambda.Body);
            throw new NotImplementedException();
        }

        protected virtual void VisitNew(NewExpression nex)
        {
            VisitExpressionList(nex.Arguments);
            throw new NotImplementedException();
        }

        protected virtual void VisitMemberInit(MemberInitExpression init)
        {
            VisitNew(init.NewExpression);
            VisitBindingList(init.Bindings);
            throw new NotImplementedException();
        }

        protected virtual void VisitListInit(ListInitExpression init)
        {
            VisitNew(init.NewExpression);
            VisitElementInitializerList(init.Initializers);
            throw new NotImplementedException();
        }

        protected virtual void VisitNewArray(NewArrayExpression na)
        {
            VisitExpressionList(na.Expressions);
            throw new NotImplementedException();
        }

        protected virtual void VisitInvocation(InvocationExpression iv)
        {
            VisitExpressionList(iv.Arguments);
            Visit(iv.Expression);
            throw new NotImplementedException();
        }
    }
}
