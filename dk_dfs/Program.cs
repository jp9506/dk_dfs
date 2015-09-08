using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk_dfs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            { }
            else if (args[0].ToLower() == "mlb")
            {
                IEnumerable<dk.MLB.Player> players = dk.MLB.GetData();
                if (args[1].ToLower() == "/s")
                    players = players.Where(x => x.Starter);
                dk.MLB.Lineup opt = new dk.MLB.Lineup();
                if (opt.Optimize(players, 50000))
                {
                    Console.WriteLine(opt.ForOutput);
                }
            } else if (args[0].ToLower() == "nfl")
            {
                IEnumerable<dk.NFL.Player> players = dk.NFL.GetData();
                dk.NFL.Lineup opt = new dk.NFL.Lineup();
                if (opt.Optimize(players, 50000))
                {
                    Console.WriteLine(opt.ForOutput);
                }
            }
        }
    }
}
