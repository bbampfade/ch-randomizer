using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace CH2.MVVM
{
    /// <summary>
    /// Contains convenience methods for creating <see cref="NotifyProperty"/> objects
    /// </summary>
    public static class NotifyProperty
    {
        /// <summary>
        /// Creates a new notify property with the given owner and name
        /// </summary>
        /// <typeparam name="T">Type of the property's value</typeparam>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="nameExpression">Expression used to determine the name of the property</param>
        /// <returns>A <see cref="NotifyProperty"/> object encapsulating the owner's property</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static NotifyProperty<T> CreateNotifyProperty<T>(this IRaisePropertyChanged owner, Expression<Func<T>> nameExpression)
        {
            return new NotifyProperty<T>(owner, nameExpression.GetName());
        }

        /// <summary>
        /// Creates a new notify property with the given owner, name, and equality comparer
        /// </summary>
        /// <typeparam name="T">Type of the property's value</typeparam>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="nameExpression">Expression used to determine the name of the property</param>
        /// <param name="itemChangedComparer">Comparer used to determine if the property value has changed</param>
        /// <returns>A <see cref="NotifyProperty"/> object encapsulating the owner's property</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static NotifyProperty<T> CreateNotifyProperty<T>(this IRaisePropertyChanged owner, Expression<Func<T>> nameExpression, IEqualityComparer<T> itemChangedComparer)
        {
            return new NotifyProperty<T>(owner, nameExpression.GetName(), itemChangedComparer);
        }

        /// <summary>
        /// Creates a new notify property with the given owner and name
        /// </summary>
        /// <typeparam name="T">Type of the property's value</typeparam>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="nameExpression">Expression used to determine the name of the property</param>
        /// <param name="initialValue">Initial value of the property</param>
        /// <returns>A <see cref="NotifyProperty"/> object encapsulating the owner's property</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static NotifyProperty<T> CreateNotifyProperty<T>(this IRaisePropertyChanged owner, Expression<Func<T>> nameExpression, T initialValue)
        {
            return new NotifyProperty<T>(owner, nameExpression.GetName(), initialValue);
        }

        /// <summary>
        /// Creates a new notify property with the given owner and name
        /// </summary>
        /// <typeparam name="T">Type of the property's value</typeparam>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="nameExpression">Expression used to determine the name of the property</param>
        /// <param name="initialValue">Initial value of the property</param>
        /// <param name="itemChangedComparer">Comparer used to determine if the property value has changed</param>
        /// <returns>A <see cref="NotifyProperty"/> object encapsulating the owner's property</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static NotifyProperty<T> CreateNotifyProperty<T>(this IRaisePropertyChanged owner, Expression<Func<T>> nameExpression, T initialValue, IEqualityComparer<T> itemChangedComparer)
        {
            return new NotifyProperty<T>(owner, nameExpression.GetName(), initialValue, itemChangedComparer);
        }
    }

}
