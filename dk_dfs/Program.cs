using System;
using System.Collections.Generic;
using System.Linq;

namespace dk_dfs
{
    class Program
    {
        private enum Game
        {
            None,
            NFL,
            MLB
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            { }
            else
            {
                Game g = Game.None;
                bool startersOnly = false;
                DateTime gametimeMin = DateTime.MinValue;
                DateTime gametimeMax = DateTime.MaxValue;
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToUpper())
                    {
                        case "MLB": g = Game.MLB; break;
                        case "NFL": g = Game.NFL; break;
                        case "/S": startersOnly = true; break;
                        case "/MIN": gametimeMin = DateTime.Parse(args[i + 1]); i++; break;
                        case "/MAX": gametimeMax = DateTime.Parse(args[i + 1]); i++; break;
                    }
                }
                if (g == Game.MLB)
                {
                    IEnumerable<dk.MLB.Player> players = dk.MLB.GetData();
                    if (startersOnly) players = players.Where(x => x.Starter);
                    players = players.Where(x => (x.GameTime >= gametimeMin && x.GameTime <= gametimeMax));
                    dk.MLB.Lineup opt = new dk.MLB.Lineup();
                    if (opt.Optimize(players, 50000))
                    {
                        Console.WriteLine(opt.ForOutput);
                    }
                } else if (g == Game.NFL)
                {
                    IEnumerable<dk.NFL.Player> players = dk.NFL.GetData();
                    players = players.Where(x => (x.GameTime >= gametimeMin && x.GameTime <= gametimeMax));
                    dk.NFL.Lineup opt = new dk.NFL.Lineup();
                    if (opt.Optimize(players, 50000))
                    {
                        Console.WriteLine(opt.ForOutput);
                    }
                }
            }
        }
    }
}
