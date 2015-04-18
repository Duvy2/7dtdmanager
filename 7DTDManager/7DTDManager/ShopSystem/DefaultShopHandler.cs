﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTDManager.ShopSystem
{
    public class DefaultShopHandler : ShopHandler
    {
        public DefaultShopHandler()
        {
            HandlerName = "DefaultShopHandler";
        }

        public override void ItemBought(Interfaces.IServerConnection server, Interfaces.IPlayer buyer, ShopItem item, int amount)
        {
            server.Execute("give {0} {1} {2}", buyer.EntityID, item.ItemName, amount);
            buyer.Message("Your items have been placed next to you.");
        }

        public override void ItemSold(Interfaces.IServerConnection server, Interfaces.IPlayer seller, ShopItem item, int amount)
        {
            throw new NotImplementedException();
        }

        public override int EvaluateBuy(Interfaces.IServerConnection server, Interfaces.IPlayer buyer, ShopItem item, int amount)
        {
            //TODO: SKillcheck
            return item.SellPrice * amount;
        }

        public override int EvaluateSell(Interfaces.IServerConnection server, Interfaces.IPlayer seller, ShopItem item, int amount)
        {
            throw new NotImplementedException();
        }
    }
}