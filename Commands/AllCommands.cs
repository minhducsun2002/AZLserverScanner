using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZLServerProtobuf.Commands
{
    public class AllCommands
    {
        public static Dictionary<int, Type> Commands { get; set; } = new Dictionary<int, Type>()
        {
            {SC10801.CommandID, typeof(SC10801)},
            {CS10800.CommandID, typeof(CS10800)}
        };

        public static Type GetCommandType(int commandId)
        {
            try
            {
                return Commands[commandId];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
