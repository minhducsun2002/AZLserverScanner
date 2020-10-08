using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZLServerProtobuf.Commands
{
    class CommandBase
    {
        public string Description { get; internal set; }
        public bool IsCS { get; internal set; }
    }
}
