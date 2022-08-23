using Microsoft.Win32;
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

namespace GPR5100_LevelEditor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private string tileSheetPath;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnClick_SelectTilesheet(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Owner).SelectTilesheet();
        }

        public void SelectTilesheet()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.DefaultExt = ".png";
            ofd.Filter = "PNG Files|*.png;*.jpg";

            if (ofd.ShowDialog() == true)
            {
                tileSheetPath = ofd.FileName;
                Txt_TilesheetPath.Text = tileSheetPath;
            }
        }
    }
}
