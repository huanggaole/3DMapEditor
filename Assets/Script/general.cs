using System;
using System.Collections;
using System.Collections.Generic;

namespace MapPCG
{
    public class Line
    {
        public Point begin;
        public Point end;
        public Line() { }
        public Line(Point _begin, Point _end)
        {
            begin = _begin;
            end = _end;
        }
    }

    public class Rectangle
    {
        public Point minP;
        public Point maxP;
        public Rectangle() { }
        public Rectangle(Point _minP, Point _maxP)
        {
            minP = _minP;
            maxP = _maxP;
        }
    }

    public class Polygon
    {
        public Point[] points;
        public int count;
        public Polygon() { }
        public Polygon(Point[] _points, int _count)
        {
            points = _points;
            count = _count;
        }
    }

    public class Geometry
    {
        public static int INF = 0x3f3f3f3f;
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp; temp = a; a = b; b = temp;
        }
        public static int Sign(int x)
        {
            int great = (x > 0) ? 1 : 0;
            int less = (x < 0) ? 1 : 0;
            return great - less;
        }
        public static double Distance(Point a, Point b)
        {
            return (b - a).abs();
        }
        public static int Cross(Point o, Point a, Point b)
        {
            return (a - o) / (b - o);
        }
        public static int CrossSign(Point o, Point a, Point b)
        {
            return Sign(Cross(o, a, b));
        }
        public static bool PointOnSegment(Point point, Line line)
        {
            return Sign((line.end - line.begin) / (point - line.begin)) == 0
                     && Sign((point - line.begin) * (point - line.end)) <= 0;
        }
        public static Rectangle BoundingRectangle(Polygon polygon)
        {
            int xmin = INF, ymin = INF, xmax = -INF, ymax = -INF;
            for (int i = 0; i < polygon.count; ++i)
            {
                int x = polygon.points[i].x, y = polygon.points[i].y;
                if (xmin > x) xmin = x;
                if (ymin > y) ymin = y;
                if (xmax < x) xmax = x;
                if (ymax < y) ymax = y;
            }
            return new Rectangle(new Point(xmin, ymin), new Point(xmax, ymax));
        }
        public static int PointInPolygon(Point point, Polygon polygon)
        {
            int result = 0;
            for (int i = 0; i < polygon.count; ++i)
            {
                Point u = (i == 0) ? polygon.points[polygon.count - 1] : polygon.points[i - 1];
                Point v = polygon.points[i];
                if (PointOnSegment(point, new Line(u, v))) return 1;
                if (Sign(u.y - v.y) > 0) Swap(ref u, ref v);
                if (Sign(u.y - point.y) >= 0 || Sign(v.y - point.y) < 0) continue;
                if (CrossSign(v, u, point) < 0) result ^= 1;
            }
            return result << 1;
        }
        public static int PointInPolygon(Point point, List<Point> polygon)
        {
            int result = 0;
            for (int i = 0; i < polygon.Count; ++i)
            {
                Point u = (i == 0) ? polygon[polygon.Count - 1] : polygon[i - 1];
                Point v = polygon[i];
                if (PointOnSegment(point, new Line(u, v))) return 1;
                if (Sign(u.y - v.y) > 0) Swap(ref u, ref v);
                if (Sign(u.y - point.y) >= 0 || Sign(v.y - point.y) < 0) continue;
                if (CrossSign(v, u, point) < 0) result ^= 1;
            }
            return result << 1;
        }
        public static int CrossRectangle(Rectangle a, Rectangle b)
        {
            int xmin = Math.Max(a.minP.x, b.minP.x);
            int ymin = Math.Max(a.minP.y, b.minP.y);
            int xmax = Math.Min(a.maxP.x, b.maxP.x);
            int ymax = Math.Min(a.maxP.y, b.maxP.y);
            return (xmin > xmax || ymin > ymax) ? 0 : 1;
        }
    }

    public class HillsPCG
    {
        static Random rd = new Random();
        public static bool HillsPointInBoundary(BaseModule[,] map, int x, int y, int type)
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, -1, 0, 1 };
            for (int dir = 0; dir < 4; ++dir)
            {
                int tx = x + dx[dir], ty = y + dy[dir];
                if (map[tx, ty].type != type) return true;
            }
            return false;
        }

        public static void GenerateHillsMap(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, int side = 10, double maxHigh = 1, int type = 1)
        {
            BaseModule[,] tmp = new BaseModule[xmax + 1, ymax + 1];
            for (int x = xmin; x <= xmax; ++x) for (int y = ymin; y <= ymax; ++y)
                {
                    tmp[x, y] = new BaseModule();
                    if (map[x, y].type == type) tmp[x, y].type = type;
                }

            // for (int x = xmin; x <= xmax; ++x)
            // {
            //     if (map[x, ymin].type == 1) { tmp[x, ymin].type = 1; tmp[x, ymin].height = 0; }
            //     if (map[x, ymax].type == 1) { tmp[x, ymax].type = 1; tmp[x, ymax].height = 0; }
            // }
            // for (int y = ymin; y <= ymax; ++y)
            // {
            //     if (map[xmin, y].type == 1) { tmp[xmin, y].type = 1; tmp[xmin, y].height = 0; }
            //     if (map[xmax, y].type == 1) { tmp[xmax, y].type = 1; tmp[xmax, y].height = 0; }
            // }
            for (int x = xmin + 1; x < xmax; ++x)
            {
                for (int y = ymin + 1; y < ymax; ++y)
                {
                    if (map[x, y].type == type && HillsPointInBoundary(map, x, y, type))
                    {
                        tmp[x, y].height = 0;
                    }
                }
            }
            RandomSamplingMapHeight(tmp, xmin, ymin, xmax, ymax, 0, side, maxHigh, type);

            for (int x = xmin; x <= xmax; ++x) for (int y = ymin; y <= ymax; ++y)
                    if (map[x, y].type == type) map[x, y].height = tmp[x, y].height;
        }

        public static void RandomSamplingMapHeight(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, int depth, int side, double maxHigh, int type)
        {
            if (depth > 100) return;
            if ((ymax - ymin <= side) && (xmax - xmin <= side))
            {
                //Random rd = new Random();
                if (map[xmin, ymin].height == -1) map[xmin, ymin].height = (map[xmin, ymin].type == type) ? maxHigh * rd.NextDouble() : 0;
                if (map[xmin, ymax].height == -1) map[xmin, ymax].height = (map[xmin, ymax].type == type) ? maxHigh * rd.NextDouble() : 0;
                if (map[xmax, ymin].height == -1) map[xmax, ymin].height = (map[xmax, ymin].type == type) ? maxHigh * rd.NextDouble() : 0;
                if (map[xmax, ymax].height == -1) map[xmax, ymax].height = (map[xmax, ymax].type == type) ? maxHigh * rd.NextDouble() : 0;
                // 采用 Bicubic interpolation 进行采样点内部的高度图差值：s(x) = -2x^3 + 3x^2
                float dx = xmax - xmin, dy = ymax - ymin;
                for (int x = xmin; x <= xmax; x++)
                {
                    for (int y = ymin; y <= ymax; y++)
                    {
                        if (map[x, y].type != type) continue;
                        if (dx == 0) dx = 1;
                        if (dy == 0) dy = 1;
                        float sxmax = (float)(-2 * Math.Pow((x - xmin) / dx, 3) + 3 * Math.Pow((x - xmin) / dx, 2));
                        float sxmin = (float)(-2 * Math.Pow((xmax - x) / dx, 3) + 3 * Math.Pow((xmax - x) / dx, 2));
                        float symax = (float)(-2 * Math.Pow((y - ymin) / dy, 3) + 3 * Math.Pow((y - ymin) / dy, 2));
                        float symin = (float)(-2 * Math.Pow((ymax - y) / dy, 3) + 3 * Math.Pow((ymax - y) / dy, 2));
                        map[x, y].height = map[xmin, ymin].height * sxmin * symin
                                            + map[xmax, ymin].height * sxmax * symin
                                            + map[xmin, ymax].height * sxmin * symax
                                            + map[xmax, ymax].height * sxmax * symax;
                    }
                }
            }
            else
            {
                int xmid = (xmin + (xmax - xmin) / 2);
                int ymid = (ymin + (ymax - ymin) / 2);
                if (xmax - xmin > side)
                {
                    RandomSamplingMapHeight(map, xmin, ymin, xmid, ymax, ++depth, side, maxHigh, type);
                    RandomSamplingMapHeight(map, xmid, ymin, xmax, ymax, ++depth, side, maxHigh, type);
                }
                else if (ymax - ymin > side)
                {
                    RandomSamplingMapHeight(map, xmin, ymin, xmax, ymid, ++depth, side, maxHigh, type);
                    RandomSamplingMapHeight(map, xmin, ymid, xmax, ymax, ++depth, side, maxHigh, type);
                }
            }
            return;
        }
    }

    public class Building
    {
        public List<Point> points;
        public List<int> type;

        public Building()
        {
            points = new List<Point>();
            type = new List<int>();
        }
    }

    public class TownPCG
    {
        static Random rd = new Random();
        public static List<Building> GenerateTownMap(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, int count, int typeNo)
        {
            List<Rectangle> rectangles = GenerateRectangles(map, xmin, ymin, xmax, ymax, count);
            //Console.WriteLine("rectangles: {0}", rectangles.Count);

            List<Building> buildings = new List<Building>();

            foreach (Rectangle rectangle in rectangles)
            {
                Building building = GenerateBuilding(rectangle);
                buildings.Add(building);
                //Console.WriteLine("buildings: {0}", buildings.Count);

                //MapObject
                MapObjectData tmp = RoomGenerator.RoomGenerator.Generate(building);
                map[rectangle.minP.x, rectangle.minP.y].obj = tmp;
            }

            // 房屋填充，供道路避障
            for (int x = xmin; x < xmax; ++x)
            {
                for (int y = ymin; y < ymax; ++y)
                {
                    for (int i = 0; i < buildings.Count; ++i)
                    {
                        if (Geometry.PointInPolygon(new Point(x, y), buildings[i].points) > 0)
                        {
                            //map[x, y].type = 3;     // 3 表示房屋
                            map[x, y].surface = 1;
                            map[x, y].typeNo = typeNo;
                        }
                    }
                }
            }

            // 改成最高点高度
            foreach (Rectangle rectangle in rectangles)
            {
                double maxHigh = 0;
                for (int x = rectangle.minP.x; x <= rectangle.maxP.x; ++x)
                {
                    for (int y = rectangle.minP.y; y <= rectangle.maxP.y; ++y)
                    {
                        if (map[x, y].surface == 1)
                        {
                            maxHigh = Math.Max(maxHigh, map[x, y].height);
                        }
                    }
                }
                //for (int x = rectangle.minP.x; x <= rectangle.maxP.x; ++x)
                //{
                //    for (int y = rectangle.minP.y; y <= rectangle.maxP.y; ++y)
                //    {
                //        //if (map[x, y].surface == 1)
                //        //{
                //            map[x, y].height = maxHigh / 2;
                //        //}
                //    }
                //}
                for (int x = rectangle.minP.x - 1; x <= rectangle.maxP.x + 1; ++x)
                {
                    for (int y = rectangle.minP.y - 1; y <= rectangle.maxP.y + 1; ++y)
                    {
                        if (x >= 0 && y >= 0 && x < map.GetLength(0) && y < map.GetLength(1))
                            map[x, y].height = maxHigh / 2;

                    }
                }
            }

            List<Point> Doors = new List<Point>();
            foreach (Building building in buildings)
            {
                for (int i = 0; i < building.type.Count; ++i)
                {
                    if (building.type[i] == 2)
                    {
                        Doors.Add(building.points[i]);
                        break;
                    }
                }
            }
            GenerateRoad(map, xmin, ymin, xmax, ymax, Doors, typeNo);

            return buildings;
        }

        // 生成 count 个不相交的矩形
        public static List<Rectangle> GenerateRectangles(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, int count = 9, int lengthMin = 15, int lengthMax = 20, int widthMin = 15, int widthMax = 20)
        {
            //Random rd = new Random();
            List<Rectangle> rectangles = new List<Rectangle>();

            for (int i = 0; i < count; ++i)
            {
                int ok = 0, cnt = 0;
                while (ok == 0)
                {
                    int length = rd.Next(lengthMin, lengthMax), width = rd.Next(widthMin, widthMax);
                    Point leftLow = new Point(rd.Next(xmin, xmax), rd.Next(ymin, ymax));
                    Point rightUp = new Point(leftLow.x + length, leftLow.y + width);
                    Rectangle rectangle = new Rectangle(leftLow, rightUp);
                    ok = 1;
                    if (rightUp.x > xmax || rightUp.y > ymax) ok = 0;
                    if (ok == 1)
                    {
                        for (int j = 0; j < rectangles.Count; ++j)
                        {
                            if (Geometry.CrossRectangle(rectangles[j], rectangle) > 0)
                            {
                                ok = 0; break;
                            }
                        }
                    }
                    // cross water
                    if (ok == 1)
                    {
                        for (int x = rectangle.minP.x; x < rectangle.maxP.x; ++x)
                        {
                            for (int y = rectangle.minP.y; y < rectangle.maxP.y; ++y)
                            {
                                if (map[x, y].type == -1)
                                {
                                    ok = 0; break;
                                }
                            }
                            if (ok == 0) break;
                        }
                    }

                    if (ok == 1) rectangles.Add(rectangle);
                    cnt++;
                    if (cnt > 100000) break;
                }
                if (cnt > 100000) break;
            }
            //Console.Write("gen rectangles: {0}", rectangles.Count);
            return rectangles;
        }

        public static Building GenerateBuilding(Rectangle rectangle)
        {
            Building building = new Building();
            //Random rd = new Random();
            int buildingType = rd.Next(0, 2);
            if (buildingType == 0)
            {
                building = BuildingType0(rectangle);
            }
            else if (buildingType == 1)
            {
                building = BuildingType1(rectangle);
            }
            // else if (buildingType == 2)
            // {
            //     building = BuildingType2(rectangle);
            // }
            // else if (buildingType == 3)
            // {
            //     building = BuildingType3(rectangle);
            // }
            InsertDoorAndWindow(building);
            return building;
        }

        // 矩形房屋
        public static Building BuildingType0(Rectangle rectangle)
        {
            Building building = new Building();
            int xmin = rectangle.minP.x + 1, ymin = rectangle.minP.y + 1;
            int xmax = rectangle.maxP.x - 1, ymax = rectangle.maxP.y - 1;
            building.points.Add(new Point(xmin, ymin)); building.type.Add(0);
            building.points.Add(new Point(xmax, ymin)); building.type.Add(0);
            building.points.Add(new Point(xmax, ymax)); building.type.Add(0);
            building.points.Add(new Point(xmin, ymax)); building.type.Add(0);
            return building;
        }

        // L形房屋
        public static Building BuildingType1(Rectangle rectangle)
        {
            Building building = BuildingType0(rectangle);
            // Random rd = new Random();
            Point center = new Point((building.points[0].x + building.points[2].x) / 2, (building.points[0].y + building.points[2].y) / 2);
            center.x += rd.Next(0, 3) - 1;
            center.y += rd.Next(0, 3) - 1;

            int type = rd.Next(0, 4);
            Point tmp = building.points[type];
            if (type == 0 || type == 2)
            {
                building.points.RemoveAt(type); building.type.RemoveAt(type);
                building.points.Insert(type, new Point(center.x, tmp.y)); building.type.Add(0);
                building.points.Insert(type, center); building.type.Add(0);
                building.points.Insert(type, new Point(tmp.x, center.y)); building.type.Add(0);
            }
            else if (type == 1 || type == 3)
            {
                building.points.RemoveAt(type); building.type.RemoveAt(type);
                building.points.Insert(type, new Point(tmp.x, center.y)); building.type.Add(0);
                building.points.Insert(type, center); building.type.Add(0);
                building.points.Insert(type, new Point(center.x, tmp.y)); building.type.Add(0);
            }
            return building;
        }
        public static void InsertDoorAndWindow(Building building)
        {
            for (int i = building.points.Count - 1; i > 0; --i)
            {
                int j = i - 1;
                Point u = building.points[i], v = building.points[j];
                if (u.x == v.x)
                {
                    int length = u.y - v.y;
                    if (Math.Abs(length) > 10)
                    {
                        building.points.Insert(i, new Point(v.x, v.y + 2 * length / 3));
                        building.type.Insert(i, 1);
                        building.points.Insert(i, new Point(v.x, v.y + length / 3));
                        building.type.Insert(i, 1);
                    }
                    else if (Math.Abs(length) > 5)
                    {
                        building.points.Insert(i, new Point(v.x, v.y + length / 2));
                        building.type.Insert(i, 1);
                    }
                }
                else
                {
                    int length = u.x - v.x;
                    if (Math.Abs(length) > 10)
                    {
                        building.points.Insert(i, new Point(v.x + 2 * length / 3, v.y));
                        building.type.Insert(i, 1);
                        building.points.Insert(i, new Point(v.x + length / 3, v.y));
                        building.type.Insert(i, 1);
                    }
                    else if (Math.Abs(length) > 5)
                    {
                        building.points.Insert(i, new Point(v.x + length / 2, v.y));
                        building.type.Insert(i, 1);
                    }
                }
            }

            //Random rd = new Random();
            int cnt = 1000;
            while (cnt > 0)
            {
                int index = rd.Next(0, building.type.Count);
                if (building.type[index] == 1)
                {
                    building.type[index] = 2;
                    break;
                }
                cnt--;
            }
            if (cnt == 0)
            {
                for (int i = 0; i < building.type.Count; ++i)
                {
                    if (building.type[i] == 1)
                    {
                        building.type[i] = 2;
                        break;
                    }
                }
            }
        }

        public static void GenerateRoad(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, List<Point> Doors, int typeNo)
        {
            // 距离每个点最近的若干个点
            //for (int i = 0; i < Doors.Count; ++i)
            //{
            //    List<Tuple<double, int>> neighbor = new List<Tuple<double, int>>();
            //    for (int j = 0; j < Doors.Count; ++j)
            //    {
            //        if (i != j)
            //        {
            //            double distance = Geometry.Distance(Doors[i], Doors[j]);
            //            neighbor.Add(new Tuple<double, int>(distance, j));
            //        }
            //    }
            //    neighbor.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            //    // 与最近的若干个点连接道路
            //    for (int j = 0; j < neighbor.Count; ++j)
            //    {
            //        int index = neighbor[j].Item2;
            //        //Console.Write(index+" ");
            //        SearchRoad(map, xmin, ymin, xmax, ymax, Doors[i], Doors[index]);
            //    }
            //    //Console.WriteLine();
            //}
            for (int i = 0; i < Doors.Count; ++i)
            {
                for (int j = i + 1; j < Doors.Count; ++j)
                {
                    SearchRoad(map, xmin, ymin, xmax, ymax, Doors[i], Doors[j], typeNo);
                }
            }
            if (Doors.Count == 1)
            {
                SearchRoad(map, xmin, ymin, xmax, ymax, Doors[0], new Point(xmin, ymin), typeNo);
            }
        }
        public static void SearchRoad(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, Point start, Point end, int typeNo)
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            int[,] vis = new int[xmax, ymax];
            for (int x = xmin; x < xmax; ++x) for (int y = ymin; y < ymax; ++y) vis[x, y] = 0;

            Queue<Tuple<Point, List<Point>>> Q = new Queue<Tuple<Point, List<Point>>>();
            Q.Enqueue(new Tuple<Point, List<Point>>(start, new List<Point>()));
            int cnt = 20000;
            while (Q.Count != 0 && cnt > 0)
            {
                cnt--;
                Tuple<Point, List<Point>> fron = Q.Dequeue();
                Point p = fron.Item1;
                List<Point> path = fron.Item2;
                for (int dir = 0; dir < 4; ++dir)
                {
                    int tx = p.x + dx[dir], ty = p.y + dy[dir];
                    if (tx == end.x && ty == end.y /*|| 
                        tx >= xmin && tx < xmax && ty >= ymin && ty < ymax && map[tx, ty].type == 4*/)
                    {
                        PaintRoad(map, path, typeNo);
                        return;
                    }
                    if (CheckRoadPoint(map, xmin, ymin, xmax, ymax, vis, tx, ty))
                    {
                        vis[tx, ty] = 1;
                        Point point = new Point(tx, ty);
                        //List<Point> newPath = path;
                        List<Point> newPath = new List<Point>();
                        for (int i = 0; i < path.Count; ++i)
                        {
                            newPath.Add(path[i]);
                        }


                        newPath.Add(point);
                        Q.Enqueue(new Tuple<Point, List<Point>>(point, newPath));
                    }
                }
            }
        }
        public static bool CheckRoadPoint(BaseModule[,] map, int xmin, int ymin, int xmax, int ymax, int[,] vis, int tx, int ty)
        {
            return tx >= xmin && tx < xmax && ty >= ymin && ty < ymax && vis[tx, ty] != 1 && map[tx, ty].surface != 1;
        }
        public static void PaintRoad(BaseModule[,] map, List<Point> path, int typeNo)
        {
            foreach (Point point in path)
            {
                //map[point.x, point.y].type = 4;
                map[point.x, point.y].surface = 0;
                map[point.x, point.y].typeNo = typeNo;
            }
        }
    }

    //public class Test
    //{
    //    static void Main(string[] args)
    //    {
    //        Console.WriteLine("Hello World");

    //        BaseModule[,] map = new BaseModule[100,100];
    //        for (int i = 0; i < 100; ++i) for (int j = 0; j < 100; ++j) map[i, j] = new BaseModule();

    //        List<Building> res = TownPCG.GenerateTownMap(map, 0, 0, 100, 100, 9);
    //        Unit.printMap(map, 100, 100);
    //    }

    //}

    public class Unit
    {
        public static void printMap(BaseModule[,] map, int row, int col)
        {
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < col; ++j)
                {
                    if (map[i, j].type == -1) Console.Write(' ');
                    else Console.Write(map[i, j].type);
                }
                Console.WriteLine();
            }
        }
    }

}