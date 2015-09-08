using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk
{
    public class NFL
    {
        public const string URL = "http://www.fantasypros.com/nfl/draftkings-cheatsheet.php?export=xls&position=";
        public enum Position
        {
            None = 0,
            QB = 1,
            RB = 2,
            WR = 4,
            TE = 8,
            DST = 16,
            FLEX = RB | WR | TE
        };
        public class Player
        {
            public Position Pos { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public double Projection { get; set; }
            public string Team { get; set; }
            public DateTime GameTime { get; set; }
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
                string[] teampos = namepos[1].Replace(")", "").Split('-');
                string[] pos = teampos[1].Trim().Split('/');
                res.Team = teampos[0].Trim();
                res.Pos = Position.None;
                foreach (string p in pos)
                {
                    switch (p.ToUpper().Trim())
                    {
                        case "QB": res.Pos |= Position.QB; break;
                        case "RB": res.Pos |= Position.RB; break;
                        case "WR":res.Pos |= Position.WR; break;
                        case "TE":res.Pos |= Position.TE; break;
                        case "DST":res.Pos |= Position.DST; break;
                    }
                }
                string[] daytime = data[data.Length - 13].Split(' ');
                DateTime lastTues = DateTime.Today.AddDays(2 - (int)DateTime.Today.DayOfWeek);
                switch (daytime[0].ToLower())
                {
                    case "sun": res.GameTime = DateTime.Parse(lastTues.AddDays(5).ToShortDateString() + " " + daytime[1]); break;
                    case "mon": res.GameTime = DateTime.Parse(lastTues.AddDays(6).ToShortDateString() + " " + daytime[1]); break;
                    case "tue": res.GameTime = DateTime.Parse(lastTues.AddDays(0).ToShortDateString() + " " + daytime[1]); break;
                    case "wed": res.GameTime = DateTime.Parse(lastTues.AddDays(1).ToShortDateString() + " " + daytime[1]); break;
                    case "thu": res.GameTime = DateTime.Parse(lastTues.AddDays(2).ToShortDateString() + " " + daytime[1]); break;
                    case "fri": res.GameTime = DateTime.Parse(lastTues.AddDays(3).ToShortDateString() + " " + daytime[1]); break;
                    case "sat": res.GameTime = DateTime.Parse(lastTues.AddDays(4).ToShortDateString() + " " + daytime[1]); break;
                }
                res.Salary = double.Parse(data[data.Length - 3].Replace("$", ""));
                res.Projection = double.Parse(data[data.Length - 4].Replace(" pts", ""));
                return res;
            }
            public override string ToString()
            {
                return this.Name + " (" + this.Team + ")";
            }
        }
        public class Lineup
        {
            public Player QB { get; set; }
            public Player RB1 { get; set; }
            public Player RB2 { get; set; }
            public Player WR1 { get; set; }
            public Player WR2 { get; set; }
            public Player WR3 { get; set; }
            public Player TE { get; set; }
            public Player FLEX { get; set; }
            public Player DST { get; set; }
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
            public string ForOutput
            {
                get
                {
                    return
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "QB", QB.ToString(), QB.Projection, QB.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "RB", RB1.ToString(), RB1.Projection, RB1.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "RB", RB2.ToString(), RB2.Projection, RB2.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "WR", WR1.ToString(), WR1.Projection, WR1.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "WR", WR2.ToString(), WR2.Projection, WR2.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "WR", WR3.ToString(), WR3.Projection, WR3.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "TE", TE.ToString(), TE.Projection, TE.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "F", FLEX.ToString(), FLEX.Projection, FLEX.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "D", DST.ToString(), DST.Projection, DST.Salary) +
                        String.Format("{0,-3}{1,-40}{2,6:N2}{3,10:C0}\n", "", "", this.Projection, this.Salary);
                }
            }
            public Player[] ToArray()
            {
                return new Player[] { QB, RB1, RB2, WR1, WR2, WR3, TE, FLEX, DST };
            }
            public bool HasNull()
            {
                return this.ToArray().Any(x => x == null);
            }
            public Lineup Copy()
            {
                return new Lineup()
                {
                    QB = this.QB,
                    RB1 = this.RB1,
                    RB2 = this.RB2,
                    WR1 = this.WR1,
                    WR2 = this.WR2,
                    WR3 = this.WR3,
                    TE = this.TE,
                    FLEX = this.FLEX,
                    DST = this.DST
                };
            }
            public bool Optimize(IEnumerable<Player> players, double maxSalary)
            {
                if (this.Salary > maxSalary) return false;
                if (!this.HasNull()) return true;
                Lineup optLineup = this.Copy();
                double tSalary = this.Salary;
                List<Player> subs = players
                    .Where(x => x.Salary + tSalary <= maxSalary)
                    .Where(x =>
                        ((this.QB == null ) && (x.Pos & Position.QB) != Position.None) ||
                        ((this.RB1 == null || this.RB2 == null) && (x.Pos & Position.RB) != Position.None) ||
                        ((this.WR1 == null || this.WR2 == null || this.WR3 == null) && (x.Pos & Position.WR) != Position.None) ||
                        ((this.TE == null) && (x.Pos & Position.TE) != Position.None) ||
                        ((this.FLEX == null) && (x.Pos & Position.FLEX) != Position.None) ||
                        ((this.DST == null) && (x.Pos & Position.DST) != Position.None)
                    ).ToList();
                if (this.QB == null)
                {
                    optLineup.QB = subs.Where(x => (x.Pos & Position.QB) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.QB != null) subs.Remove(optLineup.QB);
                }
                if (this.RB1 == null)
                {
                    optLineup.RB1 = subs.Where(x => (x.Pos & Position.RB) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.RB1 != null) subs.Remove(optLineup.RB1);
                }
                if (this.RB2 == null)
                {
                    optLineup.RB2 = subs.Where(x => (x.Pos & Position.RB) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.RB2 != null) subs.Remove(optLineup.RB2);
                }
                if (this.WR1 == null)
                {
                    optLineup.WR1 = subs.Where(x => (x.Pos & Position.WR) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.WR1 != null) subs.Remove(optLineup.WR1);
                }
                if (this.WR2 == null)
                {
                    optLineup.WR2 = subs.Where(x => (x.Pos & Position.WR) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.WR2 != null) subs.Remove(optLineup.WR2);
                }
                if (this.WR3 == null)
                {
                    optLineup.WR3 = subs.Where(x => (x.Pos & Position.WR) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.WR3 != null) subs.Remove(optLineup.WR3);
                }
                if (this.TE == null)
                {
                    optLineup.TE = subs.Where(x => (x.Pos & Position.TE) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.TE != null) subs.Remove(optLineup.TE);
                }
                if (this.FLEX == null)
                {
                    optLineup.FLEX = subs.Where(x => (x.Pos & Position.FLEX) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.FLEX != null) subs.Remove(optLineup.FLEX);
                }
                if (this.DST == null)
                {
                    optLineup.DST = subs.Where(x => (x.Pos & Position.DST) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.DST != null) subs.Remove(optLineup.DST);
                }
                if (optLineup.HasNull()) return false;
                while (subs.Count() > 0)
                {
                    // Keep only players whose projection is greater than or equal to a player they could replace
                    // And their salary allows them to replace said player
                    double oSalary = optLineup.Salary;
                    subs = subs
                        .Where(x =>
                            (this.QB == null && (x.Pos & Position.QB) != Position.None && oSalary + x.Salary - optLineup.QB.Salary <= maxSalary && x.Projection >= optLineup.QB.Projection) ||
                            (this.RB1 == null && (x.Pos & Position.RB) != Position.None && oSalary + x.Salary - optLineup.RB1.Salary <= maxSalary && x.Projection >= optLineup.RB1.Projection) ||
                            (this.RB2 == null && (x.Pos & Position.RB) != Position.None && oSalary + x.Salary - optLineup.RB2.Salary <= maxSalary && x.Projection >= optLineup.RB2.Projection) ||
                            (this.WR1 == null && (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR1.Salary <= maxSalary && x.Projection >= optLineup.WR1.Projection) ||
                            (this.WR2 == null && (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR2.Salary <= maxSalary && x.Projection >= optLineup.WR2.Projection) ||
                            (this.WR3 == null && (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR3.Salary <= maxSalary && x.Projection >= optLineup.WR3.Projection) ||
                            (this.TE == null && (x.Pos & Position.TE) != Position.None && oSalary + x.Salary - optLineup.TE.Salary <= maxSalary && x.Projection >= optLineup.TE.Projection) ||
                            (this.FLEX == null && (x.Pos & Position.FLEX) != Position.None && oSalary + x.Salary - optLineup.FLEX.Salary <= maxSalary && x.Projection >= optLineup.FLEX.Projection) ||
                            (this.DST == null && (x.Pos & Position.DST) != Position.None && oSalary + x.Salary - optLineup.DST.Salary <= maxSalary && x.Projection >= optLineup.DST.Projection)
                        ).ToList();
                    // Determine best option to replace a player
                    double bestScore = -1;
                    string strPosition = null;
                    Player sub = null;
                    if (this.QB == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.QB) != Position.None && oSalary + x.Salary - optLineup.QB.Salary <= maxSalary).OrderByDescending(x => x - optLineup.QB).FirstOrDefault();
                        if (p != null && p - optLineup.QB > bestScore)
                        {
                            strPosition = "QB";
                            bestScore = p - optLineup.QB;
                            sub = p;
                        }
                    }
                    if (this.RB1 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.RB) != Position.None && oSalary + x.Salary - optLineup.RB1.Salary <= maxSalary).OrderByDescending(x => x - optLineup.RB1).FirstOrDefault();
                        if (p != null && p - optLineup.RB1 > bestScore)
                        {
                            strPosition = "RB1";
                            bestScore = p - optLineup.RB1;
                            sub = p;
                        }
                    }
                    if (this.RB2 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.RB) != Position.None && oSalary + x.Salary - optLineup.RB2.Salary <= maxSalary).OrderByDescending(x => x - optLineup.RB2).FirstOrDefault();
                        if (p != null && p - optLineup.RB2 > bestScore)
                        {
                            strPosition = "RB2";
                            bestScore = p - optLineup.RB2;
                            sub = p;
                        }
                    }
                    if (this.WR1 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR1.Salary <= maxSalary).OrderByDescending(x => x - optLineup.WR1).FirstOrDefault();
                        if (p != null && p - optLineup.WR1 > bestScore)
                        {
                            strPosition = "WR1";
                            bestScore = p - optLineup.WR1;
                            sub = p;
                        }
                    }
                    if (this.WR2 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR2.Salary <= maxSalary).OrderByDescending(x => x - optLineup.WR2).FirstOrDefault();
                        if (p != null && p - optLineup.WR2 > bestScore)
                        {
                            strPosition = "WR2";
                            bestScore = p - optLineup.WR2;
                            sub = p;
                        }
                    }
                    if (this.WR3 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.WR) != Position.None && oSalary + x.Salary - optLineup.WR3.Salary <= maxSalary).OrderByDescending(x => x - optLineup.WR3).FirstOrDefault();
                        if (p != null && p - optLineup.WR3 > bestScore)
                        {
                            strPosition = "WR3";
                            bestScore = p - optLineup.WR3;
                            sub = p;
                        }
                    }
                    if (this.TE == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.TE) != Position.None && oSalary + x.Salary - optLineup.TE.Salary <= maxSalary).OrderByDescending(x => x - optLineup.TE).FirstOrDefault();
                        if (p != null && p - optLineup.TE > bestScore)
                        {
                            strPosition = "TE";
                            bestScore = p - optLineup.TE;
                            sub = p;
                        }
                    }
                    if (this.FLEX == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.FLEX) != Position.None && oSalary + x.Salary - optLineup.FLEX.Salary <= maxSalary).OrderByDescending(x => x - optLineup.FLEX).FirstOrDefault();
                        if (p != null && p - optLineup.FLEX > bestScore)
                        {
                            strPosition = "FLEX";
                            bestScore = p - optLineup.FLEX;
                            sub = p;
                        }
                    }
                    if (this.DST == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.DST) != Position.None && oSalary + x.Salary - optLineup.DST.Salary <= maxSalary).OrderByDescending(x => x - optLineup.DST).FirstOrDefault();
                        if (p != null && p - optLineup.DST > bestScore)
                        {
                            strPosition = "DST";
                            bestScore = p - optLineup.DST;
                            sub = p;
                        }
                    }
                    if (sub == null)
                    {
                        subs.Clear();
                    }
                    else
                    {
                        subs.Remove(sub);
                        switch (strPosition)
                        {
                            case "QB":
                                subs.Add(optLineup.QB);
                                optLineup.QB = sub;
                                break;
                            case "RB1":
                                subs.Add(optLineup.RB1);
                                optLineup.RB1 = sub;
                                break;
                            case "RB2":
                                subs.Add(optLineup.RB2);
                                optLineup.RB2 = sub;
                                break;
                            case "WR1":
                                subs.Add(optLineup.WR1);
                                optLineup.WR1 = sub;
                                break;
                            case "WR2":
                                subs.Add(optLineup.WR2);
                                optLineup.WR2 = sub;
                                break;
                            case "WR3":
                                subs.Add(optLineup.WR3);
                                optLineup.WR3 = sub;
                                break;
                            case "TE":
                                subs.Add(optLineup.TE);
                                optLineup.TE = sub;
                                break;
                            case "FLEX":
                                subs.Add(optLineup.FLEX);
                                optLineup.FLEX = sub;
                                break;
                            case "DST":
                                subs.Add(optLineup.DST);
                                optLineup.DST = sub;
                                break;
                        }
                    }
                }
                if (!optLineup.HasNull())
                {
                    this.QB = optLineup.QB;
                    this.RB1 = optLineup.RB1;
                    this.RB2 = optLineup.RB2;
                    this.WR1 = optLineup.WR1;
                    this.WR2 = optLineup.WR2;
                    this.WR3 = optLineup.WR3;
                    this.TE = optLineup.TE;
                    this.FLEX = optLineup.FLEX;
                    this.DST = optLineup.DST;
                    return true;
                }
                else
                    return false;
            }

        }
		public static List<Player> GetData()
        {
            List<Player> res = new List<Player>();
            res.AddRange(GetData(Position.QB, "QB"));
            res.AddRange(GetData(Position.RB, "RB"));
            res.AddRange(GetData(Position.WR, "WR"));
            res.AddRange(GetData(Position.TE, "TE"));
            res.AddRange(GetData(Position.DST, "DST"));
            return res;
        }
		private static List<Player> GetData(Position p, string posString)
        {
            string url = URL + posString;
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
