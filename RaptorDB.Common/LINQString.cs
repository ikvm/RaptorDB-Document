﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RaptorDB
{
	public class LINQString : ExpressionVisitor
	{
		public LINQString()
		{
		}
		public StringBuilder sb = new StringBuilder();

		protected override Expression VisitBinary(BinaryExpression b)
		{
			sb.Append("(");
			this.Visit(b.Left);
			ExpressionType t = b.NodeType;

			switch (b.NodeType)
			{
					//case ExpressionType.Not:
					//    sb.Append(" NOT ");
					//    break;
				case ExpressionType.AndAlso:
				case ExpressionType.And:
					sb.Append(" AND ");
					break;
				case ExpressionType.OrElse:
				case ExpressionType.Or:
					sb.Append(" OR ");
					break;
				case ExpressionType.Equal:
					sb.Append(" = ");
					break;
					//case ExpressionType.NotEqual:
					//    sb.Append(" <> ");
					//    break;
				case ExpressionType.LessThan:
					sb.Append(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					sb.Append(" <= ");
					break;
				case ExpressionType.GreaterThan:
					sb.Append(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					sb.Append(" >= ");
					break;
			}

			this.Visit(b.Right);
			sb.Append(")");
			return b;
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			string s = m.ToString();
			sb.Append(s.Substring(s.IndexOf('.') + 1));
			return m;
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			var e = base.VisitMember(m);
			var c = m.Expression as ConstantExpression;
			if (c != null)
			{
				Type t = c.Value.GetType();
				var x = t.InvokeMember(m.Member.Name, BindingFlags.GetField |
                    BindingFlags.GetProperty |
                    BindingFlags.Public | 
                    BindingFlags.NonPublic | 
                    BindingFlags.Instance |
                    BindingFlags.Static, null, c.Value, null);
                if (x is string)
                {
                    sb.Append("\"");
                    sb.Append(x);
                    sb.Append("\"");
                }
                else
                    sb.Append(x);
			}
			if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
			{
				sb.Append(m.Member.Name);
				return e;
			}
			return e;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			IQueryable q = c.Value as IQueryable;
			if (q != null)
			{
				sb.Append(q.ElementType.Name);
			}
			else if (c.Value == null)
			{
				sb.Append("NULL");
			}
			else
			{
				//_stack.Push(c.Value);
				//if (Type.GetTypeCode(c.Value.GetType()) == TypeCode.Object)
				//    _stack.Pop();

				switch (Type.GetTypeCode(c.Value.GetType()))
				{
					case TypeCode.Boolean:
						sb.Append(((bool)c.Value) ? 1 : 0);
						break;
					case TypeCode.String:
						sb.Append("\"");
						sb.Append(c.Value);
						sb.Append("\"");
						break;
					case TypeCode.Object:
						break;
					default:
						sb.Append(c.Value);
						break;
				}
			}
			return c;
		}
	}
}
