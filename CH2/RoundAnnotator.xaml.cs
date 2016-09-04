using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CH2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RoundAnnotator : Window
    {
        CHDB chdb;

        public RoundAnnotator()
        {
            InitializeComponent();
        }

        internal RoundAnnotator(CHDB chdb) : this()
        {
            this.chdb = chdb;
            this.DataContext = chdb;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Hide();
        }
    }
}
