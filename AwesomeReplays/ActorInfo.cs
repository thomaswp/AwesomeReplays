using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class ActorInfo
    {
        public readonly string alias, name;
        public readonly bool isHero;

        public ActorInfo(string alias, string name, bool isHero)
        {
            this.alias = alias;
            this.name = name;
            this.isHero = isHero;
        }

        public override string ToString()
        {
            return alias + "/" + name;
        }
    }
}
