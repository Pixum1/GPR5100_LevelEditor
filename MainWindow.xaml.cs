using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace GPR5100_LevelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // the name of the tilesheets path
        private string tileSheetPath = "";

        // flag for the erase mode 
        private bool eraseModeActive = false;

        // dimensions 
        private int slicedTileWidth = 16;
        private int slicedTileHeight = 16;

        // BitmapImage of the selected tilesheet
        private BitmapImage tileSheetBitmap = new BitmapImage();

        // List of all tiles within the tilesheet
        private List<CroppedBitmap> tileList = new List<CroppedBitmap>();

        // Data of the currently selected tile
        private ImageBrush currSelectedTile = new ImageBrush();
        private int currSelectedTileIndex;

        // Tile size of the map
        private int inspectorTileSize = 50;

        private int[] map = new int[0];

        public MainWindow()
        {
            InitializeComponent();
            FillMapCanvas();
        }

        #region Functions

        #region
        #endregion

        /// <summary>
        /// Toggles the visibility of a specified grid
        /// </summary>
        /// <param name="_grid"></param>
        private void ToggleUI(Grid _grid)
        {
            _grid.Visibility = (Visibility)(_grid.IsVisible ? 1 : 0);
        }
        /// <summary>
        /// Fills the map panel and map with blank (black|-1) tiles
        /// </summary>
        private void FillMapCanvas()
        {
            WrapPanel_Map.Children.Clear();

            //Calculate the maximum number of tiles that fit in the inspector
            int numOfPossibleTiles = ((int)WrapPanel_Map.Height / inspectorTileSize) * ((int)WrapPanel_Map.Width / inspectorTileSize);
            map = new int[numOfPossibleTiles];

            for (int i = 0; i < numOfPossibleTiles; i++)
            {
                map[i] = -1;

                // Create a new Rectangle for each tile
                Rectangle rect = new Rectangle();

                // Fill each tile with a black color and adjust its size
                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                rect.Width = inspectorTileSize;
                rect.Height = inspectorTileSize;

                // Add tile to the map
                WrapPanel_Map.Children.Add(rect);
            }
        }
        /// <summary>
        /// Opens a file dialog to load a tilesheet
        /// </summary>
        private void SelectTilesheet()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                // Add filters
                ofd.DefaultExt = ".png";
                ofd.Filter = "PNG Files|*.png;*.jpg";

                if (ofd.ShowDialog() == true)
                {
                    // save the tilesheets filename
                    tileSheetPath = ofd.FileName;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while selecting the tilesheet. Make sure to only use .png and .jpg files");
            }
        }
        /// <summary>
        /// Slices the tilesheet into tiles and displays those in the applications sprite panel
        /// </summary>
        private void SliceTilesheetFile()
        {
            SliceTilesheet();
            DisplayTileList();
        }
        /// <summary>
        /// Creates a new BitmapImage of the selected tilesheet and adds every tile to the tileList
        /// </summary>
        private void SliceTilesheet()
        {
            if (!File.Exists(tileSheetPath)) return;

            tileList.Clear();

            // Create a new BitmapImage of the selected tilesheet
            tileSheetBitmap = new BitmapImage(new Uri(tileSheetPath, UriKind.Relative));

            // Limit the height and width to prevent out of range errors
            int fixedBitMapHeight = (int)(tileSheetBitmap.PixelHeight - (tileSheetBitmap.PixelHeight % slicedTileHeight));
            int fixedBitMapWidth = (int)(tileSheetBitmap.PixelWidth - (tileSheetBitmap.PixelWidth % slicedTileWidth));

            for (int y = 0; y < fixedBitMapHeight - slicedTileHeight; y += slicedTileHeight)
            {
                for (int x = 0; x < fixedBitMapWidth - slicedTileWidth; x += slicedTileWidth)
                {
                    // Create a new CroppedBitmap with a specific location based on the height and width settings
                    CroppedBitmap cb = new CroppedBitmap(tileSheetBitmap, new Int32Rect(x, y, slicedTileWidth, slicedTileHeight));
                    tileList.Add(cb);
                }
            }
        }
        /// <summary>
        /// Displays all tiles in the applications sprite panel
        /// </summary>
        private void DisplayTileList()
        {
            WrapPanel_Sprites.Children.Clear();

            for (int i = 0; i < tileList.Count; i++)
            {
                // Create a new Rectangle with specified height and width
                Rectangle rect = new Rectangle();
                rect.Width = Sld_TileSize.Value;
                rect.Height = Sld_TileSize.Value;

                // Create a new ImageBrush for each tile
                ImageBrush imgBrush = new ImageBrush();
                imgBrush.ImageSource = tileList[i];

                // Paint the ImageBrush onto the Rectangle
                rect.Fill = imgBrush;

                WrapPanel_Sprites.Children.Add(rect);
            }

            // Make the currently selected tile the first in the list
            currSelectedTile = new ImageBrush(tileList[0]);
            currSelectedTileIndex = 0;
            Rect_SelectedTile.Fill = currSelectedTile;
        }
        /// <summary>
        /// Opens a XML file and applies the data to the application
        /// </summary>
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
            catch (Exception)
            {
                MessageBox.Show("There was an error while trying to load the data! Make sure to only use XML files!");
            }
        }
        /// <summary>
        /// When loading from an XML file the map panel is filled with the XML files content
        /// </summary>
        private void FillMapWithData()
        {
            for (int i = 0; i < WrapPanel_Map.Children.Count; i++)
            {
                // Get each Rectangle child from the map panel
                Rectangle rect = (Rectangle)WrapPanel_Map.Children[i];

                ImageBrush imgBrush = new ImageBrush();

                // If tile at position i is blank fill it with a black color
                if (map[i] == -1)
                {
                    rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else
                {
                    // Fill the Rectangle with the data from the XML file
                    imgBrush.ImageSource = tileList[map[i]];

                    rect.Fill = imgBrush;
                }
            }
        }
        /// <summary>
        /// Opens a FileStream to deserialize a XML file and apply it's data to the application
        /// </summary>
        /// <param name="_path">XML file path</param>
        private void ApplyXMLData(string _path)
        {
            using (FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {

                // Create a new XMLSerializer with the type of MapSave
                XmlSerializer serializer = new XmlSerializer(typeof(MapSave));

                // Create a new MapSave object with the deserialized data of the XML file
                MapSave savedMap = (MapSave)serializer.Deserialize(fs);

                // Apply the data to this applications variables
                tileSheetPath = savedMap.TileSheetPath;
                slicedTileHeight = savedMap.Height;
                slicedTileWidth = savedMap.Width;
                map = savedMap.Map;

                fs.Close();
            }
            SliceTilesheet();
            DisplayTileList();
        }
        /// <summary>
        /// Creates a new MapSave Object with the applications data
        /// </summary>
        private void SaveDataToXML()
        {
            try
            {
                // Create new SaveDialog with filters
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "XML-File|*.xml";

                if (sfd.ShowDialog() == true)
                {
                    // Create new MapSave Object with applications data
                    MapSave mapSave = new MapSave(map, tileSheetPath, slicedTileHeight, slicedTileWidth);

                    // Create XMLSerializer with the type of MapSave
                    XmlSerializer serializer = new XmlSerializer(typeof(MapSave));

                    // Create new File and write to it
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                    {
                        // Serialize the mapSave object to a new XML file
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
        /// <summary>
        /// Resizes the tiles in the applications sprite panel
        /// </summary>
        private void ResizeSpritesPanel()
        {
            if (WrapPanel_Sprites == null) return;

            // Resize every child of the Sprites panel
            for (int i = 0; i < WrapPanel_Sprites.Children.Count; i++)
            {
                // Get Rectangle child in sprite panel
                Rectangle rect = (Rectangle)WrapPanel_Sprites.Children[i];

                rect.Width = Sld_TileSize.Value;
                rect.Height = Sld_TileSize.Value;
            }
        }
        /// <summary>
        /// Applies users specified width and height to slice the tiles
        /// </summary>
        private void ApplySliceSettings()
        {
            if (Btn_SyncDimensions.IsChecked == true)
            {
                slicedTileHeight = Int32.Parse(Txt_SliceTileHeight.Text);
                slicedTileWidth = slicedTileHeight;
            }
            else
            {
                slicedTileHeight = Int32.Parse(Txt_SliceTileHeight.Text);
                slicedTileWidth = Int32.Parse(Txt_SliceTileWidth.Text);
            }
        }
        /// <summary>
        /// Draws image of the currently selected tile onto a specific Rectangle 
        /// </summary>
        /// <param name="pointOnCanvas">Location of the Rectangle the user clicked on</param>
        private void DrawOnMap(Point pointOnCanvas)
        {
            // Get new HitTestResult of the Point in the map panel
            HitTestResult result = VisualTreeHelper.HitTest(WrapPanel_Map, pointOnCanvas);

            if (result == null) return;

            try
            {
                // Get Rectangle clicked on
                Rectangle rect = (Rectangle)result.VisualHit;

                if (eraseModeActive)
                {
                    // Set map data to blank
                    map[WrapPanel_Map.Children.IndexOf(rect)] = -1;
                    rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else
                {
                    // Apply selected tile to map data
                    map[WrapPanel_Map.Children.IndexOf(rect)] = currSelectedTileIndex;
                    rect.Fill = currSelectedTile;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("There was an error while drawing on the map.");
            }
        }
        /// <summary>
        /// Selects tile of sprite panel that the user clicked on
        /// </summary>
        /// <param name="pointOnCanvas">Location of the Rectangle the user clicked on</param>
        private void SelectSprite(Point pointOnCanvas)
        {
            // Get new HitTestResult of the Point in the sprites panel
            HitTestResult result = VisualTreeHelper.HitTest(WrapPanel_Sprites, pointOnCanvas);

            if (result != null)
            {
                try
                {
                    // Get Rectangle clicked on
                    Rectangle rect = (Rectangle)result.VisualHit;

                    // Apply Rectangle data to the currently selected tile
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
        #endregion

        #region WPF Events
        private void OnClick_HideUI(object sender, RoutedEventArgs e)
        {
            ToggleUI(Grid_Options);
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
        private void LMBDown_Map(object sender, MouseButtonEventArgs e)
        {
            if (currSelectedTile == null) return;

            Point point = e.GetPosition((WrapPanel_Map));
            DrawOnMap(point);
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
            if (MessageBox.Show("Do you really want to exit?", "Exit!", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            Application.Current.Shutdown();
        }
        private void OnClick_HideSettings(object sender, RoutedEventArgs e)
        {
            ToggleUI(Grid_Settings);
        }
        private void OnClick_SelectTilesheet(object sender, RoutedEventArgs e)
        {
            SelectTilesheet();
        }

        private void OnClick_SliceTilesheet(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplySliceSettings();

                SliceTilesheetFile();

                ToggleUI(Grid_Settings);
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
            }
        }
        private void Txt_SliceTileWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Btn_SyncDimensions == null) return;

            if (Btn_SyncDimensions.IsChecked == true)
                Txt_SliceTileHeight.Text = Txt_SliceTileWidth.Text;
        }
        private void Checked_SyncDimension(object sender, RoutedEventArgs e)
        {
            Txt_SliceTileHeight.IsEnabled = false;
            Txt_SliceTileHeight.Text = Txt_SliceTileWidth.Text;
        }

        private void Unchecked_SyncDimensions(object sender, RoutedEventArgs e)
        {
            Txt_SliceTileHeight.IsEnabled = true;
        }
        private void OnClick_Minimize(object sender, RoutedEventArgs e)
        {
            // Minimize window
            WindowState = WindowState.Minimized;
        }

        private void OnClick_Maximize(object sender, RoutedEventArgs e)
        {
            // Set window dimensions to minimum
            if (this.Width == MaxWidth)
            {
                this.Width = MinWidth;
                this.Height = MinHeight;
            }
            // Set window dimensions to maximum
            else
            {
                this.Width = MaxWidth;
                this.Height = MaxHeight;
            }
        }
        #endregion
    }
}
