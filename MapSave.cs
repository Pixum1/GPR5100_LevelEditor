using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPR5100_LevelEditor
{
    [System.Serializable]
    public class MapSave
    {
        public int[] Map = new int[0];
        public string TileSheetPath = "";
        public int Height;
        public int Width;

        public MapSave()
        {

        }
        public MapSave(int[] _map, string _path, int _height, int _width)
        {
            Map = _map;
            TileSheetPath = _path;
            Height = _height;
            Width = _width;
        }
    }
}
