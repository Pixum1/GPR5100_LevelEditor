using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace GPR5100_LevelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SettingsWindow settingsWindow;

        private string tileSheetPath;

        private bool eraseModeActive = false;

        private int slicedTileWidth = 16;
        private int slicedTileHeight = 16;

        public int SlicedTileWidth { get { return slicedTileWidth; } set { slicedTileWidth = value; } }
        public int SlicedTileHeight { get { return slicedTileHeight; } set { slicedTileHeight = value; } }

        private BitmapImage tileSheetBitmap;

        private List<CroppedBitmap> tileList = new List<CroppedBitmap>();
        private ImageBrush currSelectedTile;

        private int currSelectedTileIndex;

        private int inspectorTileSize = 25;

        private int[] map;
        public MainWindow()
        {
            InitializeComponent();
            FillMapCanvas();
        }

        #region Functions
        private void FillMapCanvas()
        {
            WrapPanel_Map.Children.Clear();

            int numOfPossibleTiles = 0;

            //Calculate the maximum number of tiles that fit in the inspector
            numOfPossibleTiles = ((int)WrapPanel_Map.Height / inspectorTileSize) * ((int)WrapPanel_Map.Width / inspectorTileSize);

            map = new int[numOfPossibleTiles];

            Rectangle rect;

            for (int i = 0; i < numOfPossibleTiles; i++)
            {
                map[i] = -1;

                // Create a new Rectangle for each tile
                rect = new Rectangle();

                // Fill each tile with a black color and adjust its size
                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                rect.Width = inspectorTileSize;
                rect.Height = inspectorTileSize;

                WrapPanel_Map.Children.Add(rect);
            }
        }
        public void SelectTilesheet()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.DefaultExt = ".png";
                ofd.Filter = "PNG Files|*.png;*.jpg";

                if (ofd.ShowDialog() == true)
                {
                    tileSheetPath = ofd.FileName;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while selecting the tilesheet. Make sure to only use .png and .jpg files");
            }
        }
        public void SliceTilesheetFile()
        {
            SliceTilesheet();
            DisplayTileList();
        }
        private void SliceTilesheet()
        {
            if (!File.Exists(tileSheetPath)) return;

            tileList.Clear();

            tileSheetBitmap = new BitmapImage(new Uri(tileSheetPath, UriKind.Relative));

            int fixedBitMapHeight = (int)(tileSheetBitmap.PixelHeight - (tileSheetBitmap.PixelHeight % slicedTileHeight));
            int fixedBitMapWidth = (int)(tileSheetBitmap.PixelWidth - (tileSheetBitmap.PixelWidth % slicedTileWidth));

            for (int y = 0; y < fixedBitMapHeight - slicedTileHeight; y += slicedTileHeight)
            {
                for (int x = 0; x < fixedBitMapWidth - slicedTileWidth; x += slicedTileWidth)
                {
                    CroppedBitmap cb = new CroppedBitmap(tileSheetBitmap, new Int32Rect(x, y, slicedTileWidth, slicedTileHeight));
                    tileList.Add(cb);
                }
            }
        }
        private void DisplayTileList()
        {
            WrapPanel_Sprites.Children.Clear();

            for (int i = 0; i < tileList.Count; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Width = Sld_TileSize.Value;
                rect.Height = Sld_TileSize.Value;

                ImageBrush imgBrush = new ImageBrush();

                imgBrush.ImageSource = tileList[i];

                rect.Fill = imgBrush;

                WrapPanel_Sprites.Children.Add(rect);
            }

            currSelectedTile = new ImageBrush(tileList[0]);
            currSelectedTileIndex = 0;
            Rect_SelectedTile.Fill = currSelectedTile;
        }
        private void LoadXMLMap()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.Filter = "XML-Files|*.xml";

                if (ofd.ShowDialog() == true)
                {

                    ApplyXMLData(ofd.FileName);

                    FillMapWithData();
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show("There was an error while trying to load the data! Make sure to only use XML files!");
            }
        }
        private void FillMapWithData()
        {
            for (int i = 0; i < WrapPanel_Map.Children.Count; i++)
            {
                Rectangle rect = (Rectangle)WrapPanel_Map.Children[i];

                ImageBrush imgBrush = new ImageBrush();

                if (map[i] == -1)
                    rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                else
                {
                    imgBrush.ImageSource = tileList[map[i]];

                    rect.Fill = imgBrush;
                }
            }
        }
        private void ApplyXMLData(string _path)
        {
            using (FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MapSave));

                MapSave savedMap = (MapSave)serializer.Deserialize(fs);

                tileSheetPath = savedMap.TileSheetPath;
                slicedTileHeight = savedMap.Height;
                slicedTileWidth = savedMap.Width;
                map = savedMap.Map;

                fs.Close();
            }
            SliceTilesheet();
            DisplayTileList();
        }
        private void SaveDataToXML()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "XML-File|*.xml";

                if (sfd.ShowDialog() == true)
                {
                    MapSave mapSave = new MapSave(map, tileSheetPath, slicedTileHeight, slicedTileWidth);

                    XmlSerializer serializer = new XmlSerializer(typeof(MapSave));

                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                    {
                        serializer.Serialize(fs, mapSave);

                        fs.Close();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while trying to save your data!");
            }

        }
        private void OpenNewSettingsWindow()
        {
            if (settingsWindow != null) return;

            try
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while opening the settings window!");
            }
        }
        private void ResizeSpritesPanel()
        {
            if (WrapPanel_Sprites == null) return;

            for (int i = 0; i < WrapPanel_Sprites.Children.Count; i++)
            {
                Rectangle rect = (Rectangle)WrapPanel_Sprites.Children[i];

                rect.Width = Sld_TileSize.Value;
                rect.Height = Sld_TileSize.Value;
            }
        }
        private void DrawOnMap(Point point)
        {
            HitTestResult result = VisualTreeHelper.HitTest(WrapPanel_Map, point);

            if (result == null) return;

            try
            {
                Rectangle rect = (Rectangle)result.VisualHit;

                if (eraseModeActive)
                {
                    map[WrapPanel_Map.Children.IndexOf(rect)] = -1;

                    rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else
                {
                    map[WrapPanel_Map.Children.IndexOf(rect)] = currSelectedTileIndex;

                    rect.Fill = currSelectedTile;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while drawing on the map.");
            }
        }
        #endregion

        #region WPF Events
        private void OnClick_SelectTilesheet(object sender, RoutedEventArgs e)
        {
            SelectTilesheet();
        }
        private void OnClick_SliceTilesheet(object sender, RoutedEventArgs e)
        {
            SliceTilesheetFile();
        }
        private void OnClick_ClearMap(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to clear the map?", "Clear!", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            FillMapCanvas();
        }
        private void OnClick_SaveMap(object sender, RoutedEventArgs e)
        {
            SaveDataToXML();
        }
        private void OnClick_LoadMap(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to load without saving?", "Load!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                SaveDataToXML();
            else
                LoadXMLMap();
        }
        private void LMBDown_StackSprites(object sender, MouseButtonEventArgs e)
        {
            Point pointOnCanvas = e.GetPosition(WrapPanel_Sprites);

            SelectSprite(pointOnCanvas);
        }
        private void SelectSprite(Point pointOnCanvas)
        {
            HitTestResult result = VisualTreeHelper.HitTest(WrapPanel_Sprites, pointOnCanvas);

            if (result != null)
            {
                try
                {
                    Rectangle rect = (Rectangle)result.VisualHit;

                    ImageBrush brush = (ImageBrush)rect.Fill;

                    currSelectedTile = brush;
                    currSelectedTileIndex = WrapPanel_Sprites.Children.IndexOf(rect);

                    Rect_SelectedTile.Fill = currSelectedTile;
                }
                catch (Exception _e)
                {
                    MessageBox.Show(_e.Message);
                }
            }
        }
        private void LMBDown_Map(object sender, MouseButtonEventArgs e)
        {
            if (currSelectedTile == null) return;

            Point point = e.GetPosition((WrapPanel_Map));
            DrawOnMap(point);
        }
        private void OnClick_OpenSettings(object sender, RoutedEventArgs e)
        {
            OpenNewSettingsWindow();
        }
        private void Sld_TileSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ResizeSpritesPanel();
        }
        private void OnClick_Erase(object sender, RoutedEventArgs e)
        {
            eraseModeActive = !eraseModeActive;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Do you want to exit without saving?", "Exit!", MessageBoxButton.YesNo) == MessageBoxResult.Yes) return;

            SaveDataToXML();
        }
        private void LMBDown_MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void OnCLick_CloseApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
