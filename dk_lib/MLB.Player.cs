using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk
{
    public class MLB
    {
        public const string URL = "http://www.fantasypros.com/mlb/draftkings-cheatsheet.php?export=xls&position=";
        public enum Position
        {
            P = 1,
            C = 2,
            B1 = 4,
            B2 = 8,
            B3 = 16,
            SS = 32,
            OF = 64
        };
        public class Player
        {
            public Position Pos { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public double Projection { get; set; }
            public double Value
            {
                get
                {
                    return 1000 * Projection / Salary;
                }
            }
            public static double operator -(Player a, Player b)
            {
                if (a == null && b == null)
                    return 0;
                else if (b == null)
                    return a.Value;
                else if (a == null)
                    return 0;
                else if (a.Projection < b.Projection)
                    return 0;
                else if (a.Salary < b.Salary)
                    return double.MaxValue;
                else
                    return 1000 * (a.Projection - b.Projection) / (a.Salary - b.Salary);
            }
			public static Player Parse(string pString)
            {

            }
        }
        public class Lineup
        {
            public Player P1 { get; set; }
            public Player P2 { get; set; }
            public Player C { get; set; }
            public Player B1 { get; set; }
            public Player B2 { get; set; }
            public Player B3 { get; set; }
            public Player SS { get; set; }
            public Player OF1 { get; set; }
            public Player OF2 { get; set; }
            public Player OF3 { get; set; }
            public Double Salary
            {
                get
                {
                    return this.ToArray().Sum(x => x.Salary);
                }
            }
            public double Projection
            {
                get
                {
                    return this.ToArray().Sum(x => x.Projection);
                }
            }
            public Player[] ToArray()
            {
                return new Player[] { P1, P2, C, B1, B2, B3, SS, OF1, OF2, OF3 };
            }
        }
		public static List<Player> GetData()
        {
            List<Player> res = new List<Player>();
            res.AddRange(GetData(Position.P, "P"));
            res.AddRange(GetData(Position.C, "C"));
            res.AddRange(GetData(Position.B1, "1B"));
            res.AddRange(GetData(Position.B2, "2B"));
            res.AddRange(GetData(Position.B3, "3B"));
            res.AddRange(GetData(Position.SS, "SS"));
            res.AddRange(GetData(Position.OF, "OF"));
            return res;
        }
		private static List<Player> GetData(Position p, string posString)
        {
            string url = MLB.URL + posString;
            string[] resp = Browser.Browse(url);
            List<Player> res = new List<Player>(
                resp.Select(x => Player.Parse(x))
                    .Where(x => (x != null && (int)x.Pos < 2 * (int)p))
                );
            return res;
        }
    }
}
