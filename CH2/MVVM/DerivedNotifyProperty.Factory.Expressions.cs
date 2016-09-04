using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace CH2.MVVM
{
    /// <summary>
    /// Contains additional methods for creating <see cref="DerivedNotifyProperty&lt;TValue&gt;"/>
    /// </summary>
    partial class DerivedNotifyProperty
    {
        /// <summary>
        /// Creates a derived notify property that changes when the depended-upon property changes
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DerivedNotifyProperty<TValue> CreateDerivedNotifyProperty<TDependedProperty1, TValue>(
            this IRaisePropertyChanged owner, Expression<Func<TValue>> derivedProperty,
            IProperty<TDependedProperty1> dependedProperty, Func<TDependedProperty1, TValue> getDerivedValueFunction)
        {
            return CreateDerivedNotifyProperty(owner, derivedProperty.GetName(), dependedProperty, getDerivedValueFunction);
        }

        /// <summary>
        /// Creates a derived notify property that changes when any depended-upon property changes
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DerivedNotifyProperty<TValue> CreateDerivedNotifyProperty<TDependedProperty1, TDependedProperty2, TValue>(
            this IRaisePropertyChanged owner, Expression<Func<TValue>> derivedProperty,
            IProperty<TDependedProperty1> dependedProperty1, IProperty<TDependedProperty2> dependedProperty2,
            Func<TDependedProperty1, TDependedProperty2, TValue> getDerivedValueFunction)
        {
            return CreateDerivedNotifyProperty(owner, derivedProperty.GetName(), dependedProperty1, dependedProperty2, getDerivedValueFunction);
        }

        /// <summary>
        /// Creates a derived notify property that changes when any depended-upon property changes
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DerivedNotifyProperty<TValue> CreateDerivedNotifyProperty<TDependedProperty1, TDependedProperty2, TDependedProperty3, TValue>(
            this IRaisePropertyChanged owner, Expression<Func<TValue>> derivedProperty,
            IProperty<TDependedProperty1> dependedProperty1, IProperty<TDependedProperty2> dependedProperty2, IProperty<TDependedProperty3> dependedProperty3,
            Func<TDependedProperty1, TDependedProperty2, TDependedProperty3, TValue> getDerivedValueFunction)
        {
            return CreateDerivedNotifyProperty(owner, derivedProperty.GetName(), dependedProperty1, dependedProperty2, dependedProperty3, getDerivedValueFunction);
        }

        /// <summary>
        /// Creates a derived notify property that changes when any depended-upon property changes
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DerivedNotifyProperty<TValue> CreateDerivedNotifyProperty<TDependedProperty1, TDependedProperty2, TDependedProperty3, TDependedProperty4, TValue>(
            this IRaisePropertyChanged owner, Expression<Func<TValue>> derivedProperty,
            IProperty<TDependedProperty1> dependedProperty1, IProperty<TDependedProperty2> dependedProperty2, IProperty<TDependedProperty3> dependedProperty3, IProperty<TDependedProperty4> dependedProperty4,
            Func<TDependedProperty1, TDependedProperty2, TDependedProperty3, TDependedProperty4, TValue> getDerivedValueFunction)
        {
            return CreateDerivedNotifyProperty(owner, derivedProperty.GetName(), dependedProperty1, dependedProperty2, dependedProperty3, dependedProperty4, getDerivedValueFunction);
        }


    }

}
