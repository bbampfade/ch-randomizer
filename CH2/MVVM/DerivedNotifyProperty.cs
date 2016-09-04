using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CH2.MVVM
{
    public class DerivedNotifyProperty<TValue> : IProperty<TValue>
    {
        private readonly IRaisePropertyChanged m_owner;
        private readonly string m_derivedPropertyName;
        private readonly Func<TValue> m_getValueProperty;
        private readonly Dictionary<string, bool> m_dependentValuesToListenFor;

        private DerivedNotifyProperty(IRaisePropertyChanged owner, string derivedPropertyName, Func<TValue> getDerivedPropertyValue)
        {
            if (owner == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => owner));
            this.m_owner = owner;

            if (string.IsNullOrEmpty(derivedPropertyName))
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => derivedPropertyName));
            this.m_derivedPropertyName = derivedPropertyName;

            if (getDerivedPropertyValue == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => getDerivedPropertyValue));
            this.m_getValueProperty = getDerivedPropertyValue;
            this.m_value = new Lazy<TValue>(m_getValueProperty);
        }

        /// <summary>
        /// Creates a new <see cref="DerivedNotifyProperty&lt;TValue&gt;"/>.  When any of the specified properties change in the owner object, this property's value will be re-calculated the next time it is requested
        /// </summary>
        /// <param name="owner">Object that contains this property and the ones from which it is derived</param>
        /// <param name="derivedPropertyName">Name of this property in the owner</param>
        /// <param name="getDerivedPropertyValue">Function to determine the current value of this property</param>
        /// <param name="propertiesDependedOn">Properties that this value depends on.  When any of these properties change, this property's value will be re-calculated the next time it is requested.  
        /// These properties must therefore be in the same owner object as this one.</param>
        public DerivedNotifyProperty(IRaisePropertyChanged owner, string derivedPropertyName, Func<TValue> getDerivedPropertyValue, params string[] propertiesDependedOn)
            : this(owner, derivedPropertyName, getDerivedPropertyValue)
        {
            this.m_owner.PropertyChanged += m_owner_PropertyChanged;

            if (propertiesDependedOn == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => propertiesDependedOn));
            this.m_dependentValuesToListenFor = propertiesDependedOn.ToDictionary(property => property, property => true);
        }

        /// <summary>
        /// Creates a new <see cref="DerivedNotifyProperty&lt;TValue&gt;"/>.  When any of the specified properties change, this property's value will be re-calculated the next time it is requested
        /// </summary>
        /// <param name="owner">Object that contains this property</param>
        /// <param name="derivedPropertyName">Name of this property in the owner</param>
        /// <param name="getDerivedPropertyValue">Function to determine the current value of this property</param>
        /// <param name="valueChangesToListenFor">Properties that this value depends on.  When any of these properties change, this property's value will be re-calculated the next time it is requested.</param>
        public DerivedNotifyProperty(IRaisePropertyChanged owner, string derivedPropertyName, Func<TValue> getDerivedPropertyValue, params INotifyValueChanged[] valueChangesToListenFor)
            : this(owner, derivedPropertyName, getDerivedPropertyValue)
        {
            if (valueChangesToListenFor == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => valueChangesToListenFor));

            foreach (INotifyValueChanged valueChangeToListenFor in valueChangesToListenFor)
            {
                valueChangeToListenFor.ValueChanged += TriggerValueChanged;
            }
        }

        /// <summary>
        /// Gets the name of this property
        /// </summary>
        public string Name
        {
            get
            {
                return m_derivedPropertyName;
            }
        }

        private Lazy<TValue> m_value;

        /// <summary>
        /// Gets the current value of this property
        /// </summary>
        public TValue Value
        {
            get
            {
                return m_value.Value;
            }
        }

        /// <summary>
        /// Event fired when the property value has changed
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Raises this object's <see cref="ValueChanged"/> event
        /// </summary>
        /// <param name="e"></param>
        protected void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged.Raise(this, e);
        }

        private void m_owner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_dependentValuesToListenFor.ContainsKey(e.PropertyName))
            {
                RefreshProperty();
            }
        }

        private void TriggerValueChanged(object sender, EventArgs e)
        {
            RefreshProperty();
        }

        private static readonly ValueChangedEventArgs s_emptyValueChangedEventArgs = new ValueChangedEventArgs();
        /// <summary>
        /// Triggers this property to appear changed and causes its its value to be recomputed the next time it's accessed
        /// </summary>
        public void RefreshProperty()
        {
            //ensure we retrieve the value anew the next time it is requested
            this.m_value = new Lazy<TValue>(m_getValueProperty);

            OnValueChanged(s_emptyValueChangedEventArgs);
            m_owner.RaisePropertyChanged(Name);
        }

    }
}
