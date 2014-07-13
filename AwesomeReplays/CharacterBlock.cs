using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class CharacterBlock
    {
        public const int ID_BITS = 18;
        public const int COUNT_BITS = 7;

        public readonly int id;
        public readonly int index;
        public readonly MoveInfo[] moves;
        public readonly int bitLength;

        private CharacterBlock(int index, int id, MoveInfo[] moves)
        {
            this.index = index;
            this.id = id;
            this.moves = moves;
            this.bitLength = ID_BITS + COUNT_BITS + moves.Length * MoveInfo.TOTAL_BITS;
        }

        public static CharacterBlock Read(BitData data, int index)
        {
            int originalIndex = index;
            int id = data.ReadInt(index, ID_BITS);
            index += ID_BITS;
            int length = data.ReadInt(index, COUNT_BITS);
            index += COUNT_BITS;
            int time = 0;
            List<MoveInfo> infos = new List<MoveInfo>();
            for (int i = 0; i < length; i++)
            {
                if (index >= data.Length) return null;
                MoveInfo info = MoveInfo.Create(data, index);
                if (info.time < time) return null;
                time = info.time;
                index += MoveInfo.TOTAL_BITS;
                infos.Add(info);
            }
            return new CharacterBlock(originalIndex, id, infos.ToArray());
        }
    }
}
