using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.Movies
{
    public class Cast : Person
    {
        public string Character { get; set; }
        public int Order { get; set; }
    }
}
