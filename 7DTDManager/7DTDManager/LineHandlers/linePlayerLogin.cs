﻿using _7DTDManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _7DTDManager.LineHandlers
{
    public class linePlayerLogin : BaseLineHandler
    {
        static Regex rgPlayerJoin = new Regex(".*Player connected, .*entityid=(?<entityid>[0-9]+), name=(?<name>.*), steamid=(?<steamid>[0-9]+),.*");

        public override  bool ProcessLine(IServerConnection serverConnection, string currentLine)
        {
            if (rgPlayerJoin.IsMatch(currentLine))
            {
                Match match = rgPlayerJoin.Match(currentLine);
                GroupCollection groups = match.Groups;

                IPlayer p = serverConnection.AllPlayers.AddPlayer(groups["name"].Value, groups["steamid"].Value, groups["entityid"].Value);
                p.Login();
                serverConnection.Execute("lp");
                return true;
            }
            return false;
        }

        

    }
}
