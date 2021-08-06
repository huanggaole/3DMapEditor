using System;
using System.Collections.Generic;
using System.Text;

namespace RoomGenerator
{
    class OuterSkeleton
    {
        public enum LineType { wall, window, door, empty };
        public struct Point
        {
            public int x, y;
            public LineType type;
            public Point(int x, int y, LineType type)
            {
                this.x = x;
                this.y = y;
                this.type = type;
            }
            public int Dist(Point b)
            {
                return (int)Math.Sqrt((this.x - b.x) * (this.x - b.x) + (this.y - b.y) * (this.y - b.y));
            }
        }

        public class Cell
        {
            public int x1, x2, y1, y2; // delete public after debug
            public int window;
            public Requirement req;
            public int rmid;
            public int[] nbh;
            public Cell(int x1, int x2, int y1, int y2)
            {
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
                this.window = 0;
                this.req = null;
                nbh = new int[4] { -1, -1, -1, -1 };
                rmid = -1;
            }
            public Point Midpoint()
            {
                return new Point((x1 + x2) / 2, (y1 + y2) / 2, LineType.empty);
            }
            public int Dist(Cell o)
            {
                return this.Midpoint().Dist(o.Midpoint());
            }
        }

        public class Fur
        {
            public FurType type;
            public int x, y;
            public int o; //oreintation
            public Fur(FurType type, int x, int y, int o)
            {
                this.type = type;
                this.x = x;
                this.y = y;
                this.o = o;
            }
        }

        Point[] poly; //list of polygon
        int len, temp;
        List<Cell> cell;
        int[,] belong;
        List<int> seed;
        int door_dist;
        int minx, miny, maxx, maxy;
        int[,] walk;
        int[,] a; //final map
        Random rd = new Random();
        List<Fur> fur;

        public void Init(int x, int y, int len)  //the first Point(location of the door) and the number of points(edges)
        {
            poly = new Point[len + 1];
            poly[0] = new Point(x, y, LineType.door);
            this.len = len;
            this.temp = 1;
        }

        public void Add(int x, int y, LineType type)  //a new Point and line type between last Point and this Point
        {
            poly[temp] = new Point(x, y, type);
            temp++;
            if (temp == len)
            {
                poly[temp] = poly[0];
            }
        }

        bool Inside(Cell cell) //determine whether a cell is inside the skeleton
        {
            int num = 0;
            Point mid = cell.Midpoint();
            for (int i = 0; i < len; i++)
                if (poly[i].y == poly[i + 1].y)
                {
                    int mn = Math.Min(poly[i].x, poly[i + 1].x);
                    int mx = Math.Max(poly[i].x, poly[i + 1].x);
                    if (mn <= mid.x && mx > mid.x && poly[i].y > mid.y)
                        num++;
                }
            return (num % 2) == 1;
        }

