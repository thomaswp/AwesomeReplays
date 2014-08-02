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

        public int abilityStart;
        public int nameStart;
        public ActorInfo info;
        public string playerName;

        public int RightBit { get { return index + bitLength; } }

        public float GetAverageMovement()
        {
            float dis = 0;
            for (int i = 0; i < moves.Length - 1; i++)
            {
                MoveInfo now = moves[i], next = moves[i + 1];
                float dx = now.XRel - next.XRel;
                float dy = now.YRel - next.YRel;
                dis += dx * dx + dy * dy;
            }
            return dis / moves.Length;
        }

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
            int time = -1;
            List<MoveInfo> infos = new List<MoveInfo>();
            for (int i = 0; i < length; i++)
            {
                if (index >= data.Length) return null;
                MoveInfo info = MoveInfo.Create(data, index);
                if (info.time <= time) return null;
                time = info.time;
                index += MoveInfo.TOTAL_BITS;
                infos.Add(info);
            }
            if (infos.Count == 0) return null;
            return new CharacterBlock(originalIndex, id, infos.ToArray());
        }

        public override string ToString()
        {
            string s = info.name;
            if (playerName != null) s += " (" + playerName + ")";
            s += " [" + moves.Length + "]";
            return s;
        }
    }
}
