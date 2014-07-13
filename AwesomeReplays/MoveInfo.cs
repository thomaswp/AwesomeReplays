using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public struct MoveInfo
    {
        public const int COORD_BITS = 13;
        public const int MAX_COORD = 1 << COORD_BITS;

        public const int TIME_BITS = 9;
        public const int MAX_TIME = 1 << TIME_BITS;

        public const int TOTAL_BITS = TIME_BITS + COORD_BITS * 2;

        public int index, x, y, time;
        public override string ToString()
        {
            return string.Format("{0:D5} [{3:D3}]: ({1:D4}, {2:D4})", index, x, y, time);
        }

        public static MoveInfo Create(BitData data, int i)
        {
            return new MoveInfo()
            {
                index = i,
                time = data.ReadInt(i, TIME_BITS),
                x = data.ReadInt(i + TIME_BITS, COORD_BITS),
                y = data.ReadInt(i + TIME_BITS + COORD_BITS, COORD_BITS)
            };
        }
    }
}