        /*
        static public void GetGrids_old(OuterSkeleton skel, int tw) //skel, cell size threhold tw
        {
            // ----------- get x & y cell separate lines
            List<int> xaxisO = new List<int>();
            List<int> yaxisO = new List<int>();
            for (int i = 0; i < skel.len; i++)
            {
                if (skel.poly[i].x == skel.poly[i + 1].x)
                    xaxisO.Add(skel.poly[i].x);
                if (skel.poly[i].y == skel.poly[i + 1].y)
                    yaxisO.Add(skel.poly[i].y);
                if (skel.poly[i].type == skel.poly[i + 1].type)
                {
                    if (skel.poly[i].type == OuterSkeleton.LineType.window)
                    {
                        if (skel.poly[i].x == skel.poly[i + 1].x)
                            yaxisO.Add((skel.poly[i].y + skel.poly[i + 1].y) / 2);
                        if (skel.poly[i].y == skel.poly[i + 1].y)
                            xaxisO.Add((skel.poly[i].x + skel.poly[i + 1].x) / 2);
                    }
                    if (skel.poly[i].type == OuterSkeleton.LineType.wall)
                    {
                        if (skel.poly[i].x == skel.poly[i + 1].x)
                        {
                            int mn = Math.Min(skel.poly[i].y, skel.poly[i + 1].y);
                            int mx = Math.Max(skel.poly[i].y, skel.poly[i + 1].y);
                            for (int j = mn + tw; j + tw * 0.7 <= mx; j += tw)
                            {
                                yaxisO.Add(j);
                            }
                        }
                        if (skel.poly[i].y == skel.poly[i + 1].y)
                        {
                            yaxisO.Add(skel.poly[i].y);
                            int mn = Math.Min(skel.poly[i].x, skel.poly[i + 1].x);
                            int mx = Math.Max(skel.poly[i].x, skel.poly[i + 1].x);
                            for (int j = mn + tw; j + tw * 0.7 <= mx; j += tw)
                            {
                                xaxisO.Add(j);
                            }
                        }
                    }
                }
            }
            List<int> xaxis = new List<int>();
            List<int> yaxis = new List<int>();
            xaxisO.Sort();
            foreach (int i in xaxisO)
            {
                if (xaxis.Exists(x => x == i) == false)
                    xaxis.Add(i);
            }
            yaxisO.Sort();
            foreach (int i in yaxisO)
            {
                if (yaxis.Exists(x => x == i) == false)
                    yaxis.Add(i);
            }
            // ----------- get list of cells
            skel.cell = new List<Cell>();
            for (int i = 0; i < xaxis.Count-1; i++)
            {
                for (int j = 0; j < yaxis.Count-1; j++)
                {
                    Cell no = new Cell(xaxis[i], xaxis[i + 1], yaxis[j], yaxis[j + 1]);
                    if (skel.Inside(no)) skel.cell.Add(no);
                }
            }
        }
        */

