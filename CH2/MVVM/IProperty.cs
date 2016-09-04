namespace CH2.MVVM
{
    public interface IProperty<TValue> : INotifyValueChanged
    {
        string Name { get; }
        TValue Value { get; }
    }
}
