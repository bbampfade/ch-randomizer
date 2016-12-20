using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace CH2.MVVM.Behaviors
{
    internal class CancellableSelectionBehavior : Behavior<Selector>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CancellableSelectionBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged ));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty IgnoreNullSelectionProperty =
            DependencyProperty.Register("IgnoreNullSelection", typeof(bool), typeof(CancellableSelectionBehavior), new PropertyMetadata(true));

        public bool IgnoreNullSelection
        {
            get { return (bool)GetValue(IgnoreNullSelectionProperty); }
            set { SetValue(IgnoreNullSelectionProperty, value); }
        }

        public static readonly DependencyProperty AutoScrollToSelectedItemProperty =
            DependencyProperty.Register("AutoScrollToSelectedItem", typeof(bool), typeof(CancellableSelectionBehavior), new PropertyMetadata(true));

        public bool AutoScrollToSelectedItem
        {
            get { return (bool)GetValue(AutoScrollToSelectedItemProperty); }
            set { SetValue(AutoScrollToSelectedItemProperty, value); }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (CancellableSelectionBehavior)d;

            // OnSelectedItemChanged can be raised before AssociatedObject is assigned
            if (behavior.AssociatedObject == null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    var selector = behavior.AssociatedObject;
                    if (selector == null)
                    {
                        return; // can still be null in the designer for some reason
                    }
                    var selectorType = behavior.AssociatedObject.GetType();
                    selector.SelectedValue = e.NewValue;
                    if (typeof(DataGrid).IsAssignableFrom(selectorType))
                    {
                        var control = selector as DataGrid;
                        control.ScrollIntoView(e.NewValue);
                    }
                }));
            }
            else
            {
                var selector = behavior.AssociatedObject;
                var selectorType = behavior.AssociatedObject.GetType();
                selector.SelectedValue = e.NewValue;
                if (typeof(DataGrid).IsAssignableFrom(selectorType))
                {
                    var control = selector as DataGrid;
                    control.ScrollIntoView(e.NewValue);
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreNullSelection && (e.AddedItems == null || e.AddedItems.Count == 0)) return;
            SelectedItem = AssociatedObject.SelectedItem;
            if (SelectedItem != AssociatedObject.SelectedItem)
            {
                AssociatedObject.SelectedItem = SelectedItem;
            }
        }
    }
}
