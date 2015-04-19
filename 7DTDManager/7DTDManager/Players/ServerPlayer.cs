﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTDManager.Players
{
    class ServerPlayer : Player
    {
        public override void Message(string p, params object[] args)
        {
            Console.WriteLine(String.Format(p, args));
        }

        public override int AdminLevel
        {
            get
            {
                return 100;
            }
        }

        public override bool IsAdmin
        {
            get
            {
                return true;
            }
        }
    }
}
