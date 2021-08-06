
public enum VoxelType { WALL, DOOR, WINDOW, FLOOR, ROOF, TREE, GRASS, ROCK }

public enum FurType { Empty, DoubleBed, SmallContainer, Sofa, Table, TV, DiningTable, Toilet, Washer, Frozer, Cooker }
public class MapObjectData
{
    public VoxelType[,,] voxel_types; // 构成该物体（建筑或树木）的体素类型
    public int[,,] voxel_direction; // 体素方向
    public FurType[,,] fur_types;
    public int[,,] fur_direction;
    public float direction = 0; // 物体方向，任意角度，单位：度
    public float[] scale = { 1, 1, 1 }; // 物体缩放，本地空间（x，y，z）三个方向
    public int[] staircase;
}

public class BaseModule
{
    public int x;
    public int y;
    public double height; // 高度
    public int islandCnt; //岛屿编号
    public int type; // -1:无 0:陆地 1:丘陵 2:城镇 
    public int typeNo; // 类型数据编号
    public int surface; // -1:无 0:道路 1:房屋 2:植被 3:森林
    public MapObjectData obj = null;

    public BaseModule()
    {
        this.x = -1;
        this.y = -1;
        this.height = -1.0;
        this.islandCnt = -1;
        this.type = -1;
        this.typeNo = -1;
        this.surface = -1;
        this.obj = null;
    }
}

public class Point
{
    public int x;
    public int y;
    public Point() { }
    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public static Point operator +(Point a, Point b)
    {
        return new Point(a.x + b.x, a.y + b.y);
    }
    public static Point operator -(Point a, Point b)
    {
        return new Point(a.x - b.x, a.y - b.y);
    }
    public static int operator *(Point a, Point b)
    {
        return a.x * b.x + a.y * b.y;
    }
    public static int operator /(Point a, Point b)
    {
        return a.x * b.y - a.y * b.x;
    }
    public double abs()
    {
        return System.Math.Sqrt(x * x + y * y);
    }
}