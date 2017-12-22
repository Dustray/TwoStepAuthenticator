using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimeSpace

{
    public class DateTimeUnit
    {
        public class Seconds
        {
            public static long ToMillis(long second)
            {
                return second * 1000;

            }
        }
    }
}
