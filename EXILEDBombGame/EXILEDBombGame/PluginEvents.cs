using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Hints;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EXILEDBombGame
{
    public class PluginEvents
    {
        private PluginMain plugin;
        public float timeLeft = 0f;
        public float bombTimer = 0f;
        public bool bombPlanted = false;
        public CoroutineHandle plantHandle;
        public Pickup bomb;
        public Room bombsite;
        public int CI = 0;
        public int NTF = 0;
        public bool bombSpawned = false;

        public PluginEvents(PluginMain main)
        {
            plugin = main;
        }

        public void RoundStart()
        {
            Timing.CallDelayed(1f, () =>
            {
                bombPlanted = false;
                CI = 0;
                NTF = 0;
                bombSpawned = false;
                SetupItems();
                SetupBombSites();
                SetupPlayers();
                timeLeft = plugin.Config.RoundTime;
                Timing.KillCoroutines(PluginMain.roundTimerHandle);
                Timing.KillCoroutines(plantHandle);
                PluginMain.roundTimerHandle = Timing.RunCoroutine(RoundTimer());
            });
            
        }

        public void Waiting()
        {
            Timing.CallDelayed(1f, () =>
            {
                Timing.KillCoroutines(PluginMain.roundTimerHandle);
                Timing.KillCoroutines(plantHandle);
            });
        }

        private void SetupItems()
        {
            var pcs = GameObject.FindObjectsOfType<Pickup>();
            foreach (var item in pcs)
            {
                item.Delete();
            }
        }

        public void RespawnTeam(RespawningTeamEventArgs ev)
        {
            ev.Players.Clear();
            ev.MaximumRespawnAmount = 0;
        }

        private void SetupPlayers()
        {
            Room cispawn = Map.Rooms.First(p => p.Name.Contains(plugin.Config.CISpawn));
            Room ntfspawn = Map.Rooms.First(p => p.Name.Contains(plugin.Config.NTFSpawn));
            for (int i = 0; i < Player.List.Count(); i++)
            {
                Player plr = Player.List.ElementAt(i);
                if (i % 2 == 0)
                {
                    SpawnAsNTF(plr, ntfspawn);
                }
                else
                {
                    SpawnAsCI(plr, cispawn);
                }
            }
        }

        private void SpawnAsNTF(Player player, Room spawn)
        {
            player.SetRole(RoleType.NtfLieutenant, true);
            player.Position = spawn.Transform.position + Vector3.up * 1.5f;
            player.ClearInventory();
            player.Broadcast(10, plugin.Config.NTFSpawnText);
            foreach (var item in plugin.Config.NTFItems)
            {
                player.AddItem(item);
            }
        }

        private void SpawnAsCI(Player player, Room spawn)
        {
            player.SetRole(RoleType.ChaosInsurgency, true);
            player.Position = spawn.Transform.position + Vector3.up * 1.5f;
            player.ClearInventory();
            player.Broadcast(10, plugin.Config.CISpawnText);
            if (!bombSpawned)
            {
                bombSpawned = true;
                player.AddItem(ItemType.Coin);
            }
            foreach (var item in plugin.Config.CIItems)
            {
                player.AddItem(item);
            }
        }

        private void SetupBombSites()
        {
            bombsite = Map.Rooms.First(p => p.Name.Contains(plugin.Config.BombsiteSpawn));
        }

        private IEnumerator<float> RoundTimer()
        {
            CI = Player.List.Where(p => p.Role == RoleType.ChaosInsurgency).Count();
            NTF = Player.List.Where(p => p.Role == RoleType.NtfLieutenant).Count();
            while (true)
            {
                DisplayHintAll(FormatInfo(), 1f, null);
                yield return Timing.WaitForSeconds(1f);
                timeLeft -= 1f;
                if (bombPlanted)
                {
                    bombTimer -= 1f;
                    if (bombTimer <= 0f)
                    {
                        foreach (var plr in Player.List)
                        {
                            if (plr.Role != RoleType.ChaosInsurgency)
                            {
                                plr.Kill();
                            }
                        }
                        AlphaWarheadController.Host.Detonate();
                        Map.Broadcast(10, plugin.Config.BombExplodeText);
                        break;
                    }
                }
                if (timeLeft <= 0f)
                {
                    foreach (var plr in Player.List)
                    {
                        plr.Kill();
                    }
                    Map.Broadcast(10, plugin.Config.RoundDrawText);
                    break;
                }
                CI = Player.List.Where(p => p.Role == RoleType.ChaosInsurgency).Count();
                NTF = Player.List.Where(p => p.Role == RoleType.NtfLieutenant).Count();
            }
        }

        public string FormatInfo()
        {
            return plugin.Config.RoundInfoText.Replace("%roundtimer%", timeLeft.ToString()).Replace("%ntf%", NTF.ToString()).Replace("%ci%", CI.ToString()).Replace("%bombplanted%", FormatBombInfo());
        }

        public void PlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            if (ev.Door.CheckpointDoor && ev.Door.DoorName.Contains("ENT"))
            {
                ev.IsAllowed = false;
                return;
            }
            ev.IsAllowed = true;
        }

        public void PlayerPickupItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup == bomb || ev.Pickup.info.itemId == ItemType.Coin)
            {
                if (bombPlanted)
                {
                    ev.IsAllowed = false;
                    if (!plantHandle.IsValid && !plantHandle.IsRunning)
                    {
                        plantHandle = Timing.RunCoroutine(DiffuseBomb(ev.Player));
                    }
                }
            }
        }

        private IEnumerator<float> DiffuseBomb(Player player)
        {
            //if (player.Inventory.items.FindIndex(p => p.id == ItemType.Disarmer) != -1)
            if (player.Role == RoleType.NtfLieutenant)
            {
                player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(1);
                yield return Timing.WaitForSeconds(plugin.Config.DiffuseTime);
                if (player.Role == RoleType.NtfLieutenant)
                {
                    Map.Broadcast(5, plugin.Config.BombDiffuseText);
                    player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(0);
                    foreach (var plr in Player.List)
                    {
                        if (plr.Role != RoleType.NtfLieutenant)
                        {
                            plr.Kill();
                        }
                    }
                }
            }
            Timing.KillCoroutines(plantHandle);
            plantHandle = new CoroutineHandle();
        }

        public string FormatBombInfo()
        {
            if (bombPlanted)
                return "True" + plugin.Config.BombInfoText.Replace("%bombtimer%", bombTimer.ToString());
            return "False";
        }

        public void PlayerDropItem(DroppingItemEventArgs ev)
        {
            if (ev.Item.id == ItemType.Coin)
            {
                if (Vector3.Distance(ev.Player.Position, bombsite.Transform.position) <= plugin.Config.DistanceFromSite)
                {
                    ev.IsAllowed = false;
                    if (!plantHandle.IsValid && !plantHandle.IsRunning)
                    {
                        plantHandle = Timing.RunCoroutine(PlantBomb(ev.Player));
                    }
                }
            }
        }

        public void PlayerDied(DiedEventArgs ev)
        {
        }

        private IEnumerator<float> PlantBomb(Player player)
        {
            if (Vector3.Distance(player.Position, bombsite.Transform.position) > plugin.Config.DistanceFromSite)
            {
                Timing.KillCoroutines(plantHandle);
                plantHandle = new CoroutineHandle();
                yield break;
            }
            player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(1);
            yield return Timing.WaitForSeconds(plugin.Config.PlantTime);
            if (player.Role == RoleType.ChaosInsurgency && player.Inventory.items.FindIndex(p => p.id == ItemType.Coin) != -1 && Vector3.Distance(player.Position, bombsite.Transform.position) <= plugin.Config.DistanceFromSite)
            {
                player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(0);
                player.Inventory.items.RemoveAt(player.Inventory.items.FindIndex(p => p.id == ItemType.Coin));
                Map.Broadcast(5, plugin.Config.BombPlantText);
                bomb = ItemType.Coin.Spawn(0f, player.Position + Vector3.up * 1.5f);
                bombTimer = plugin.Config.BombTimer;
                timeLeft += bombTimer;
                bombPlanted = true;
            }
            Timing.KillCoroutines(plantHandle);
            plantHandle = new CoroutineHandle();
        }

        public static void DisplayHintAll(string hint, float time, HintEffect[] effects)
        {
            foreach (Player player in Player.List)
            {
                player.ReferenceHub.hints.Show(new TextHint(hint, new HintParameter[]
                {
                    new StringHintParameter("")
                }, effects, time));
            }
        }
    }
}