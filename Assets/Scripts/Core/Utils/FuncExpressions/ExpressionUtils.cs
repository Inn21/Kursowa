#region

using System;
using System.Linq.Expressions;

#endregion

namespace Engine.Core.Utils.FuncExpressions
{
    public static class ExpressionUtils
    {
        public static MemberExpression GetMemberExpression<T>(Expression<Func<T>> exp)
        {
            var body = exp.Body as MemberExpression;
            try
            {
                if (body == null)
                {
                    var ue = (UnaryExpression)exp.Body;
                    body = ue.Operand as MemberExpression;
                }
            }
            catch
            {
                // Debug.LogException(e);
            }

            return body;
        }

        public static string GetBeautifiedExpression<T>(Expression<Func<T>> exp)
        {
            var expression = exp.ToString();
            // Beautify expression for display...
            expression = expression.Substring(expression.LastIndexOf('.'));
            if (expression.EndsWith(")")) expression = expression.Substring(0, expression.Length - 1);

            return expression;
        }
    }
}