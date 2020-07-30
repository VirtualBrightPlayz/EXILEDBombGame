using CustomPlayerEffects;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Hints;
using MEC;
using Mirror;
using PlayableScps.Messages;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using Targeting;
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
        public CoroutineHandle buyHandle;
        public Pickup bomb;
        public List<Room> bombsite;
        public int CI = 0;
        public int NTF = 0;
        public bool bombSpawned = false;
        public bool bombDiffused = false;
        public bool diffusing = false;
        public bool roundStarted = true;
        public Player diffuser = null;
        public List<Player> NTFList;
        public List<Player> CIList;
        public List<string> CIPlayers = new List<string>();
        public List<string> NTFPlayers = new List<string>();
        public ReferenceHub bombdummy;

        public PluginEvents(PluginMain main)
        {
            plugin = main;
        }

        public void RoundStart()
        {
            plugin.canBuy = true;
            PluginMain.roundCount++;
            if (PluginMain.roundCount > plugin.Config.RoundsBeforeReset)
            {
                PluginMain.money = new Dictionary<string, int>();
                PluginMain.roundCount = 0;
                CIPlayers = new List<string>();
                NTFPlayers = new List<string>();
            }
            roundStarted = false;
            Timing.CallDelayed(1f, () =>
            {
                bombPlanted = false;
                CI = 0;
                NTF = 0;
                bombSpawned = false;
                bombDiffused = false;
                diffusing = false;
                diffuser = null;
                roundStarted = false;
                SetupItems();
                SetupBombSites();
                SetupPlayers();
                timeLeft = plugin.Config.RoundTime;
                plugin.canBuy = true;
                Timing.KillCoroutines(PluginMain.roundTimerHandle);
                Timing.KillCoroutines(plantHandle);
                Timing.KillCoroutines(buyHandle);
                PluginMain.roundTimerHandle = Timing.RunCoroutine(RoundTimer());
                buyHandle = Timing.CallDelayed(plugin.Config.BuyTimer, () =>
                {
                    plugin.canBuy = false;
                });
                Timing.CallDelayed(1f, () =>
                {
                    roundStarted = true;
                });
            });

        }

        public void PlayerRoleChange(ChangingRoleEventArgs ev)
        {
            if (roundStarted)
                ev.NewRole = RoleType.Spectator;
        }

        public void Waiting()
        {
            Timing.CallDelayed(1f, () =>
            {
                Timing.KillCoroutines(PluginMain.roundTimerHandle);
                Timing.KillCoroutines(plantHandle);
                Timing.KillCoroutines(buyHandle);
                plugin.canBuy = false;
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

        public void EndRoundCheck(EndingRoundEventArgs ev)
        {
            if (plugin.canBuy)
            {
                ev.IsAllowed = false;
                ev.IsRoundEnded = false;
                return;
            }
            CI = Player.List.Where(p => p.Role == plugin.Config.CIRole).Count();
            NTF = Player.List.Where(p => p.Role == plugin.Config.NTFRole).Count();
            if (NTF == 0)
            {
                foreach (var plr in Player.List)
                {
                    if (plr.Role != plugin.Config.CIRole)
                    {
                        plr.Kill();
                    }
                }
                Map.Broadcast(10, plugin.Config.RoundCIWin);
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                ev.LeadingTeam = RoundSummary.LeadingTeam.ChaosInsurgency;
                AddMoney(plugin.Config.RoundWinMoney, CIList);
                AddMoney(plugin.Config.RoundLoseMoney, NTFList);
            }
            else if (CI == 0 && bombPlanted && bombTimer > 0f)
            {
                ev.IsAllowed = false;
                ev.IsRoundEnded = false;
            }
            else if (CI == 0 && !bombPlanted)
            {
                foreach (var plr in Player.List)
                {
                    if (plr.Role != plugin.Config.NTFRole)
                    {
                        plr.Kill();
                    }
                }
                Map.Broadcast(5, plugin.Config.RoundNTFWin);
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                ev.LeadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                AddMoney(plugin.Config.RoundWinMoney, NTFList);
                AddMoney(plugin.Config.RoundLoseMoney, CIList);
            }
            else if (timeLeft <= 0f)
            {
                foreach (var plr in Player.List)
                {
                    plr.Kill();
                }
                Map.Broadcast(10, plugin.Config.RoundNTFWin);
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                ev.LeadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                AddMoney(plugin.Config.RoundWinMoney, NTFList);
                AddMoney(plugin.Config.RoundLoseMoney, CIList);
            }
            else if (bombTimer <= 0f && bombPlanted)
            {
                foreach (var plr in Player.List)
                {
                    if (plr.Role != plugin.Config.CIRole)
                    {
                        plr.Kill();
                    }
                }
                AlphaWarheadController.Host.Detonate();
                Map.Broadcast(10, plugin.Config.BombExplodeText);
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                ev.LeadingTeam = RoundSummary.LeadingTeam.ChaosInsurgency;
                AddMoney(plugin.Config.RoundWinMoney, CIList);
                AddMoney(plugin.Config.RoundLoseMoney, NTFList);
            }
            else if (bombDiffused)
            {
                foreach (var plr in Player.List)
                {
                    if (plr.Role != plugin.Config.NTFRole)
                    {
                        plr.Kill();
                    }
                }
                Map.Broadcast(5, plugin.Config.BombDiffuseText);
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                ev.LeadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                AddMoney(plugin.Config.RoundWinMoney, NTFList);
                AddMoney(plugin.Config.RoundLoseMoney, CIList);
            }
            else
            {
                ev.IsAllowed = false;
                ev.IsRoundEnded = false;
            }
        }

        public void PlayerElevatorInteract(InteractingElevatorEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        public void AddMoney(int amnt, RoleType role)
        {
            foreach (var plr in Player.List)
            {
                if (plr.Role == role)
                    AddMoney(amnt, plr);
            }
        }

        public void AddMoney(int amnt, List<Player> players)
        {
            foreach (var plr in players)
            {
                AddMoney(amnt, plr);
            }
        }

        public void AddMoney(int amnt, Player plr)
        {
            if (!PluginMain.money.ContainsKey(plr.UserId))
            {
                PluginMain.money.Add(plr.UserId, plugin.Config.StartMoney);
            }
            PluginMain.money[plr.UserId] += amnt;
        }

        public void RespawnTeam(RespawningTeamEventArgs ev)
        {
            ev.Players.Clear();
            ev.MaximumRespawnAmount = 0;
        }

        // https://stackoverflow.com/questions/273313/randomize-a-listt
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        private void SetupPlayers()
        {
            Room cispawn = Map.Rooms.First(p => p.Name.Contains(plugin.Config.CISpawn));
            Room ntfspawn = Map.Rooms.First(p => p.Name.Contains(plugin.Config.NTFSpawn));
            CIList = new List<Player>();
            NTFList = new List<Player>();
            var lst = Player.List.ToList();
            //lst.RemoveAll(p => CIPlayers.Contains(p.UserId));
            //lst.RemoveAll(p => NTFPlayers.Contains(p.UserId));
            lst = Shuffle<Player>(lst).ToList();
            int offset = 0;
            if (CIPlayers.Count <= NTFPlayers.Count)
            {
                offset++;
            }
            for (int i = 0; i < lst.Count(); i++)
            {
                Player plr = lst[i];
                if (NTFPlayers.Contains(plr.UserId))
                {
                    SpawnAsNTF(plr, ntfspawn);
                    continue;
                }
                if (CIPlayers.Contains(plr.UserId))
                {
                    SpawnAsCI(plr, cispawn);
                    continue;
                }
                if (offset % 2 == 0)
                {
                    SpawnAsNTF(plr, ntfspawn);
                }
                else
                {
                    SpawnAsCI(plr, cispawn);
                }
                offset++;
            }
            /*try
            {
                foreach (var room in bombsite)
                {
                    var dum = SpawnDummyModel(room.Transform.position + Vector3.up * 5f, Quaternion.identity, RoleType.Tutorial, 1f, 1f, 1f);
                    foreach (var player in Player.List)
                    {
                        NetworkServer.SendToAll<Scp096TriggerMessage>(new Scp096TriggerMessage(player.ReferenceHub, dum)
                        {
                        }, 0);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }*/
        }

        private void SpawnAsNTF(Player player, Room spawn)
        {
            NTFList.Add(player);
            if (!NTFPlayers.Contains(player.UserId))
            {
                NTFPlayers.Add(player.UserId);
            }
            player.SetRole(plugin.Config.NTFRole, true);
            player.Health = plugin.Config.PlayerMaxHP;
            player.MaxHealth = plugin.Config.PlayerMaxHP;
            player.Position = spawn.Transform.position + Vector3.up * 1.5f;
            player.ClearInventory();
            player.Broadcast(10, plugin.Config.NTFSpawnText);
            foreach (var item in plugin.Config.NTFItems)
            {
                player.AddItem(item);
            }
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato556, 500);
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, 500);
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, 500);
        }

        private void SpawnAsCI(Player player, Room spawn)
        {
            CIList.Add(player);
            if (!CIPlayers.Contains(player.UserId))
            {
                CIPlayers.Add(player.UserId);
            }
            player.SetRole(plugin.Config.CIRole, true);
            player.Health = plugin.Config.PlayerMaxHP;
            player.MaxHealth = plugin.Config.PlayerMaxHP;
            player.Position = spawn.Transform.position + Vector3.up * 1.5f;
            player.ClearInventory();
            player.Broadcast(10, plugin.Config.CISpawnText);
            if (!bombSpawned)
            {
                bombSpawned = true;
                player.AddItem(ItemType.KeycardChaosInsurgency);
            }
            foreach (var item in plugin.Config.CIItems)
            {
                player.AddItem(item);
            }
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato556, 500);
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato762, 500);
            player.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, 500);
        }

        private void SetupBombSites()
        {
            bombsite = new List<Room>();
            foreach (var site in plugin.Config.BombsiteSpawn)
            {
                bombsite.Add(Map.Rooms.First(p => p.Name.Contains(site)));
            }
        }

        private IEnumerator<float> RoundTimer()
        {
            CI = Player.List.Where(p => p.Role == plugin.Config.CIRole).Count();
            NTF = Player.List.Where(p => p.Role == plugin.Config.NTFRole).Count();
            while (true)
            {
                DisplayHintAll(FormatInfo(), 1f, null);
                yield return Timing.WaitForSeconds(1f);
                timeLeft -= 1f;
                if (bombPlanted)
                {
                    bombTimer -= 1f;
                }
                CI = Player.List.Where(p => p.Role == plugin.Config.CIRole).Count();
                NTF = Player.List.Where(p => p.Role == plugin.Config.NTFRole).Count();
                /*if (bomb != null && bombPlanted)
                {
                    PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CurClass = RoleType.Tutorial;
                    PlayerManager.localPlayer.GetComponent<FallDamage>().RpcDoSound(bomb.position, 999f);
                }*/
            }
        }

        public string FormatInfo()
        {
            return plugin.Config.RoundInfoText.Replace("%roundtimer%", timeLeft.ToString()).Replace("%ntf%", NTF.ToString()).Replace("%ci%", CI.ToString()).Replace("%bombplanted%", FormatBombInfo());
        }

        public void PlayerDoorInteract(InteractingDoorEventArgs ev)
        {
            if (plugin.canBuy)
            {
                ev.IsAllowed = false;
                return;
            }
            if (ev.Door.CheckpointDoor && ev.Door.DoorName.Contains("ENT"))
            {
                ev.IsAllowed = false;
                return;
            }
            ev.IsAllowed = true;
        }

        public void PlayerPickupItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup == bomb || ev.Pickup.info.itemId == ItemType.KeycardChaosInsurgency)
            {
                if (bombPlanted)
                {
                    ev.IsAllowed = false;
                    if (diffuser != null && (diffuser.ReferenceHub == ev.Player.ReferenceHub || diffuser.Role == RoleType.Spectator))
                    {
                        Timing.KillCoroutines(plantHandle);
                        diffuser.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(0);
                        diffuser = null;
                        diffusing = false;
                    }
                    else
                    {
                        plantHandle = Timing.RunCoroutine(DiffuseBomb(ev.Player));
                    }
                }
                else if (ev.Player.Role == plugin.Config.NTFRole)
                {
                    ev.IsAllowed = false;
                }
            }
        }

        private IEnumerator<float> DiffuseBomb(Player player)
        {
            //if (player.Inventory.items.FindIndex(p => p.id == ItemType.Disarmer) != -1)
            if (player.Role == plugin.Config.NTFRole)
            {
                player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(1);
                diffusing = true;
                diffuser = player;
                yield return Timing.WaitForSeconds(plugin.Config.DiffuseTime);
                if (player.Role == plugin.Config.NTFRole)
                {
                    player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(0);
                    bombDiffused = true;
                    bombPlanted = false;
                    diffusing = false;
                    diffuser = null;
                    AddMoney(plugin.Config.BombInteractMoney, player);
                }
            }
            Timing.KillCoroutines(plantHandle);
            plantHandle = new CoroutineHandle();
        }

        public string FormatBombInfo()
        {
            if (bombPlanted)
                return plugin.Config.BombInfoText.Replace("%bombtimer%", bombTimer.ToString());
            return "";
        }

        public ReferenceHub SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z)
        {
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
            CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
            if (ccm == null)
                Log.Error("CCM is null, doufus. You need to do this the harder way.");
            ccm.CurClass = role;
            //ccm.RefreshPlyModel(role);
            obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
            obj.GetComponent<QueryProcessor>().PlayerId = 9999;
            obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
            ccm._hub.playerStats.Health = 100000f;
            obj.transform.localScale = new Vector3(x, y, z);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            NetworkServer.Spawn(obj);
            return ccm._hub;
        }

        public Room GetClosestBombSite(Vector3 pos, float maxdist)
        {
            Room r = null;
            float dist = maxdist;
            foreach (var room in bombsite)
            {
                if (Vector3.Distance(pos, room.Transform.position) < dist)
                {
                    r = room;
                    dist = Vector3.Distance(pos, room.Transform.position);
                }
            }
            return r;
        }

        public void PlayerDropItem(DroppingItemEventArgs ev)
        {
            if (ev.Item.id == ItemType.KeycardChaosInsurgency)
            {
                if (GetClosestBombSite(ev.Player.Position, plugin.Config.DistanceFromSite) != null)
                {
                    ev.IsAllowed = false;
                    {
                        Timing.KillCoroutines(plantHandle);
                        plantHandle = Timing.RunCoroutine(PlantBomb(ev.Player));
                    }
                }
            }
        }

        public void PlayerDied(DiedEventArgs ev)
        {
            if (ev.Killer != null && ev.Killer != ev.Target)
            {
                AddMoney(plugin.Config.KillMoney, ev.Killer);
            }
        }

        private IEnumerator<float> PlantBomb(Player player)
        {
            if (GetClosestBombSite(player.Position, plugin.Config.DistanceFromSite) == null)
            {
                Timing.KillCoroutines(plantHandle);
                plantHandle = new CoroutineHandle();
                yield break;
            }
            player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(1);
            yield return Timing.WaitForSeconds(plugin.Config.PlantTime);
            if (player.Role == plugin.Config.CIRole && player.Inventory.items.FindIndex(p => p.id == ItemType.KeycardChaosInsurgency) != -1 && GetClosestBombSite(player.Position, plugin.Config.DistanceFromSite) != null)
            {
                player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<Ensnared>(0);
                bombTimer = plugin.Config.BombTimer;
                player.Inventory.items.RemoveAt(player.Inventory.items.FindIndex(p => p.id == ItemType.KeycardChaosInsurgency));
                bomb = Pickup.Inv.SetPickup(ItemType.KeycardChaosInsurgency, 0f, player.Position + Vector3.up * -1f, Quaternion.identity, 0, 0, 0);
                bomb.GetComponent<Rigidbody>().isKinematic = true;
                //bomb = ItemType.KeycardChaosInsurgency.Spawn(0f, player.Position + Vector3.up * 1.5f);
                //bombdummy = SpawnDummyModel(bomb.position, bomb.rotation, RoleType.Scp096, 0.1f, 0.1f, 0.1f);
                timeLeft += bombTimer;
                bombPlanted = true;
                Map.Broadcast(5, plugin.Config.BombPlantText);
                AddMoney(plugin.Config.BombInteractMoney, player);
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