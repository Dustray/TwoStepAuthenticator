using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimeUnit
{
    class StrongRandom:Random
    {
        public StrongRandom():base()
        {
        }
        public StrongRandom(byte seed) : base()
        {
            
        }
    }
}
