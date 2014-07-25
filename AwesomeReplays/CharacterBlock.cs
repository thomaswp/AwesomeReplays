using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class CharacterBlock
    {
        private static Dictionary<string, string> classNames = new Dictionary<string,string>();
        static CharacterBlock()
        {
            classNames.Add("Vampire", "Ayla");
            classNames.Add("Tank", "Clunk");
            classNames.Add("Blazer", "Coco");
            classNames.Add("Heavy", "Derpl");
            classNames.Add("Dasher", "Froggy G.");
            classNames.Add("Butterfly", "Genji");
            classNames.Add("Maw", "Gnaw");
            classNames.Add("Cowboy", "Lonestar");
            classNames.Add("Assassin", "Penny Fox");
            classNames.Add("Hunter", "Raelynn");
            classNames.Add("Spy", "Sentry X-58");
            classNames.Add("Brute", "Skolldir");
            classNames.Add("Shaman", "Skree");
            classNames.Add("Captain", "Swiggins");
            classNames.Add("Commando", "Ted");
            classNames.Add("Summoner", "Voltar");
            classNames.Add("Jetter", "Yuri");

            classNames.Add("CreepCritterRibbit4", "Ribbit IV Creep");
            classNames.Add("CreepCritterSorona", "Sorona Creep");
            classNames.Add("CreepCritterAIStation404", "AI Station Creep");
            classNames.Add("CreepCritterCrab", "Health Creep");
            classNames.Add("CreepRoshan", "Solarboss");
            classNames.Add("CreepDroidMelee", "Melee Creep");
            classNames.Add("CreepDroidFlying", "Flying Creep");
            classNames.Add("CreepDroidSuper", "Missile Creep");
            classNames.Add("CreepCowboyBull", "Lonestar Bull");
            classNames.Add("CreepChameleonClone", "Leon Clone");
            classNames.Add("CreepSummonerDrone", "Voltar Drone");
            classNames.Add("CreepSummonerHealtotem", "Voltar Healbot");
            classNames.Add("CreepMawTurret", "Gnaw Weedling");
            classNames.Add("CreepCaptainChain", "Swiggins Humbolt");
            classNames.Add("CreepCommandoAirstrike", "Ted Airstrike");
            classNames.Add("CreepShamanWall", "Skree Totem");
            classNames.Add("CreepSpyBooth", "Sentry Teleporter");
            classNames.Add("CreepSpy", "Sentry Blackhole");
        }
        
        public const int ID_BITS = 18;
        public const int COUNT_BITS = 7;

        public readonly int id;
        public readonly int index;
        public readonly MoveInfo[] moves;
        public readonly int bitLength;

        public int abilityStart;
        public string name;

        public static string GetCharacterName(string className)
        {
            return classNames.ContainsKey(className) ? classNames[className] : null;
        }

        public int RightBit { get { return index + bitLength; } }

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
            return new CharacterBlock(originalIndex, id, infos.ToArray());
        }

    }
}
