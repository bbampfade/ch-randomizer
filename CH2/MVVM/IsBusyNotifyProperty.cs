using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace CH2.MVVM
{
    /// <summary>
    /// Property representing whether an object is currently busy.  Tracks the number of times it has been set to 'true' and 'false' and aggregates this into an effective 'true' or 'false' accordingly.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class IsBusyNotifyProperty : NotifyProperty<bool>
    {
        /// <summary>
        /// Creates a new <see cref="IsBusyNotifyProperty"/> with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="nameExpression">Expression used to determine the name of the property</param>
        /// <returns>A completed <see cref="IsBusyNotifyProperty"/> object</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IsBusyNotifyProperty Create(IRaisePropertyChanged owner, Expression<Func<bool>> nameExpression)
        {
            return new IsBusyNotifyProperty(owner, nameExpression.GetName());
        }


        /// <summary>
        /// Creates a new <see cref="IsBusyNotifyProperty"/> with the given owner and name
        /// </summary>
        /// <param name="owner">Object that contains the property</param>
        /// <param name="name">Name of this property within the parent object</param>
        public IsBusyNotifyProperty(IRaisePropertyChanged owner, string name)
            : base(owner, name)
        {

        }


        private int m_count = 0; //number of times the property was set to true

        /// <summary>
        /// Sets a new value for this property; if the new value differs from the old value, a PropertyChanged event will be fired from its owner
        /// </summary>
        /// <param name="newValue">New value to be set</param>
        /// <param name="overrideEqualityCheck">Whether or not to override the equality check.  If true, will raise the change events even if the value is unchanged</param>
        /// <returns>True if the value was changed</returns>
        public override bool SetValue(bool newValue, bool overrideEqualityCheck)
        {
            if (newValue)
            {
                m_count++;
                if (m_count == 1 || overrideEqualityCheck) //changed from false to true (or equality check skipped)
                {
                    base.SetValue(true, overrideEqualityCheck);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                m_count--;
                if (m_count == 0 || overrideEqualityCheck) //changed from true to false (or equality check skipped)
                {
                    base.SetValue(false, overrideEqualityCheck);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

}