        public int GetGrids(int tw = 4, int td = 6) //cell size threhold tw, seed cell distance threhold td
        {
            // ----------- get x & y cell separate lines
            List<int> xaxisO = new List<int>();
            List<int> yaxisO = new List<int>();
            minx = int.MaxValue;
            miny = int.MaxValue;
            maxx = -1;
            maxy = -1;
            for (int i = 0; i < len; i++)
            {
                minx = Math.Min(minx, poly[i].x);
                maxx = Math.Max(maxx, poly[i].x);
                miny = Math.Min(miny, poly[i].y);
                maxy = Math.Max(maxy, poly[i].y);
                if (poly[i].x == poly[i + 1].x)
                    xaxisO.Add(poly[i].x);
                if (poly[i].y == poly[i + 1].y)
                    yaxisO.Add(poly[i].y);
            }
            for (int i = minx; i <= maxx; i += tw) xaxisO.Add(i);
            for (int i = miny; i <= maxy; i += tw) yaxisO.Add(i);
            List<int> xaxis = new List<int>();
            List<int> yaxis = new List<int>();
            xaxisO.Sort();
            foreach (int i in xaxisO)
            {
                if (xaxis.Exists(x => x == i) == false)
                    xaxis.Add(i);
            }
            yaxisO.Sort();
            foreach (int i in yaxisO)
            {
                if (yaxis.Exists(x => x == i) == false)
                    yaxis.Add(i);
            }
            // ----------- get list of cells and prepare relateive parameters

            /*minx--;
            miny--;
            maxx++;
            maxy++;*/
            cell = new List<Cell>();
            for (int i = 0; i < xaxis.Count - 1; i++)
            {
                for (int j = 0; j < yaxis.Count - 1; j++)
                {
                    Cell no = new Cell(xaxis[i], xaxis[i + 1], yaxis[j], yaxis[j + 1]);
                    if (Inside(no)) cell.Add(no);
                }
            }
            belong = new int[maxx - minx + 1, maxy - miny + 1];
            for (int i = 0; i < maxx - minx + 1; i++)
                for (int j = 0; j < maxy - miny + 1; j++)
                    belong[i, j] = -1;
            for (int i = 0; i < cell.Count; i++)
            {
                for (int x = cell[i].x1; x <= cell[i].x2; x++)
                {
                    for (int y = cell[i].y1; y <= cell[i].y2; y++)
                    {
                        belong[x - minx, y - miny] = i;
                    }
                }
            }
            foreach (Point pt in poly)
            {
                if (pt.type == LineType.window) cell[belong[pt.x - minx, pt.y - miny]].window++;
            }

            for (int i = 0; i < cell.Count; i++)
            {
                Cell c = cell[i];
                if (c.x1 > minx) c.nbh[0] = belong[c.x1 - 1 - minx, c.y1 - miny];
                if (c.y2 < maxy) c.nbh[1] = belong[c.x1 - minx, c.y2 - miny];
                if (c.y1 > miny) c.nbh[3] = belong[c.x1 - minx, c.y1 - 1 - miny];
                if (c.x2 < maxx) c.nbh[2] = belong[c.x2 - minx, c.y1 - miny];
                if (c.nbh[1] == i) c.nbh[1] = -1;
                if (c.nbh[2] == i) c.nbh[2] = -1;
            }
            // ----------- select seeds
            seed = new List<int>();
            for (int i = 0; i < len; i++)
            {
                if (poly[i].type == LineType.window) seed.Add(belong[poly[i].x - minx, poly[i].y - miny]);
                if (poly[i].type == LineType.door) seed.Add(belong[poly[i].x - minx, poly[i].y - miny]);
            }
            while (true)
            {
                int id = -1, mdist = 0;
                for (int i = 0; i < cell.Count; i++)
                    if (seed.Exists(x => x == i) == false)
                    {
                        int dist = int.MaxValue;
                        for (int j = 0; j < seed.Count; j++)
                        {
                            dist = Math.Min(dist, cell[i].Dist(cell[seed[j]]));
                        }

                        if (dist > td && dist > mdist)
                        {
                            mdist = dist;
                            id = i;
                        }
                    }
                if (id == -1) break;
                seed.Add(id);
            }
            door_dist = 0;
            for (int i = 0; i < seed.Count; i++)
            {
                door_dist = Math.Max(door_dist, cell[belong[poly[0].x - minx, poly[0].y - miny]].Dist(cell[seed[i]]));
            }
            return seed.Count;
        }

        public int Select(Requirement req, List<Requirement> req2, int th = 6) //current requirement, all requirement, distance therhold
        {
            float[] posibility = new float[seed.Count];
            for (int i = 0; i < seed.Count; i++)
            {
                float p = 1;
                int x = seed[i];
                if (cell[x].req != null)
                {
                    posibility[i] = -1;
                    continue;
                }
                if (cell[x].window == 0)
                {
                    if (req.window) p = 0;
                    else p *= 0.5f;
                }
                if (req.type == RoomType.social)
                {
                    p *= 1 - (float)cell[belong[poly[0].x - minx, poly[0].y - miny]].Dist(cell[x]) / door_dist;
                }
                if (req.type == RoomType.personal)
                {
                    p *= (float)cell[belong[poly[0].x - minx, poly[0].y - miny]].Dist(cell[x]) / door_dist;
                }
                for (int j = 0; j < req.simmilar.Count; j++)
                {
                    if (req2[req.simmilar[j]].cell > 0)
                        p *= (float)Math.Pow(0.5, cell[x].Dist(cell[req2[req.simmilar[j]].cell]));
                }
                if (req.connect >= 0 && req2[req.connect].cell > 0)
                {
                    int dis = cell[x].Dist(cell[req2[req.connect].cell]);
                    if (dis <= th) p *= (float)Math.Pow(0.5, dis);
                    else p = 0;
                }
                if (x == belong[poly[0].x - minx, poly[0].y - miny])
                {
                    if (req.type != RoomType.social) p = 0;
                }
                posibility[i] = p;
            }

            float sum = 0;
            for (int i = 0; i < seed.Count; i++) if (posibility[i] >= 0) sum += posibility[i];
            if (sum < 0.000001) return -1;

            sum *= (float)rd.NextDouble();
            for (int i = 0; i < seed.Count; i++)
                if (posibility[i] > 0)
                {
                    sum -= posibility[i];
                    if (sum < 0.000001) return i;
                }
            return -1;
        }

