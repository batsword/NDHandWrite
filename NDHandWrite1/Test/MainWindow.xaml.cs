using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    using MyTablet;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(this.MainWindow_Loaded);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InkTablet its = UIHelper.FindChild<InkTablet>(this, "its");
            its.SetCursor();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var s = this.its.NameStr;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string ss = "/ab/csd/dfas/fsa/";

            ss = ss.Trim('/');
            MessageBox.Show(ss);
        }
    }
}
