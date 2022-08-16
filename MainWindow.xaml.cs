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
        private string tileSheetPath;

        private int slicedTileWidth = 16;
        private int slicedTileHeight = 16;

        private BitmapImage tileSheetBitmap;

        private int inspectorTileSize = 100;

        private List<CroppedBitmap> tileList = new List<CroppedBitmap>();
        private ImageBrush currSelectedTile;

        private int currSelectedTileIndex;

        private int[] map;

        private int MapTileSize = 25;

        public MainWindow()
        {
            InitializeComponent();
            FillMapCanvas();
        }

        private void FillMapCanvas()
        {
            WrapPanel_Map.Children.Clear();

            int numOfPossibleTiles = 0;

            numOfPossibleTiles = ((int)WrapPanel_Map.Height / MapTileSize) * ((int)WrapPanel_Map.Width / MapTileSize);

            map = new int[numOfPossibleTiles];

            Rectangle rect;

            for (int i = 0; i < numOfPossibleTiles; i++)
            {
                map[i] = -1;

                rect = new Rectangle();

                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                rect.Width = MapTileSize;
                rect.Height = MapTileSize;

                WrapPanel_Map.Children.Add(rect);
            }
        }
        private void SelectTilesheet()
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
        private void SliceTilesheetFile()
        {
            ApplySliceSettings();
            SliceTilesheet();
            DisplayTileList();
        }
        private void ApplySliceSettings()
        {
            Int32.TryParse(Txt_SliceTileHeight.Text, out slicedTileHeight);
            Int32.TryParse(Txt_SliceTileWidth.Text, out slicedTileWidth);
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
                rect.Width = inspectorTileSize;
                rect.Height = inspectorTileSize;

                ImageBrush imgBrush = new ImageBrush();

                imgBrush.ImageSource = tileList[i];

                rect.Fill = imgBrush;

                WrapPanel_Sprites.Children.Add(rect);
            }
        }
        private void LoadXMLMap()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "XML-Files|*.xml";

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    ApplyXMLData(ofd.FileName);

                    FillMapWithData();
                }
                catch (Exception _e)
                {
                    MessageBox.Show(_e.Message);
                }
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
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_path);

                if (doc == null) return;

                // Get XMLNodes
                XmlNode mapNode = doc.DocumentElement.SelectSingleNode($"/MapSave/Map");
                XmlNode tileSheetNode = doc.DocumentElement.SelectSingleNode("/MapSave/TileSheetPath");
                XmlNode heightNode = doc.DocumentElement.SelectSingleNode("/MapSave/Height");
                XmlNode widthNode = doc.DocumentElement.SelectSingleNode("/MapSave/Width");

                // Get the saved tilesheetPath from the XML Document
                tileSheetPath = tileSheetNode.InnerText;

                // Get the slicedTileWidht and Height from the XML Document
                Int32.TryParse(heightNode.InnerText,out slicedTileHeight);
                Int32.TryParse(widthNode.InnerText, out slicedTileWidth);

                // Update UI Textbox for Slice width and height
                Txt_SliceTileHeight.Text = slicedTileHeight.ToString();
                Txt_SliceTileWidth.Text = slicedTileWidth.ToString();

                SliceTilesheet();
                DisplayTileList();

                // Get Map data from the XML Document
                for (int i = 0; i < mapNode.ChildNodes.Count; i++)
                {
                    map[i] = Int32.Parse(mapNode.ChildNodes[i].InnerText);
                }
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
            }
        }

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
            FillMapCanvas();
        }
        private void OnClick_SaveMap(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "XML-File|*.xml";

            if (sfd.ShowDialog() == true)
            {
                MapSave mapSave = new MapSave(map, tileSheetPath, slicedTileHeight, slicedTileWidth);

                XmlSerializer serializer = new XmlSerializer(mapSave.GetType());

                try
                {
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                    {
                        serializer.Serialize(fs, mapSave);
                    }
                }
                catch (Exception _e)
                {
                    MessageBox.Show(_e.Message);
                }
            }
        }
        private void OnClick_LoadMap(object sender, RoutedEventArgs e)
        {
            LoadXMLMap();
        }
        private void LMBDown_StackSprites(object sender, MouseButtonEventArgs e)
        {
            Point pointOnCanvas = e.GetPosition(WrapPanel_Sprites);

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
            Point point = e.GetPosition((WrapPanel_Map));

            HitTestResult result = VisualTreeHelper.HitTest(WrapPanel_Map, point);

            if (result != null)
            {
                try
                {
                    Rectangle rect = (Rectangle)result.VisualHit;

                    map[WrapPanel_Map.Children.IndexOf(rect)] = currSelectedTileIndex;

                    rect.Fill = currSelectedTile;
                }
                catch (Exception _e)
                {
                    MessageBox.Show(_e.Message);
                }
            }
        }
        #endregion
    }
}