        public void Put(int x, Requirement req, int id)
        {
            cell[seed[x]].req = req;
            cell[seed[x]].rmid = id;
        }

        bool Expand_valid(int x, int fx)
        {
            int fx1 = (fx + 1) % 4;
            while (cell[x].nbh[fx1] != -1 && cell[cell[x].nbh[fx1]].rmid == cell[x].rmid)
            {
                x = cell[x].nbh[fx1];
            }
            fx1 = (fx + 3) % 4;
            while (cell[x].nbh[fx1] != -1 && cell[cell[x].nbh[fx1]].rmid == cell[x].rmid)
            {
                if (cell[x].nbh[fx] == -1) return false;
                if (cell[cell[x].nbh[fx]].rmid != -1) return false;
                x = cell[x].nbh[fx1];
            }
            if (cell[x].nbh[fx] == -1) return false;
            if (cell[cell[x].nbh[fx]].rmid != -1) return false;
            return true;
        }

        int[] Roomsize(int x) //find size of cell x
        {
            int[] a = new int[2];
            int fx = 0;
            for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) ;
            fx = 3;
            for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) ;
            fx = 1;
            for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) a[0]++;
            fx = 2;
            for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) a[1]++;
            return a;
        }
        public int Expand(Requirement req, bool repet = false)
        {
            int failN = 0;
            while (failN < 5)
            {
                int fx = rd.Next(4);
                int x = seed[req.cell];
                while (cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid) x = cell[x].nbh[fx];
                if (!Expand_valid(x, fx))
                {
                    failN++;
                    continue;
                }
                failN = 0;

                int fx1 = (fx + 1) % 4;
                for (; cell[x].nbh[fx1] != -1 && cell[cell[x].nbh[fx1]].rmid == cell[x].rmid; x = cell[x].nbh[fx1]) ;
                fx1 = (fx + 3) % 4;
                for (; cell[x].nbh[fx1] != -1 && cell[cell[x].nbh[fx1]].rmid == cell[x].rmid; x = cell[x].nbh[fx1])
                {
                    cell[cell[x].nbh[fx]].rmid = cell[x].rmid;
                    cell[cell[x].nbh[fx]].req = cell[x].req;
                }
                cell[cell[x].nbh[fx]].rmid = cell[x].rmid;
                cell[cell[x].nbh[fx]].req = cell[x].req;
                if (repet) break;
                int[] size = Roomsize(x);
                if (size[0] >= req.width && size[1] >= req.height) break;
            }
            if (failN < 5) return 1;
            else return 0;
        }

        public void Mergecell()
        {
            List<Cell> cellN = new List<Cell>();
            List<int> flag = new List<int>();
            for (int i = 0; i < cell.Count; i++)
            {
                if (flag.Exists(x => x == i))
                {
                    continue;
                }
                int x = i, x1, x2, y1, y2, y;
                int fx = 0;
                for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) ;
                fx = 3;
                for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) ;
                x1 = cell[x].x1;
                y1 = cell[x].y1;
                fx = 1;
                for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx])
                {
                    for (y = x; cell[y].nbh[2] != -1 && cell[cell[y].nbh[2]].rmid == cell[x].rmid; y = cell[y].nbh[2]) flag.Add(y);
                    flag.Add(y);
                }
                for (y = x; cell[y].nbh[2] != -1 && cell[cell[y].nbh[2]].rmid == cell[x].rmid; y = cell[y].nbh[2]) flag.Add(y);
                flag.Add(y);
                fx = 2;
                for (; cell[x].nbh[fx] != -1 && cell[cell[x].nbh[fx]].rmid == cell[x].rmid; x = cell[x].nbh[fx]) ;
                x2 = cell[x].x2;
                y2 = cell[x].y2;
                Cell no = new Cell(x1, x2, y1, y2);
                no.req = cell[x].req;
                no.rmid = cell[x].rmid;
                cellN.Add(no);
            }
            cell = cellN;
        }

        public bool Fulfill(List<Requirement> req)
        {
            int num = 0;
            foreach (Cell c in cell)
            {
                if (c.rmid == -1) continue;
                if (c.x2 - c.x1 >= c.req.height && c.y2 - c.y1 >= c.req.width) num++;
            }
            if (num == req.Count) return true;
            return false;
        }

        void WalkDfs(int x, int y)
        {
            if (belong[x, y] == -1) return;
            if (walk[x, y] == temp) return;
            walk[x, y] = temp;
            if (x > 0 && (a[x, y] & 2) == 0) WalkDfs(x - 1, y);
            if (y > 0 && (a[x, y] & 1) == 0) WalkDfs(x, y - 1);
            if (x < maxx - minx && (a[x + 1, y] & 2) == 0) WalkDfs(x + 1, y);
            if (y < maxy - miny && (a[x, y + 1] & 1) == 0) WalkDfs(x, y + 1);
        }
        public void DoorInit()
        {
            walk = new int[maxx - minx + 1, maxy - miny + 1];
            a = new int[maxx - minx + 1, maxy - miny + 1];
            for (int i = 0; i < maxx - minx + 1; i++)
                for (int j = 0; j < maxy - miny + 1; j++)
                {
                    belong[i, j] = -1;
                    a[i, j] = walk[i, j] = 0;
                }
            for (int i = 0; i < cell.Count; i++)
            {
                for (int x = cell[i].x1; x < cell[i].x2; x++)
                {
                    for (int y = cell[i].y1; y < cell[i].y2; y++)
                    {
                        belong[x - minx, y - miny] = i;
                    }
                }
                for (int j = cell[i].x1; j < cell[i].x2; j++)
                {
                    a[j - minx, cell[i].y1 - miny] |= 1;
                    a[j - minx, cell[i].y2 - miny] |= 1;
                }
                for (int j = cell[i].y1; j < cell[i].y2; j++)
                {
                    a[cell[i].x1 - minx, j - miny] |= 2;
                    a[cell[i].x2 - minx, j - miny] |= 2;
                }
            }
            a[poly[0].x - minx, poly[0].y - miny] <<= 2;
            temp = 1;
            WalkDfs(poly[0].x - minx, poly[0].y - miny);
        }

        bool ChokePoint(int x, int y)
        {
            if (x == 0 || belong[x - 1, y] < 0) return false;
            if (y == 0 || belong[x, y - 1] < 0) return false;
            if (walk[x, y] > 0)
            {
                if (x > 0 && walk[x - 1, y] == 0) return true;
                if (y > 0 && walk[x, y - 1] == 0) return true;
            }
            else
            {
                if (x > 0 && walk[x - 1, y] > 0) return true;
                if (y > 0 && walk[x, y - 1] > 0) return true;
            }
            return false;
        }

        public bool Door()
        {
            List<int> alter = new List<int>();

            for (int i = 0; i < maxx - minx + 1; i++)
            {
                for (int j = 0; j < maxy - miny + 1; j++)
                {
                    if (belong[i, j] < 0) continue;
                    if (a[i, j] > 0 && ChokePoint(i, j))
                    {
                        alter.Add(i);
                        alter.Add(j);
                    }
                }
            }
            if (alter.Count == 0) return false;
            int k = rd.Next(alter.Count);
            if ((k ^ 1) < k) k ^= 1;
            a[alter[k], alter[k + 1]] <<= 2;
            temp++;
            WalkDfs(alter[k], alter[k + 1]);
            return true;
        }
        public void PutFur()
        {
            fur = new List<Fur>();
            foreach (Cell c in cell)
            {
                if (c.req == null) return;
                if (c.req.rname == RoomName.livingroom)
                {
                    int fx = rd.Next(4);
                    if (fx == 0)
                    {
                        fur.Add(new Fur(FurType.Sofa, c.x1 + 1, (c.y1 + c.y2) / 2, 2));
                        fur.Add(new Fur(FurType.Table, (c.x1 + c.x2) / 2 - 1, (c.y1 + c.y2) / 2, 0));
                        fur.Add(new Fur(FurType.TV, c.x2 - 2, (c.y1 + c.y2) / 2, 0));
                    }
                    if (fx == 2)
                    {
                        fur.Add(new Fur(FurType.Sofa, c.x2 - 2, (c.y1 + c.y2) / 2, 0));
                        fur.Add(new Fur(FurType.Table, (c.x1 + c.x2) / 2 - 1, (c.y1 + c.y2) / 2, 0));
                        fur.Add(new Fur(FurType.TV, c.x1 + 1, (c.y1 + c.y2) / 2, 2));
                    }
                    if (fx == 1)
                    {
                        fur.Add(new Fur(FurType.Sofa, (c.x1 + c.x2) / 2, c.y2 - 2, 3));
                        fur.Add(new Fur(FurType.Table, (c.x1 + c.x2) / 2, (c.y1 + c.y2) / 2 - 1, 1));
                        fur.Add(new Fur(FurType.TV, (c.x1 + c.x2) / 2, c.y1 + 1, 1));
                    }
                    if (fx == 3)
                    {
                        fur.Add(new Fur(FurType.Sofa, (c.x1 + c.x2) / 2, c.y1 + 1, 1));
                        fur.Add(new Fur(FurType.Table, (c.x1 + c.x2) / 2, (c.y1 + c.y2) / 2 - 1, 1));
                        fur.Add(new Fur(FurType.TV, (c.x1 + c.x2) / 2, c.y2 - 2, 3));
                    }
                }
                if (c.req.rname == RoomName.diningroom)
                {
                    fur.Add(new Fur(FurType.DiningTable, (c.x1 + c.x2) / 2, (c.y1 + c.y2) / 2, 0));
                }
                if (c.req.rname == RoomName.kitchen)
                {
                    int fx = rd.Next(4);
                    if (fx == 0)
                    {
                        int y = c.y1 + rd.Next(c.y2 - c.y1 - 1);
                        fur.Add(new Fur(FurType.Frozer, c.x1 + 1, y, 2));
                        fur.Add(new Fur(FurType.Cooker, c.x1 + 1, y + 1, 2));
                    }
                    if (fx == 1)
                    {
                        int x = c.x1 + rd.Next(c.x2 - c.x1 - 1);
                        fur.Add(new Fur(FurType.Frozer, x, c.y2 - 2, 3));
                        fur.Add(new Fur(FurType.Cooker, x + 1, c.y2 - 2, 3));
                    }
                    if (fx == 2)
                    {
                        int y = c.y1 + rd.Next(c.y2 - c.y1 - 1);
                        fur.Add(new Fur(FurType.Frozer, c.x2 - 2, y, 0));
                        fur.Add(new Fur(FurType.Cooker, c.x2 - 2, y + 1, 0));
                    }
                    if (fx == 3)
                    {
                        int x = c.x1 + rd.Next(c.x2 - c.x1 - 1);
                        fur.Add(new Fur(FurType.Frozer, x, c.y1 + 1, 1));
                        fur.Add(new Fur(FurType.Cooker, x + 1, c.y1 + 1, 1));
                    }
                }
                if (c.req.rname == RoomName.toliet)
                {
                    int x = (c.x1 + c.x2) / 2, y = (c.y1 + c.y2) / 2;
                    if (c.x2 - c.x1 > c.y2 - c.y1)
                    {
                        fur.Add(new Fur(FurType.Toilet, x, y, 0));
                        fur.Add(new Fur(FurType.Washer, x + 1, y, 2));
                    }
                    else
                    {
                        fur.Add(new Fur(FurType.Toilet, x, y, 1));
                        fur.Add(new Fur(FurType.Washer, x, y + 1, 3));
                    }
                }
                if (c.req.rname == RoomName.bedroom)
                {
                    int fx = rd.Next(4);
                    if (fx == 0)
                    {
                        fur.Add(new Fur(FurType.DoubleBed, c.x1 + 1, (c.y1 + c.y2) / 2 - 1, 2));
                        fur.Add(new Fur(FurType.SmallContainer, c.x1 + 1, (c.y1 + c.y2) / 2 - 2, 2));
                    }
                    if (fx == 2)
                    {
                        fur.Add(new Fur(FurType.DoubleBed, c.x2 - 2, (c.y1 + c.y2) / 2 - 1, 0));
                        fur.Add(new Fur(FurType.SmallContainer, c.x2 - 2, (c.y1 + c.y2) / 2 - 2, 0));
                    }
                    if (fx == 1)
                    {
                        fur.Add(new Fur(FurType.DoubleBed, (c.x1 + c.x2) / 2 - 1, c.y2 - 2, 3));
                        fur.Add(new Fur(FurType.SmallContainer, (c.x1 + c.x2) / 2 - 2, c.y2 - 2, 3));
                    }
                    if (fx == 3)
                    {
                        fur.Add(new Fur(FurType.DoubleBed, (c.x1 + c.x2) / 2 - 1, c.y1 + 1, 1));
                        fur.Add(new Fur(FurType.SmallContainer, (c.x1 + c.x2) / 2 - 2, c.y1 + 1, 1));
                    }
                }
            }
        }

        public MapObjectData ConvertInit(int he)
        {
            MapObjectData obj = new MapObjectData();
            for (int i = 0; i < maxx - minx + 1; i++)
            {
                for (int j = 0; j < maxy - miny + 1; j++)
                {
                    if (a[i, j] == 12)
                    {
                        int num = 0;
                        if (i == 0 || belong[i - 1, j] == -1) num++;
                        if (j == 0 || belong[i, j - 1] == -1) num++;
                        if (num == 2)
                        {
                            a[i, j] = 1024;
                            obj.staircase = new int[2] { minx + i - 1, miny + j };
                        }
                    }
                }
            }


            foreach (Point p in poly)
            {
                if (p.type == LineType.window)
                {
                    a[p.x - minx, p.y - miny] <<= 4;
                }
            }
            obj.voxel_direction = new int[maxx - minx + 1, maxy - miny + 1, he];
            obj.voxel_types = new VoxelType[maxx - minx + 1, maxy - miny + 1, he];

            obj.fur_direction = new int[maxx - minx + 1, maxy - miny + 1, he];
            obj.fur_types = new FurType[maxx - minx + 1, maxy - miny + 1, he];
            return obj;
        }

        public MapObjectData Convert(int he, MapObjectData obj)
        {
            for (int i = 0; i < maxx - minx + 1; i++)
            {
                for (int j = 0; j < maxy - miny + 1; j++)
                {
                    if (belong[i, j] < 0)
                    {
                        if ((a[i, j] & 3) > 0) obj.voxel_types[i, j, he] = VoxelType.WALL;
                        if ((a[i, j] & 12) > 0) obj.voxel_types[i, j, he] = VoxelType.DOOR;
                        if ((a[i, j] & 48) > 0) obj.voxel_types[i, j, he] = VoxelType.WINDOW;
                        obj.voxel_direction[i, j, he] = -a[i, j];
                        if ((a[i, j] & 12) > 0) obj.voxel_direction[i, j, he] >>= 2;
                        if ((a[i, j] & 48) > 0) obj.voxel_direction[i, j, he] >>= 4;
                    }
                    else
                    {
                        if (a[i, j] == 0) obj.voxel_types[i, j, he] = VoxelType.FLOOR;
                        if ((a[i, j] & 3) > 0) obj.voxel_types[i, j, he] = VoxelType.WALL;
                        if ((a[i, j] & 12) > 0) obj.voxel_types[i, j, he] = VoxelType.DOOR;
                        if ((a[i, j] & 48) > 0) obj.voxel_types[i, j, he] = VoxelType.WINDOW;
                        obj.voxel_direction[i, j, he] = a[i, j];
                        if ((a[i, j] & 12) > 0) obj.voxel_direction[i, j, he] >>= 2;
                        if ((a[i, j] & 48) > 0) obj.voxel_direction[i, j, he] >>= 4;
                    }
                }
            }
            foreach (Fur f in fur)
            {
                obj.fur_types[f.x - minx, f.y - miny, he] = f.type;
                obj.fur_direction[f.x - minx, f.y - miny, he] = f.o;
            }
            return obj;
        }

        public void Write_debuger(List<Requirement> req = null)
        {
            for (int i = 0; i < len; i++)
            {
                Console.WriteLine("{0} {1} {2}", poly[i].x, poly[i].y, poly[i].type);
            }
            int n = maxx - minx + 1;
            Console.WriteLine("[");
            for (int i = 0; i < maxx - minx + 1; i++)
            {
                Console.Write("[");
                for (int j = 0; j < maxy - miny; j++)
                {
                    Console.Write("{0},", a[i, j]);
                }
                Console.WriteLine("{0}],", a[i, maxy - miny - 1]);
            }
            Console.WriteLine("]");
            /*for (int i = 0; i < req.Count; i++)
            {
                Console.Write("{0} ", req[i].cell);
            }
            Console.WriteLine();
            /*
            n += 10;
            char[,] b = new char[n, n];
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    b[i, j] = ' ';
                }
            }
            foreach (Cell c in cell)
            {
                for (int j = c.x1; j <= c.x2; j++)
                {
                    b[j, c.y1] = b[j, c.y2] = '|';
                }
                for (int j = c.y1; j <= c.y2; j++)
                {
                    b[c.x1, j] = b[c.x2, j] = '-';
                }
                for (int j = c.x1 + 1; j < c.x2; j++)
                {
                    for (int k = c.y1 + 1; k < c.y2; k++)
                    {
                        b[j, k] = c.rmid==-1?' ':(char)(c.rmid+'0');
                    }
                }
            }
            /*foreach(int x in seed)
            {
                for (int j = cell[x].x1+1; j < cell[x].x2; j++)
                {
                    for (int k = cell[x].y1 + 1; k < cell[x].y2; k++)
                    {
                        a[j, k] = '8';
                    }
                }
            }
            
            for(int i=0;i<req.Count;i++)
            {
                int x = seed[req[i].cell];
                if (x == -1) continue;
                for (int j = cell[x].x1 + 1; j < cell[x].x2; j++)
                {
                    for (int k = cell[x].y1 + 1; k < cell[x].y2; k++)
                    {
                        a[j, k] = (char)(i+'0');
                    }
                }
            }*/

            /*for (int i=0;i<len;i++)
            {
                if(poly[i].type==LineType.window)
                    b[poly[i].x, poly[i].y] = '9';
                if (poly[i].type == LineType.door)
                    b[poly[i].x, poly[i].y] = '0';
            }
            for(int i=0;i<n;i++)
            {
                for(int j=0;j<n;j++)
                {
                    Console.Write(b[i, j]);
                }
                Console.WriteLine();
            }*/
        }
    }
}
