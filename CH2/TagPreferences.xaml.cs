using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CH2
{
    /// <summary>
    /// Interaction logic for TagPreferences.xaml
    /// </summary>
    public partial class TagPreferences : Window
    {
        private CHDB chdb;

        public TagPreferences()
        {
            InitializeComponent();
        }

        internal TagPreferences(CHDB chdb) : this()
        {
            this.chdb = chdb;
            DataContext = chdb;
        }


        private void toWhitelistButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("toWhitelistButton_Click");
            var itemsToMove = dontCareTagsLV.SelectedItems;
            if (itemsToMove.Count > 0)
            {
                chdb.moveElements("None", "whitelist", itemsToMove);
            }
        }

        private void fromWhitelistButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fromWhitelistButton_Click");
            var itemsToMove = whitelistLV.SelectedItems;
            if (itemsToMove.Count > 0)
            {
                chdb.moveElements("whitelist", "None", itemsToMove);
            }
        }

        private void toBlacklistButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("toBlacklistButton_Click");
            var itemsToMove = dontCareTagsLV.SelectedItems;
            if (itemsToMove.Count > 0)
            {
                chdb.moveElements("None", "blacklist", itemsToMove);
            }
        }

        private void fromBlacklistButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fromBlacklistButton_Click");
            var itemsToMove = blacklistLV.SelectedItems;
            if (itemsToMove.Count > 0)
            {
                chdb.moveElements("blacklist", "None", itemsToMove);
            }
        }
    }
}
