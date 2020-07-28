using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXILEDBombGame
{
    public class PluginMain : Plugin<Config>
    {
        public override string Author => "VirtualBrightPlayz";
        public override string Name => "BombGame";
        public override Version Version => new Version(1, 0, 0);
        
        public PluginEvents PLEV;
        public static CoroutineHandle roundTimerHandle;

        public override void OnEnabled()
        {
            base.OnEnabled();
            PLEV = new PluginEvents(this);
            Exiled.Events.Handlers.Server.RoundStarted += PLEV.RoundStart;
            Exiled.Events.Handlers.Player.DroppingItem += PLEV.PlayerDropItem;
            Exiled.Events.Handlers.Player.Died += PLEV.PlayerDied;
            Exiled.Events.Handlers.Player.PickingUpItem += PLEV.PlayerPickupItem;
            Exiled.Events.Handlers.Player.InteractingDoor += PLEV.PlayerDoorInteract;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Exiled.Events.Handlers.Server.RoundStarted -= PLEV.RoundStart;
            Exiled.Events.Handlers.Player.DroppingItem -= PLEV.PlayerDropItem;
            Exiled.Events.Handlers.Player.Died -= PLEV.PlayerDied;
            Exiled.Events.Handlers.Player.PickingUpItem -= PLEV.PlayerPickupItem;
            Exiled.Events.Handlers.Player.InteractingDoor -= PLEV.PlayerDoorInteract;
            PLEV = null;
        }
    }
}
