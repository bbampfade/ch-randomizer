using CH2.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TokenizedTag;

namespace CH2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TagEditor : Window
    {
        private CHDB chdb;

        public TagEditor()
        {
            InitializeComponent();
        }

        internal TagEditor(CHDB chdb) : this()
        {
            this.chdb = chdb;
            this.DataContext = chdb;
            TagControl.AllTags = chdb.AllTags;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Hide();
        }

        private void TagControl_TagClick(object sender, TokenizedTagEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("TagClick: {0}", e.Item.Text));
        }

        private void TagControl_TagApplied(object sender, TokenizedTagEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("TagApplied: {0}", e.Item.Text));
            // forward to viewmodel so it can update DB
            chdb.TagApplied(sender, e);
        }

        private void TagControl_TagRemoved(object sender, TokenizedTagEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("TagRemoved: {0}", e.Item.Text));
            // forward to viewmodel so it can update DB
            chdb.TagRemoved(sender, e);
        }

        private void TagControl_TagAdded(object sender, TokenizedTagEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("TagAdded: {0}", e.Item.Text));
            if (e.Item.Text != null)
            {
                chdb.TagApplied(sender, e);
            }
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }

        private void SyncFromPlayerButton_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
