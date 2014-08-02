using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class ActorInfo
    {
        private static Dictionary<string, ActorInfo> classNames = new Dictionary<string, ActorInfo>();

        static ActorInfo()
        {
            addClass("Vampire", "Ayla", true);
            addClass("Tank", "Clunk", true);
            addClass("Blazer", "Coco", true);
            addClass("Heavy", "Derpl", true);
            addClass("Dasher", "Froggy G.", true, "Froggy");
            addClass("Butterfly", "Genji", true);
            addClass("Maw", "Gnaw", true);
            addClass("Chameleon", "Leon", true);
            addClass("Cowboy", "Lonestar", true);
            addClass("Assassin", "Penny Fox", true, "Penny");
            addClass("Hunter", "Raelynn", true);
            addClass("Spy", "Sentry X-58", true, "Sentry");
            addClass("Brute", "Skolldir", true);
            addClass("Shaman", "Skree", true);
            addClass("Captain", "Swiggins", true);
            addClass("Commando", "Ted", true);
            addClass("Bird", "Vinnie & Spike", true, "Vinnie");
            addClass("Summoner", "Voltar", true);
            addClass("Jetter", "Yuri", true);

            addClass("CreepCritterRibbit4", "Ribbit IV Creep", false, "Health_Creep");
            addClass("CreepCritterSorona", "Sorona Creep", false, "Health_Creep");
            addClass("CreepCritterAIStation404", "AI Station Creep", false, "Health_Creep");
            addClass("CreepCritterCrab", "Health Creep", false);
            addClass("CreepRoshan", "Solarboss", false);
            addClass("CreepDroidMelee", "Melee Creep", false);
            addClass("CreepDroidFlying", "Flying Creep", false);
            addClass("CreepDroidSuper", "Missile Creep", false);
            addClass("CreepCowboyBull", "Lonestar Bull", false);
            addClass("CreepChameleonClone", "Leon Clone", false);
            addClass("CreepSummonerDrone", "Voltar Drone", false);
            addClass("CreepSummonerHealtotem", "Voltar Healbot", false);
            addClass("CreepMawTurret", "Gnaw Weedling", false);
            addClass("CreepCaptainChain", "Swiggins Humbolt", false);
            addClass("CreepCommandoAirstrike", "Ted Airstrike", false);
            addClass("CreepShamanWall", "Skree Totem", false);
            addClass("CreepSpyBooth", "Sentry Teleporter", false);
            addClass("CreepSpy", "Sentry Blackhole", false);
        }

        public readonly string alias, name, icon;
        public readonly bool isHero;

        private static void addClass(string alias, string name, bool isHero, string icon = null)
        {
            if (icon == null) icon = name;
            icon = "Avatar_" + icon.Replace(" ", "_") + ".png";
            classNames.Add(alias, new ActorInfo(alias, name, isHero, icon));
        }

        public static ActorInfo GetFromAlias(string className)
        {
            return classNames.ContainsKey(className) ? classNames[className] : null;
        }

        public ActorInfo(string alias, string name, bool isHero, string icon)
        {
            this.alias = alias;
            this.name = name;
            this.isHero = isHero;
            this.icon = icon;
        }

        public override string ToString()
        {
            return alias + "/" + name;
        }
    }
}
