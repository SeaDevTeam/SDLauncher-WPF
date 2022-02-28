using System;
using System.Collections.Generic;
using System.Text;
using MineStatLib;

namespace Turtlz_Launcher
{
    public class MCStatusEx
    {
        public string Motd { get; set; }
        public string Version { get; set; }
        public string MaxPlayers { get; set; }
        public string Players { get; set; }
        public string Latency { get; set; }
        public bool? Online { get; set; }
        public static MineStat mscplex;
        public MCStatusEx(string ip, int port)
        {
            Online = null;
            var th = new System.Threading.Thread(() =>
            {
                mscplex = new MineStat(ip, (ushort)port);
                if (mscplex.ServerUp)
                {
                    Online = true;
                    Motd = mscplex.Motd;
                    Version = mscplex.Version;
                    MaxPlayers = mscplex.MaximumPlayers;
                    Players = mscplex.CurrentPlayers;
                    Latency = mscplex.Latency.ToString();
                }
                else
                {
                    Online = false;
                }
            });
            th.Start();
        }
    }
}
