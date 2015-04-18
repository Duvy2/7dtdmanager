﻿using _7DTDManager.Interfaces;
using _7DTDManager.Interfaces.Commands;
using _7DTDManager.Objects;
using _7DTDManager.ShopSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _7DTDManager.Commands
{
    public class cmdShop : AdminCommandBase
    {
        public cmdShop()
        {
            CommandHelp = "Shop edit functions";
            CommandName = "shop";
            CommandLevel = 100;
            CommandArgs = 2;
            CommandUsage = "/shop [create|remove|additem|removeitem] <args>";
        }

        Regex rgAddItem = new Regex("(?<shopid>[0-9]+) (?<itemname>[a-z0-9]+) (?<itemcount>[0-9]+) (?<restockcount>[0-9]+) (?<restockdelay>[0-9]+) (?<maxstock>[0-9]+) (?<minlevel>[0-9]+) (?<cost>[0-9]+)");

        public override bool Execute(IServerConnection server, IPlayer p, params string[] args)
        {
            string subCmd = args[1];
            string restCmd = String.Join(" ", args, 2, args.Length - 2);
            switch (subCmd)
            {
                case "create":
                    {
                        Shop newShop = new Shop { ShopName = restCmd, ShopPosition = new AreaDefiniton(p.CurrentPosition) };

                        int nShopID = Program.Config.RegisterShop(newShop);
                        Program.Config.Save();
                        newShop.Init();
                        p.Message("New shop '{0}' has been created. ID {1}", restCmd, nShopID);
                        return true;
                    }
                case "remove":
                    {
                        if (restCmd == "all")
                        {
                            Program.Config.Shops.Clear();
                            Program.Config.Save();
                            p.Message("All shops deleted.");
                            return true;
                        }
                    }
                    break;
                case "additem":
                    {
                        if (rgAddItem.IsMatch(restCmd))
                        {
                            Match match = rgAddItem.Match(restCmd);
                            GroupCollection groups = match.Groups;

                            int shopID = Convert.ToInt32(groups["shopid"].Value);
                            Shop shop = (from s in Program.Config.Shops where s.ShopID == shopID select s).FirstOrDefault();
                            if ( shop == null)
                            {
                                p.Error("Shop with ID {0} not found.", shopID);
                                return true;
                            }

                            bool newItem = false;
                            ShopItem item = (from items in shop.ShopItems where items.ItemName.ToLowerInvariant() == groups["itemname"].Value select items).FirstOrDefault();
                            if (item == null)
                            {
                                item = new ShopItem();
                                newItem = true;
                            }
                            // "(?<shopid>[0-9]+) (?<itemname>[a-z0-9]+) (?<itemcount>[0-9]+) 
                            // (?<restockcount>[0-9]+) (?<restockdelay>[0-9]+) (?<maxstock>[0-9]+) (?<minlevel>[0-9]+) (?<cost>[0-9]+)");
                            item.ItemName = groups["itemname"].Value;
                            item.StockAmount = Convert.ToInt32(groups["itemcount"].Value);
                            item.RestockAmount = Convert.ToInt32(groups["restockcount"].Value);
                            item.RestockDelay = Convert.ToInt32(groups["restockdelay"].Value);
                            item.MaxStock = Convert.ToInt32(groups["maxstock"].Value);
                            item.LevelRequired = Convert.ToInt32(groups["minlevel"].Value);
                            item.SellPrice = Convert.ToInt32(groups["cost"].Value);
                            if ( newItem )
                            {
                                shop.RegisterItem(item);
                            }
                            Program.Config.Save();
                            p.Message("{0} {1} have been added to '{2}'", item.StockAmount, item.ItemName, shop.ShopName);
                            return true;
                        }

                    }
                    break;
                case "removeitem":
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}