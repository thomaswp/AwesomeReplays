using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class CharacterBlock
    {
        private static Dictionary<string, ActorInfo> classNames = new Dictionary<string, ActorInfo>();

        static CharacterBlock()
        {
            addClass("Vampire", "Ayla", true);
            addClass("Tank", "Clunk", true);
            addClass("Blazer", "Coco", true);
            addClass("Heavy", "Derpl", true);
            addClass("Dasher", "Froggy G.", true);
            addClass("Butterfly", "Genji", true);
            addClass("Maw", "Gnaw", true);
            addClass("Chameleon", "Leon", true);
            addClass("Cowboy", "Lonestar", true);
            addClass("Assassin", "Penny Fox", true);
            addClass("Hunter", "Raelynn", true);
            addClass("Spy", "Sentry X-58", true);
            addClass("Brute", "Skolldir", true);
            addClass("Shaman", "Skree", true);
            addClass("Captain", "Swiggins", true);
            addClass("Commando", "Ted", true);
            addClass("Bird", "Vinnie & Spike", true);
            addClass("Summoner", "Voltar", true);
            addClass("Jetter", "Yuri", true);

            addClass("CreepCritterRibbit4", "Ribbit IV Creep");
            addClass("CreepCritterSorona", "Sorona Creep");
            addClass("CreepCritterAIStation404", "AI Station Creep");
            addClass("CreepCritterCrab", "Health Creep");
            addClass("CreepRoshan", "Solarboss");
            addClass("CreepDroidMelee", "Melee Creep");
            addClass("CreepDroidFlying", "Flying Creep");
            addClass("CreepDroidSuper", "Missile Creep");
            addClass("CreepCowboyBull", "Lonestar Bull");
            addClass("CreepChameleonClone", "Leon Clone");
            addClass("CreepSummonerDrone", "Voltar Drone");
            addClass("CreepSummonerHealtotem", "Voltar Healbot");
            addClass("CreepMawTurret", "Gnaw Weedling");
            addClass("CreepCaptainChain", "Swiggins Humbolt");
            addClass("CreepCommandoAirstrike", "Ted Airstrike");
            addClass("CreepShamanWall", "Skree Totem");
            addClass("CreepSpyBooth", "Sentry Teleporter");
            addClass("CreepSpy", "Sentry Blackhole");
        }

        private static void addClass(string alias, string name, bool isHero = false)
        {
            classNames.Add(alias, new ActorInfo(alias, name, isHero));
        }
        
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

        public static ActorInfo GetCharacterName(string className)
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

        public override string ToString()
        {
            string s = info.name;
            if (playerName != null) s += " (" + playerName + ")";
            s += " [" + moves.Length + "]";
            return s;
        }
    }
}
