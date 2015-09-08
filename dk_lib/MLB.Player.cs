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
            public string Team { get; set; }
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
                string[] teampos = namepos[1].Replace(")", "").Split('-');
                string[] pos = teampos[1].Trim().Split('/');
                res.Team = teampos[0].Trim();
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
            public override string ToString()
            {
                return this.Name + " (" + this.Team + ")";
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
                List<Player> subs = players
                    .Where(x => x.Salary + tSalary <= maxSalary)
                    .Where(x =>
                        ((this.P1 == null || this.P2 == null) && (x.Pos & Position.P) != Position.None) ||
                        ((this.C == null) && (x.Pos & Position.C) != Position.None) ||
                        ((this.B1 == null) && (x.Pos & Position.B1) != Position.None) ||
                        ((this.B2 == null) && (x.Pos & Position.B2) != Position.None) ||
                        ((this.B3 == null) && (x.Pos & Position.B3) != Position.None) ||
                        ((this.SS == null) && (x.Pos & Position.SS) != Position.None) ||
                        ((this.OF1 == null || this.OF2 == null || this.OF3 == null) && (x.Pos & Position.OF) != Position.None)
                    ).ToList();
                if (this.P1 == null)
                {
                    optLineup.P1 = subs.Where(x => (x.Pos & Position.P) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.P1 != null) subs.Remove(optLineup.P1);
                }
                if (this.P2 == null)
                {
                    optLineup.P2 = subs.Where(x => (x.Pos & Position.P) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.P2 != null) subs.Remove(optLineup.P2);
                }
                if (this.C == null)
                {
                    optLineup.C = subs.Where(x => (x.Pos & Position.C) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.C != null) subs.Remove(optLineup.C);
                }
                if (this.B1 == null)
                {
                    optLineup.B1 = subs.Where(x => (x.Pos & Position.B1) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.B1 != null) subs.Remove(optLineup.B1);
                }
                if (this.B2 == null)
                {
                    optLineup.B2 = subs.Where(x => (x.Pos & Position.B2) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.B2 != null) subs.Remove(optLineup.B2);
                }
                if (this.B3 == null)
                {
                    optLineup.B3 = subs.Where(x => (x.Pos & Position.B3) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.B3 != null) subs.Remove(optLineup.B3);
                }
                if (this.SS == null)
                {
                    optLineup.SS = subs.Where(x => (x.Pos & Position.SS) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.SS != null) subs.Remove(optLineup.SS);
                }
                if (this.OF1 == null)
                {
                    optLineup.OF1 = subs.Where(x => (x.Pos & Position.OF) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.OF1 != null) subs.Remove(optLineup.OF1);
                }
                if (this.OF2 == null)
                {
                    optLineup.OF2 = subs.Where(x => (x.Pos & Position.OF) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.OF2 != null) subs.Remove(optLineup.OF2);
                }
                if (this.OF3 == null)
                {
                    optLineup.OF3 = subs.Where(x => (x.Pos & Position.OF) != Position.None).OrderBy(x => x.Salary).ThenByDescending(x => x.Projection).FirstOrDefault();
                    if (optLineup.OF3 != null) subs.Remove(optLineup.OF3);
                }
                while (subs.Count() > 0)
                {
                    // Keep only players whose projection is greater than or equal to a player they could replace
                    // And their salary allows them to replace said player
                    double oSalary = optLineup.Salary;
                    subs = subs
                        .Where(x =>
                            (this.P1 == null && (x.Pos & Position.P) != Position.None && oSalary + x.Salary - optLineup.P1.Salary <= maxSalary && x.Projection >= optLineup.P1.Projection) ||
                            (this.P2 == null && (x.Pos & Position.P) != Position.None && oSalary + x.Salary - optLineup.P2.Salary <= maxSalary && x.Projection >= optLineup.P2.Projection) ||
                            (this.C == null && (x.Pos & Position.C) != Position.None && oSalary + x.Salary - optLineup.C.Salary <= maxSalary && x.Projection >= optLineup.C.Projection) ||
                            (this.B1 == null && (x.Pos & Position.B1) != Position.None && oSalary + x.Salary - optLineup.B1.Salary <= maxSalary && x.Projection >= optLineup.B1.Projection) ||
                            (this.B2 == null && (x.Pos & Position.B2) != Position.None && oSalary + x.Salary - optLineup.B2.Salary <= maxSalary && x.Projection >= optLineup.B2.Projection) ||
                            (this.B3 == null && (x.Pos & Position.B3) != Position.None && oSalary + x.Salary - optLineup.B3.Salary <= maxSalary && x.Projection >= optLineup.B3.Projection) ||
                            (this.SS == null && (x.Pos & Position.SS) != Position.None && oSalary + x.Salary - optLineup.SS.Salary <= maxSalary && x.Projection >= optLineup.SS.Projection) ||
                            (this.OF1 == null && (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF1.Salary <= maxSalary && x.Projection >= optLineup.OF1.Projection) ||
                            (this.OF2 == null && (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF2.Salary <= maxSalary && x.Projection >= optLineup.OF2.Projection) ||
                            (this.OF3 == null && (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF3.Salary <= maxSalary && x.Projection >= optLineup.OF3.Projection)
                        ).ToList();
                    // Determine best option to replace a player
                    double bestScore = -1;
                    string strPosition = null;
                    Player sub = null;
                    if (this.P1 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.P) != Position.None && oSalary + x.Salary - optLineup.P1.Salary <= maxSalary).OrderByDescending(x => x - optLineup.P1).FirstOrDefault();
                        if (p != null && p - optLineup.P1 > bestScore)
                        {
                            strPosition = "P1";
                            bestScore = p - optLineup.P1;
                            sub = p;
                        }
                    }
                    if (this.P2 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.P) != Position.None && oSalary + x.Salary - optLineup.P2.Salary <= maxSalary).OrderByDescending(x => x - optLineup.P2).FirstOrDefault();
                        if (p != null && p - optLineup.P2 > bestScore)
                        {
                            strPosition = "P2";
                            bestScore = p - optLineup.P2;
                            sub = p;
                        }
                    }
                    if (this.C == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.C) != Position.None && oSalary + x.Salary - optLineup.C.Salary <= maxSalary).OrderByDescending(x => x - optLineup.C).FirstOrDefault();
                        if (p != null && p - optLineup.C > bestScore)
                        {
                            strPosition = "C";
                            bestScore = p - optLineup.C;
                            sub = p;
                        }
                    }
                    if (this.B1 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.B1) != Position.None && oSalary + x.Salary - optLineup.B1.Salary <= maxSalary).OrderByDescending(x => x - optLineup.B1).FirstOrDefault();
                        if (p != null && p - optLineup.B1 > bestScore)
                        {
                            strPosition = "B1";
                            bestScore = p - optLineup.B1;
                            sub = p;
                        }
                    }
                    if (this.B2 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.B2) != Position.None && oSalary + x.Salary - optLineup.B2.Salary <= maxSalary).OrderByDescending(x => x - optLineup.B2).FirstOrDefault();
                        if (p != null && p - optLineup.B2 > bestScore)
                        {
                            strPosition = "B2";
                            bestScore = p - optLineup.B2;
                            sub = p;
                        }
                    }
                    if (this.B3 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.B3) != Position.None && oSalary + x.Salary - optLineup.B3.Salary <= maxSalary).OrderByDescending(x => x - optLineup.B3).FirstOrDefault();
                        if (p != null && p - optLineup.B3 > bestScore)
                        {
                            strPosition = "B3";
                            bestScore = p - optLineup.B3;
                            sub = p;
                        }
                    }
                    if (this.SS == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.SS) != Position.None && oSalary + x.Salary - optLineup.SS.Salary <= maxSalary).OrderByDescending(x => x - optLineup.SS).FirstOrDefault();
                        if (p != null && p - optLineup.SS > bestScore)
                        {
                            strPosition = "SS";
                            bestScore = p - optLineup.SS;
                            sub = p;
                        }
                    }
                    if (this.OF1 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF1.Salary <= maxSalary).OrderByDescending(x => x - optLineup.OF1).FirstOrDefault();
                        if (p != null && p - optLineup.OF1 > bestScore)
                        {
                            strPosition = "OF1";
                            bestScore = p - optLineup.OF1;
                            sub = p;
                        }
                    }
                    if (this.OF2 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF2.Salary <= maxSalary).OrderByDescending(x => x - optLineup.OF2).FirstOrDefault();
                        if (p != null && p - optLineup.OF2 > bestScore)
                        {
                            strPosition = "OF2";
                            bestScore = p - optLineup.OF2;
                            sub = p;
                        }
                    }
                    if (this.OF3 == null)
                    {
                        Player p = subs.Where(x => (x.Pos & Position.OF) != Position.None && oSalary + x.Salary - optLineup.OF3.Salary <= maxSalary).OrderByDescending(x => x - optLineup.OF3).FirstOrDefault();
                        if (p != null && p - optLineup.OF3 > bestScore)
                        {
                            strPosition = "OF3";
                            bestScore = p - optLineup.OF3;
                            sub = p;
                        }
                    }
                    if (sub == null)
                    {
                        subs.Clear();
                    }
                    else
                    {
                        switch (strPosition)
                        {
                            case "P1":
                                subs.Add(optLineup.P1);
                                optLineup.P1 = sub;
                                break;
                            case "P2":
                                subs.Add(optLineup.P2);
                                optLineup.P2 = sub;
                                break;
                            case "C":
                                subs.Add(optLineup.C);
                                optLineup.C = sub;
                                break;
                            case "B1":
                                subs.Add(optLineup.B1);
                                optLineup.B1 = sub;
                                break;
                            case "B2":
                                subs.Add(optLineup.B2);
                                optLineup.B2 = sub;
                                break;
                            case "B3":
                                subs.Add(optLineup.B3);
                                optLineup.B3 = sub;
                                break;
                            case "SS":
                                subs.Add(optLineup.SS);
                                optLineup.SS = sub;
                                break;
                            case "OF1":
                                subs.Add(optLineup.OF1);
                                optLineup.OF1 = sub;
                                break;
                            case "OF2":
                                subs.Add(optLineup.OF2);
                                optLineup.OF2 = sub;
                                break;
                            case "OF3":
                                subs.Add(optLineup.OF3);
                                optLineup.OF3 = sub;
                                break;
                        }
                    }
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
