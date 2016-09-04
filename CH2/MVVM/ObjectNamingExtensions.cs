using System;
using System.Linq.Expressions;

namespace CH2.MVVM
{
    /// <summary>
    /// Extensions for getting names of code elements
    /// </summary>
    public static class ObjectNamingExtensions
    {
        /// <summary>
        /// Gets the name of the member referenced by the given expression
        /// </summary>
        /// <typeparam name="T">Type of the return value of the expression</typeparam>
        /// <param name="e">Expression, e.g. () => thisName will return thisName</param>
        public static string GetName<T>(this Expression<Func<T>> e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(GetName(() => e));
                //throw new ArgumentNullException(nameof(e)); //C#6
            }

            var member = e.Body as MemberExpression;

            // If the method gets a lambda expression 
            // that is not a member access,
            // for example, () => x + y, an exception is thrown.
            if (member != null)
                return member.Member.Name;
            else
                throw new ArgumentException("'" + e +
                    "': is not a valid expression for this method");
        }

    }
}
