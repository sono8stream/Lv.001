using System;

namespace Util.Wolf
{
    class WolfDataReader
    {

        private byte[] bytes;

        public WolfDataReader(string filePath)
        {
            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
            }
        }

        public int ReadByte(int offset, out int nextOffset)
        {
            int res = bytes[offset];
            nextOffset = offset + 1;
            return res;
        }

        public int ReadInt(int offset, bool isLittleEndian, out int nextOffset)
        {
            if (offset < 0 || offset + 4 > bytes.Length)
            {
                nextOffset = offset;
                return 0;
            }

            int res = 0;
            if (isLittleEndian)
            {
                for (int i = 4 - 1; i >= 0; i--)
                {
                    res *= 256;
                    res += bytes[offset + i];
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    res *= 256;
                    res += bytes[offset + i];
                }
            }

            nextOffset = offset + 4;
            return res;
        }

        public int[,] ReadLayer(int width, int height, int offset)
        {
            int[,] mapData = new int[height, width];
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    int val = ReadInt(offset, true, out offset);
                    mapData[i, j] = val;
                }
            }

            return mapData;
        }

        public string ReadString(int offset, out int nextOffset)
        {
            int byteLength = ReadInt(offset, true, out offset);
            byte[] strBytes = new byte[byteLength];
            Array.Copy(bytes, offset, strBytes, 0, byteLength);

            nextOffset = offset + byteLength;
            string res = System.Text.Encoding.GetEncoding("shift_jis").GetString(strBytes);
            res = res.Substring(0, res.Length - 1);// 末尾のスペースを削除
            return res;
        }
    }
}