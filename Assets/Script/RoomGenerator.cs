using System;
using System.Collections.Generic;
using System.Text;

namespace RoomGenerator
{
    public enum RoomType { personal, social, none };
    public enum RoomName { livingroom, diningroom, kitchen, toliet, bedroom };
    class Requirement
    {
        public bool window;
        public RoomType type;
        public int connect;
        public List<int> simmilar;
        public int cell;
        public int width, height;
        public RoomName rname;
        public Requirement(int width, int height, bool window, RoomType type, RoomName rname, int connect) //minimum size=width*height, window0/1, type, connect(-1: no connection requirement), 
        {
            this.height = height;
            this.width = width;
            this.window = window;
            this.type = type;
            this.connect = connect;
            this.rname = rname;
            this.cell = -1;
            this.simmilar = new List<int>();
        }
        public void Add(int x)
        {
            simmilar.Add(x);
        }
    }
    class RoomGenerator
    {
        static public int GenerateCell(OuterSkeleton skel) //input outer polygon, return maximum # of rooms
        {
            int num = skel.GetGrids(3, 5);
            return num;
        }
        static bool SelectSeed(OuterSkeleton skel, List<Requirement> req)
        {
            for (int i = 0; i < req.Count; i++)
            {
                int x = skel.Select(req[i], req, 6);
                if (x == -1)
                {
                    return true;
                }
                req[i].cell = x;
                skel.Put(x, req[i], i);
            }
            return false;
        }
        static public void FloorPlan(OuterSkeleton skel, List<Requirement> req)
        {
            while (SelectSeed(skel, req))
            {
                for (int i = 0; i < req.Count; i++)
                    if (req[i].cell >= 0)
                    {
                        skel.Put(req[i].cell, null, -1);
                        req[i].cell = -1;
                    }
            }
            for (int i = 0; i < req.Count; i++)
            {
                if (req[i].cell == -1) continue;
                skel.Expand(req[i]);
            }
            while (true)
            {
                int flag = 0;
                for (int i = 0; i < req.Count; i++)
                {
                    flag += skel.Expand(req[i], true);
                }
                if (flag == 0) break;
            }

        }

        static public MapObjectData ConvertToMapData(OuterSkeleton skel)
        {
            MapObjectData obj = skel.ConvertInit(1);
            return skel.Convert(0, obj);
        }

        static OuterSkeleton ConverToSkel(MapPCG.Building build, int scale = 2)
        {
            int minx = int.MaxValue, miny = int.MaxValue;
            for (int i = 0; i < build.points.Count; i++)
            {
                minx = Math.Min(minx, build.points[i].x);
                miny = Math.Min(miny, build.points[i].y);
            }
            OuterSkeleton skel = new OuterSkeleton();
            skel.Init(minx + (build.points[0].x - minx) * scale, miny + (build.points[0].y - miny) * scale, build.points.Count);
            for (int i = 1; i < build.points.Count; i++)
            {
                skel.Add(minx + (build.points[i].x - minx) * scale, miny + (build.points[i].y - miny) * scale,
                    build.type[i] == 0 ? OuterSkeleton.LineType.wall : OuterSkeleton.LineType.window);
            }
            return skel;
        }
        static List<Requirement> ReqSample()
        {
            List<Requirement> req = new List<Requirement>();
            Requirement r;
            r = new Requirement(5, 5, false, RoomType.social, RoomName.livingroom, -1);
            req.Add(r);
            r = new Requirement(4, 4, false, RoomType.social, RoomName.diningroom, 2);
            req.Add(r);
            r = new Requirement(4, 4, false, RoomType.none, RoomName.kitchen, 1);
            req.Add(r);
            r = new Requirement(3, 3, false, RoomType.none, RoomName.toliet, 0);
            req.Add(r);
            r = new Requirement(4, 4, true, RoomType.personal, RoomName.bedroom, -1);
            req.Add(r);
            // r = new Requirement(4, 4, true, RoomType.personal, RoomName.bedroom, -1);
            // r.Add(4);
            // req.Add(r);
            /*r = new Requirement(5, 5, true, RoomType.personal, -1);
            r.Add(3);
            req.Add(r);*/
            return req;
        }
        static List<Requirement> ReqSampleEasy()
        {
            List<Requirement> req = new List<Requirement>();
            Requirement r;
            r = new Requirement(4, 4, false, RoomType.social, RoomName.livingroom, -1);
            req.Add(r);
            r = new Requirement(4, 4, true, RoomType.personal, RoomName.bedroom, -1);
            req.Add(r);
            /*r = new Requirement(5, 5, true, RoomType.personal, -1);
            r.Add(3);
            req.Add(r);*/
            return req;
        }
        static public MapObjectData Generate(MapPCG.Building build, List<Requirement> req = null)
        {
            if (req == null) req = ReqSample();
            OuterSkeleton skel;
            int failN = 0, x = 1;
            while (true)
            {
                skel = ConverToSkel(build, 1);
                skel.GetGrids(4, 6);
                FloorPlan(skel, req);
                skel.Mergecell();
                if (skel.Fulfill(req)) break;
                failN++;
                if (failN > 10000) break;
            }
            if (failN > 10000)
            {
                req = ReqSampleEasy();
                failN = 0;
                x = 1;
                while (true)
                {
                    skel = ConverToSkel(build, 1);
                    skel.GetGrids(4, 6);
                    FloorPlan(skel, req);
                    skel.Mergecell();
                    if (skel.Fulfill(req)) break;
                    failN++;
                    if (failN > 1000) break;
                }
            }
            skel.DoorInit();
            while (skel.Door()) ;
            skel.PutFur();
            MapObjectData obj = ConvertToMapData(skel);

            skel.Write_debuger();
            obj.scale = new float[3] { (float)(1.0 / x), (float)(1.0 / x), 1f };
            return obj;
        }
    }
}