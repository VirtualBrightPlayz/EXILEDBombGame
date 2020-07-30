using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXILEDBombGame
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ResetTeams : ICommand
    {
        public string Command => "resetteams";

        public string[] Aliases => new string[0];

        public string Description => "Resets the team list";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PluginMain.instance.PLEV.CIPlayers.Clear();
            PluginMain.instance.PLEV.NTFPlayers.Clear();
            response = "Teams reset.";
            return true;
        }
    }
}
