using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeReplays
{
    public struct TimedData
    {
        public const int TIME_BITS = 9;
        public const int MAX_TIME = 1 << TIME_BITS;

        public readonly int index;
        public readonly int time, value;
        public readonly int dataLength;

        public int NextBit { get { return index + TIME_BITS + dataLength; } }

        public TimedData(BitData data, int index, int dataLength)
            : this(index, data.ReadInt(index, TIME_BITS), 
                   data.ReadInt(index + TIME_BITS, dataLength), dataLength)
        { }

        public TimedData(int index, int time, int value, int dataLength)
        {
            this.index = index;
            this.time = time;
            this.value = value;
            this.dataLength = dataLength;
        }

        public override string ToString()
        {
            return string.Format("({0}: {1})", time, value);
        }

        public static List<TimedData> ReadList(BitData data, int index, int dataLength, int countLength = 6)
        {
            List<TimedData> list = new List<TimedData>();
            int num = data.ReadInt(index, countLength);
            index += countLength;
            int lastTime = -1;
            for (int i = 0; i < num; i++)
            {
                if (index >= data.Length) return null;

                TimedData td = new TimedData(data, index, dataLength);
                index = td.NextBit;
                list.Add(td);

                if (lastTime == -1 && td.time != 0) return null;
                if (td.time < lastTime) return null;
                lastTime = td.time;
            }
            return list;
        }
    }
}
