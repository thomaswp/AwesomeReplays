using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace AwesomeReplays
{
    public class BitData
    {

        public readonly bool[] data;

        public int Length { get { return data.Length; } }

        public BitData(string filename) : this(File.ReadAllBytes(filename)) { }

        public BitData(byte[] bytes)
        {
            BitArray barray = new BitArray(bytes);
            data = new bool[barray.Length];
            barray.CopyTo(data, 0);
        }

        public int ReadInt(int index, int length)
        {
            if (length > 32) throw new Exception("Too large for int");
            return (int)ReadLong(index, length);
        }

        public long ReadLong(int index, int length)
        {
            long v = 0;
            for (int i = index + length - 1; i >= index; i--)
            {
                v <<= 1;
                if (i < data.Length && data[i]) v |= 1;
            }
            return v;
        }

        public string ReadString(int index)
        {
            string s = "";
            while (index < Length)
            {
                char c = (char)ReadInt(index, 8);
                if (c == 0) break;
                s += c;
                index += 8;
            }
            return s;
        }

        public string ReadBlock(int index, int length)
        {
            string s = "";
            for (int i = index + length - 1; i >= index; i--)
            {
                s += data[i] ? "1" : "0";
            }
            return s;
        }

        public List<int> SearchInt(int value, int length)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < data.Length; i++)
            {
                if (value == ReadInt(i, length)) indices.Add(i); 
            }
            return indices;
        }

        public List<int> SearchIntSeries(int[] series, int length)
        {
            List<int> indices = new List<int>();
            int index = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (ReadInt(i, length) == series[index])
                {
                    indices.Add(i);
                    index++;
                    if (index == series.Length) return indices;
                }
            }
            return null;
        }
    }
}
