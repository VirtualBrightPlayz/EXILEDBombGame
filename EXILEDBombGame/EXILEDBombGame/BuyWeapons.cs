using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXILEDBombGame
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class BuyWeapons : ICommand
    {
        public string Command => "buy";

        public string[] Aliases => new string[0];

        public string Description => "Buy weapons and items";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender)
            {
                PlayerCommandSender plr = ((PlayerCommandSender)sender);
                var args = arguments.Array;
                if (args.Length == 2 && int.TryParse(args[1], out int idx))
                {
                    if (PluginMain.instance.canBuy)
                    {
                        if (plr.CCM.CurClass == PluginMain.instance.Config.NTFRole)
                        {
                            var item = PluginMain.instance.Config.NTFBuyables.ElementAt(idx);
                            if (!PluginMain.money.ContainsKey(plr.CCM.UserId))
                            {
                                PluginMain.money.Add(plr.CCM.UserId, PluginMain.instance.Config.StartMoney);
                            }
                            if (PluginMain.money[plr.CCM.UserId] >= item.Price)
                            {
                                if (item.Sight == -1 || item.Barrel == -1 || item.Other == -1)
                                {
                                    plr.RH.inventory.AddNewItem(item.Item, s: item.Sight, b: item.Barrel, o: item.Other);
                                }
                                else
                                {
                                    plr.RH.inventory.AddNewItem(item.Item, dur: new Item(plr.RH.inventory.GetItemByID(item.Item)).durability, s: item.Sight, b: item.Barrel, o: item.Other);
                                }
                                PluginMain.money[plr.CCM.UserId] -= item.Price;
                                response = PluginMain.instance.Config.BuyTimeCompleteText;
                                return true;
                            }
                            else
                            {
                                response = PluginMain.instance.Config.BuyTimeFailText;
                                return true;
                            }
                        }
                        else if (plr.CCM.CurClass == PluginMain.instance.Config.CIRole)
                        {
                            var item = PluginMain.instance.Config.CIBuyables.ElementAt(idx);
                            if (!PluginMain.money.ContainsKey(plr.CCM.UserId))
                            {
                                PluginMain.money.Add(plr.CCM.UserId, PluginMain.instance.Config.StartMoney);
                            }
                            if (PluginMain.money[plr.CCM.UserId] >= item.Price)
                            {
                                if (item.Sight == -1 || item.Barrel == -1 || item.Other == -1)
                                {
                                    plr.RH.inventory.AddNewItem(item.Item, s: item.Sight, b: item.Barrel, o: item.Other);
                                }
                                else
                                {
                                    plr.RH.inventory.AddNewItem(item.Item, dur: new Item(plr.RH.inventory.GetItemByID(item.Item)).durability, s: item.Sight, b: item.Barrel, o: item.Other);
                                }
                                PluginMain.money[plr.CCM.UserId] -= item.Price;
                                response = PluginMain.instance.Config.BuyTimeCompleteText;
                                return true;
                            }
                            else
                            {
                                response = PluginMain.instance.Config.BuyTimeFailText;
                                return true;
                            }
                        }
                        else
                        {
                            response = PluginMain.instance.Config.BuyTimeFailText;
                            return true;
                        }
                    }
                    else
                    {
                        response = PluginMain.instance.Config.BuyTimeExpireText;
                        return true;
                    }
                }
                else
                {
                    if (!PluginMain.money.ContainsKey(plr.CCM.UserId))
                    {
                        PluginMain.money.Add(plr.CCM.UserId, PluginMain.instance.Config.StartMoney);
                    }
                    response = PluginMain.instance.Config.BuyTimeTitleText.Replace("%money%", PluginMain.money[plr.CCM.UserId].ToString());
                    int i = 0;
                    if (plr.CCM.CurClass == PluginMain.instance.Config.CIRole)
                    {
                        foreach (var item in PluginMain.instance.Config.CIBuyables)
                        {
                            response += i.ToString() + " - " + item.DisplayName + " - $" + item.Price.ToString() + "\n";
                            i++;
                        }
                    }
                    else if (plr.CCM.CurClass == PluginMain.instance.Config.NTFRole)
                    {
                        foreach (var item in PluginMain.instance.Config.NTFBuyables)
                        {
                            response += i.ToString() + " - " + item.DisplayName + " - $" + item.Price.ToString() + "\n";
                            i++;
                        }
                    }
                    return true;
                }
            }
            response = "";
            return false;
        }
    }
}
