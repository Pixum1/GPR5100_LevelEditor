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
        public SettingsWindow()
        {
            InitializeComponent();
        }
        private void OnClick_SelectTilesheet(object sender, RoutedEventArgs e)
        {
            try
            {
                ((MainWindow)this.Owner).SelectTilesheet();
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
            }
        }

        private void OnClick_SliceTilesheet(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Btn_SyncDimensions.IsChecked == true)
                {
                    ((MainWindow)this.Owner).SlicedTileHeight = Int32.Parse(Txt_SliceTileHeight.Text);
                    ((MainWindow)this.Owner).SlicedTileWidth = Int32.Parse(Txt_SliceTileHeight.Text);
                }
                else
                {
                    ((MainWindow)this.Owner).SlicedTileHeight = Int32.Parse(Txt_SliceTileHeight.Text);
                    ((MainWindow)this.Owner).SlicedTileWidth = Int32.Parse(Txt_SliceTileHeight.Text);
                }

                ((MainWindow)this.Owner).SliceTilesheetFile();

                this.Close();
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
            }
        }

        private void Btn_SyncDimensions_Click(object sender, RoutedEventArgs e)
        {
            Txt_SliceTileHeight.IsEnabled = !Txt_SliceTileHeight.IsEnabled;

            if (Btn_SyncDimensions.IsChecked == true)
                Txt_SliceTileHeight.Text = Txt_SliceTileWidth.Text;
        }
        private void Txt_SliceTileWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Btn_SyncDimensions == null) return;

            if (Btn_SyncDimensions.IsChecked == true)
                Txt_SliceTileHeight.Text = Txt_SliceTileWidth.Text;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((MainWindow)this.Owner).settingsWindow = null;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
