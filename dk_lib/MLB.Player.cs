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
            None = 0,
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
            public bool Starter { get; set; }
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
                Player res = new Player();
                string[] data = pString.Split('\t');
                string[] namepos = data[0].Split('(');
                res.Name = namepos[0].Trim();
                string[] pos = namepos[1].Replace(")", "").Split('-')[1].Trim().Split('/');
                res.Pos = Position.None;
                foreach (string p in pos)
                {
                    switch (p.ToUpper().Trim())
                    {
                        case "P": res.Pos |= Position.P; break;
                        case "C": res.Pos |= Position.C; break;
                        case "1B":res.Pos |= Position.B1; break;
                        case "2B":res.Pos |= Position.B2; break;
                        case "3B":res.Pos |= Position.B3; break;
                        case "SS":res.Pos |= Position.SS; break;
                        case "OF":res.Pos |= Position.OF; break;
                    }
                }
                res.Salary = double.Parse(data[data.Length - 3].Replace("$", ""));
                res.Projection = double.Parse(data[data.Length - 4].Replace(" pts", ""));
                if (data.Length == 14)
                    res.Starter = (res.Projection > 0);
                else
                    res.Starter = (data[2] != "-");
                return res;
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
                    return this.ToArray().Where(x => x != null).Sum(x => x.Salary);
                }
            }
            public double Projection
            {
                get
                {
                    return this.ToArray().Where(x => x != null).Sum(x => x.Projection);
                }
            }
            public Player[] ToArray()
            {
                return new Player[] { P1, P2, C, B1, B2, B3, SS, OF1, OF2, OF3 };
            }
            public bool HasNull()
            {
                return this.ToArray().Any(x => x == null);
            }
            public Lineup Copy()
            {
                return new Lineup()
                {
                    P1 = this.P1,
                    P2 = this.P2,
                    C = this.C,
                    B1 = this.B1,
                    B2 = this.B2,
                    B3 = this.B3,
                    SS = this.SS,
                    OF1 = this.OF1,
                    OF2 = this.OF2,
                    OF3 = this.OF3
                };
            }
            public bool Optimize(IEnumerable<Player> players, double maxSalary)
            {
                if (this.Salary > maxSalary) return false;
                if (!this.HasNull()) return true;
                Lineup optLineup = this.Copy();
                double tSalary = this.Salary;
                List<Player> subs = new List<Player>(players
                    .Where(x => x.Salary + tSalary <= maxSalary)
                    .Where(x =>
                        ((this.P1 == null || this.P2 == null) && (x.Pos & Position.P) != Position.None) ||
                        ((this.C == null) && (x.Pos & Position.C) != Position.None) ||
                        ((this.B1 == null) && (x.Pos & Position.B1) != Position.None) ||
                        ((this.B2 == null) && (x.Pos & Position.B2) != Position.None) ||
                        ((this.B3 == null) && (x.Pos & Position.B3) != Position.None) ||
                        ((this.SS == null) && (x.Pos & Position.SS) != Position.None) ||
                        ((this.OF1 == null || this.OF2 == null || this.OF3 == null) && (x.Pos & Position.OF) != Position.None)
                    ));

                while (subs.Count > 0)
                {

                }
                if (!optLineup.HasNull())
                {
                    this.P1 = optLineup.P1;
                    this.P2 = optLineup.P2;
                    this.C = optLineup.C;
                    this.B1 = optLineup.B1;
                    this.B2 = optLineup.B2;
                    this.B3 = optLineup.B3;
                    this.SS = optLineup.SS;
                    this.OF1 = optLineup.OF1;
                    this.OF2 = optLineup.OF2;
                    this.OF3 = optLineup.OF3;
                    return true;
                }
                else
                    return false;
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
                resp.Skip(6)
                    .Select(x => Player.Parse(x))
                    .Where(x => (x != null && (int)x.Pos < 2 * (int)p))
                );
            return res;
        }
    }
}
