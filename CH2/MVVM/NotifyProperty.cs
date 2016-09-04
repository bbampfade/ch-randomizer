using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CH2.MVVM
{
    /// <summary>
    /// Class encapsulating a property that raises <see cref="INotifyPropertyChanged.PropertyChanged"/> events on its parent when changed
    /// </summary>
    /// <typeparam name="T">Type of the property's value</typeparam>
    [DebuggerDisplay("{Value}")]
    public class NotifyProperty<T> : IProperty<T>
    {
        private readonly string m_name;
        private readonly IRaisePropertyChanged m_owner;
        private readonly IEqualityComparer<T> m_itemChangedComparer;

        /// <summary>
        /// Creates a notify property with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains this property, on which <see cref="INotifyPropertyChanged.PropertyChanged"/> events will be raised</param>
        /// <param name="name">Name of the property within its owner</param>
        public NotifyProperty(IRaisePropertyChanged owner, string name)
        {
            if (owner == null)
                throw new NotSupportedException("Null owners (e.g. static properties) are not supported");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("No property name was specified", ObjectNamingExtensions.GetName(() => name));

            this.m_owner = owner;
            this.m_name = name;
            this.m_itemChangedComparer = EqualityComparer<T>.Default; //will be overwritten by other constructors if necessary
        }

        /// <summary>
        /// Creates a notify property with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains this property, on which <see cref="INotifyPropertyChanged.PropertyChanged"/> events will be raised</param>
        /// <param name="name">Name of the property within its owner</param>
        /// <param name="itemChangedComparer">Comparer that will be used to determine if the value has changed</param>
        public NotifyProperty(IRaisePropertyChanged owner, string name, IEqualityComparer<T> itemChangedComparer)
            : this(owner, name, default(T), itemChangedComparer)
        { }

        /// <summary>
        /// Creates a notify property with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains this property, on which <see cref="INotifyPropertyChanged.PropertyChanged"/> events will be raised</param>
        /// <param name="name">Name of the property within its owner</param>
        /// <param name="initialValue">Initial value of the property</param>
        public NotifyProperty(IRaisePropertyChanged owner, string name, T initialValue)
            : this(owner, name, initialValue, EqualityComparer<T>.Default)
        { }

        /// <summary>
        /// Creates a notify property with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains this property, on which <see cref="INotifyPropertyChanged.PropertyChanged"/> events will be raised</param>
        /// <param name="name">Name of the property within its owner</param>
        /// <param name="initialValue">Initial value of the property</param>
        /// <param name="itemChangedComparer">Comparer that will be used to determine if the value has changed</param>
        public NotifyProperty(IRaisePropertyChanged owner, string name, T initialValue, IEqualityComparer<T> itemChangedComparer)
            : this(owner, name)
        {
            if (itemChangedComparer == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => itemChangedComparer));
            this.m_itemChangedComparer = itemChangedComparer;

            this.m_value = initialValue;
        }

        /// <summary>
        /// Gets the name of this property
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        private T m_value;
        /// <summary>
        /// Gets this property's current value
        /// </summary>
        public T Value
        {
            get
            {
                return m_value;
            }
        }

        /// <summary>
        /// Event fired when the property value has changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged.Raise(this, e);
        }

        /// <summary>
        /// Sets a new value for this property.  If it is different from the current property, the appropriate change events will be raised.
        /// </summary>
        /// <param name="newValue">New value to be set</param>
        /// <returns>True if the value was changed</returns>
        public bool SetValue(T newValue)
        {
            return SetValue(newValue, false);
        }

        /// <summary>
        /// Sets a new value for this property; if the new value differs from the old value, a PropertyChanged event will be fired from its owner
        /// </summary>
        /// <param name="newValue">New value to be set</param>
        /// <param name="overrideEqualityCheck">Whether or not to override the equality check.  If true, will raise the change events even if the value is unchanged</param>
        /// <returns>True if the value was changed</returns>
        public virtual bool SetValue(T newValue, bool overrideEqualityCheck)
        {
            if (!overrideEqualityCheck && m_itemChangedComparer.Equals(m_value, newValue))
                return false;

            m_value = newValue;

            OnValueChanged(s_emptyValueChangedArgs);
            m_owner.RaisePropertyChanged(Name);

            return true;
        }

        private static readonly ValueChangedEventArgs s_emptyValueChangedArgs = new ValueChangedEventArgs();
    }

}
