
namespace Expression.Map
{
    [System.Serializable]
    public class Hd2dTileInfoList
    {
        public int length;
        public Hd2dTileInfo[] tiles;

        public Hd2dTileInfoList(int length)
        {
            this.length = length;
            tiles = new Hd2dTileInfo[length];
        }

        public Hd2dTileInfo this[long i]
        {
            get { return tiles[i]; }
            set { tiles[i] = value; }
        }
    }
}
