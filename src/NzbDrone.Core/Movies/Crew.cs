using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.Movies
{
    public class Crew : Person
    {
        public string Department { get; set; }
        public string Job { get; set; }
    }
}
