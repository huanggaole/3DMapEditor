using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData {
    public float[,] heightmap;     // 值为高度图中当前位置的高度
    public int[,] maptype;         // 1代表绿地，0代表水体
    public int[,,] mapvoxel;            // 用来存放3D地图，1代表绿地，0代表水体，-1代表空。
}

public class PlainPCG: MonoBehaviour
{
    public GameObject ground_grass;
    public GameObject ground_water;
    public GameObject cliff_toprock;
    public GameObject cliff_rock;
    public GameObject cliff_topwater;
    public GameObject cliff_water;
    public List<GameObject> stones;
    public List<GameObject> trees;


    public int mapLength;
    public int mapWidth;
    public int mapHeight;

    public void GenerateMap(){
        // 使用算法生成地图数组
        MapData map = this.GeneratePlainMap(mapLength, mapWidth, mapHeight);
        // 根据地图数组建模游戏场景
        if (map != null) {
            this.RenderMap(map);
        }
    }

    // 渲染地图的函数
    public void RenderMap(MapData map){
        // 首先把之前已经生成的地图完全清除
        int childCount = this.gameObject.transform.childCount;
        for (int i = childCount - 1; i >=0; i-- )
        {
            DestroyImmediate(this.gameObject.transform.GetChild(i).gameObject);
        }
        // 根据地图数据装配新地图
        for(int y = 0; y < mapLength; y++){
            for(int x = 0; x < mapWidth; x++){
                for (int z = 0; z < mapHeight; z++) {
                    if (map.mapvoxel[x, y, z] != -1) {
                        Vector3 position = new Vector3(x, z, y);
                        Quaternion rotation = new Quaternion();
                        if (map.mapvoxel[x, y, z] == 1) {
                            Instantiate(this.ground_grass, position, rotation, this.gameObject.transform);
                        }
                        if (map.mapvoxel[x, y, z] == 0)
                        {
                            Instantiate(this.ground_water, position, rotation, this.gameObject.transform);
                        }
                        if (map.mapvoxel[x, y, z] == 2)
                        {
                            position.y--;
                            GameObject stoneobj = this.stones[Random.Range(0, this.stones.Count)];
                            Instantiate(stoneobj, position, rotation, this.gameObject.transform);
                            continue;
                        }
                        if (map.mapvoxel[x, y, z] == 3)
                        {
                            position.y--;
                            GameObject treeobj = this.trees[Random.Range(0, this.stones.Count)];
                            Instantiate(treeobj, position, rotation, this.gameObject.transform);
                            continue;
                        }
                        int dh = 0;
                        GameObject objtop = this.cliff_toprock;
                        GameObject obj = this.cliff_rock;
                        // 渲染侧边
                        // 渲染北侧边
                        objtop = this.cliff_toprock;
                        obj = this.cliff_rock;
                        if (y == 0)
                        {
                            dh = z;
                        }
                        else
                        {
                            dh = (int)(map.heightmap[x,y] - map.heightmap[x,y - 1]);
                            if (dh < 0) dh = 0;
                            if (map.maptype[x,y] == 0 && map.maptype[x,y - 1] == 0) {
                                objtop = this.cliff_topwater;
                                obj = this.cliff_water;
                            }
                        }
                        for (int zz = 0; zz < dh; zz++)
                        {
                            position = new Vector3(x, z - zz - 1, y - 1);
                            rotation = new Quaternion();
                            rotation.SetEulerAngles(0, Mathf.PI * 180 / 180, 0);
                            if (zz == 0)
                            {
                                Instantiate(objtop, position, rotation, this.gameObject.transform);
                            }
                            else
                            {
                                Instantiate(obj, position, rotation, this.gameObject.transform);
                            }
                        }
                        // 渲染南侧边
                        objtop = this.cliff_toprock;
                        obj = this.cliff_rock;
                        if (y == mapLength - 1)
                        {
                            dh = z;
                        }
                        else
                        {
                            dh = (int)(map.heightmap[x, y] - map.heightmap[x, y + 1]);
                            if (dh < 0) dh = 0;
                            if (map.maptype[x, y] == 0 && map.maptype[x, y + 1] == 0)
                            {
                                objtop = this.cliff_topwater;
                                obj = this.cliff_water;
                            }
                        }
                        for (int zz = 0; zz < dh; zz++)
                        {
                            position = new Vector3(x, z - zz - 1, y + 1);
                            rotation = new Quaternion();
                            rotation.SetEulerAngles(0, Mathf.PI * 0 / 180, 0);
                            if (zz == 0)
                            {
                                Instantiate(objtop, position, rotation, this.gameObject.transform);
                            }
                            else
                            {
                                Instantiate(obj, position, rotation, this.gameObject.transform);
                            }
                        }
                        // 渲染西侧边
                        objtop = this.cliff_toprock;
                        obj = this.cliff_rock;
                        if (x == 0)
                        {
                            dh = z;
                        }
                        else
                        {
                            dh = (int)(map.heightmap[x, y] - map.heightmap[x - 1, y]);
                            if (dh < 0) dh = 0;
                            if (map.maptype[x, y] == 0 && map.maptype[x - 1, y] == 0)
                            {
                                objtop = this.cliff_topwater;
                                obj = this.cliff_water;
                            }
                        }
                        for (int zz = 0; zz < dh; zz++)
                        {
                            position = new Vector3(x - 1, z - zz - 1, y);
                            rotation = new Quaternion();
                            rotation.SetEulerAngles(0, Mathf.PI * -90 / 180, 0);
                            if (zz == 0)
                            {
                                Instantiate(objtop, position, rotation, this.gameObject.transform);
                            }
                            else
                            {
                                Instantiate(obj, position, rotation, this.gameObject.transform);
                            }
                        }
                        // 渲染东侧边
                        objtop = this.cliff_toprock;
                        obj = this.cliff_rock;
                        if (x == mapWidth - 1)
                        {
                            dh = z;
                        }
                        else
                        {
                            dh = (int)(map.heightmap[x, y] - map.heightmap[x + 1, y]);
                            if (dh < 0) dh = 0;
                            if (map.maptype[x, y] == 0 && map.maptype[x + 1, y] == 0)
                            {
                                objtop = this.cliff_topwater;
                                obj = this.cliff_water;
                            }
                        }
                        for (int zz = 0; zz < dh; zz++)
                        {
                            position = new Vector3(x + 1, z - zz - 1, y);
                            rotation = new Quaternion();
                            rotation.SetEulerAngles(0, Mathf.PI * 90 / 180, 0);
                            if (zz == 0)
                            {
                                Instantiate(objtop, position, rotation, this.gameObject.transform);
                            }
                            else
                            {
                                Instantiate(obj, position, rotation, this.gameObject.transform);
                            }
                        }
                    }
                }
            }
        }
        // 
    }

    // 生成地图的函数
    MapData GeneratePlainMap(int mapLength, int mapWidth, int mapHeight){
        if (mapLength < 1 || mapWidth < 1) {
            return null;
        }
        MapData map = new MapData();
        map.heightmap = new float[mapWidth, mapLength];   // 值为高度图中当前位置的高度
        map.maptype = new int[mapWidth, mapLength];         // 1代表绿地，0代表水体
        for (int y = 0; y < mapLength; y++){
            for(int x = 0; x < mapWidth; x++){
                map.heightmap[x,y] = -1;
                map.maptype[x,y] = 1;
            }
        }
        // 利用差值法生成地形高度图，值域为[0,1]
        this.RandomSamplingMapHeight(map.heightmap, 0, 0, mapWidth - 1, mapLength - 1);
        // 找到地形图上的最高点与最低点。
        float maxalt = -1.0f;
        float minalt = 2.0f;
        Vector2 maxP = new Vector2(0, 0);
        Vector2 minP = new Vector2(mapWidth - 1, mapLength - 1);
        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map.heightmap[x, y] > maxalt) {
                    maxalt = map.heightmap[x, y];
                    maxP.x = x;
                    maxP.y = y;
                } else if (map.heightmap[x, y] < minalt) {
                    minalt = map.heightmap[x, y];
                    minP.x = x;
                    minP.y = y;
                }
            }
        }
        // 在最高点与最低点之间生成一条河流，河流总是从高处向低处流。如果遇到多处邻接的地块相同，则任选一处邻接地块流动。
        Vector2 tmpP = new Vector2(maxP.x, maxP.y);
        map.maptype[(int)tmpP.x, (int)tmpP.y] = 0;
        int [] dx = { -1, 0, 1, 0 };
        int [] dy = { 0, -1, 0, 1};
        while (tmpP.x != minP.x || tmpP.y != minP.y) {
            List<Vector2> nextPs = new List<Vector2>();
            for (int i = 0; i < 4; i++) {
                int newx = (int)tmpP.x + dx[i];
                int newy = (int)tmpP.y + dy[i];
                if (newx > 0 && newy > 0 && newx < mapWidth && newy < mapLength) {
                    if (map.heightmap[(int)tmpP.x, (int)tmpP.y] >= map.heightmap[newx, newy]) {
                        nextPs.Add(new Vector2(newx, newy));
                    }
                }
            }
            if (nextPs.Count == 0) {
                break;
            }
            int index = Random.Range(0, nextPs.Count);
            tmpP.x = nextPs[index].x;
            tmpP.y = nextPs[index].y;
            map.maptype[(int)tmpP.x, (int)tmpP.y] = 0;
        }
        // 地形高度图乘以玩家设置的地图高度并取整，得到地图中每个图块的体素高度。
        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map.heightmap[x, y] = Mathf.Floor(map.heightmap[x, y] * mapHeight);
            }
        }
        // 新建一个三维数组，用来存放3D地图，1代表绿地，0代表水体，2代表石头，3代表树木，-1代表空。
        map.mapvoxel = new int[mapWidth, mapLength, mapHeight + 1];
        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapHeight; z++) {
                    if (z == (int)map.heightmap[x, y]) {
                        map.mapvoxel[x, y, z] = map.maptype[x, y];
                    }
                    else {
                        map.mapvoxel[x, y, z] = -1;
                    }
                }
            }
        }
        // 在地图上放置地图面积1/50数量的石头
        int stoneNum = this.mapWidth * this.mapLength / 50;
        for (int i = 0; i < stoneNum; i++) {
            int rndx = Random.Range(0, mapWidth);
            int rndy = Random.Range(0, mapLength);
            if (map.maptype[rndx, rndy] == 1) {
                map.mapvoxel[rndx, rndy, (int)map.heightmap[rndx, rndy] + 1] = 2;
            }
        }
        // 在地图上放置地图面积1/10数量的石头
        int treeNum = this.mapWidth * this.mapLength / 10;
        for (int i = 0; i < treeNum; i++)
        {
            int rndx = Random.Range(0, mapWidth);
            int rndy = Random.Range(0, mapLength);
            if (map.maptype[rndx, rndy] == 1)
            {
                map.mapvoxel[rndx, rndy, (int)map.heightmap[rndx, rndy] + 1] = 3;
            }
        }
        return map;
    }

    // 采用分治法随机采样地图某一坐标处的地形图高度，取值为[0,1]
    public void RandomSamplingMapHeight(float [,]heightmap, int xmin, int ymin, int xmax, int ymax, int depth = 0) {
        if (depth > 100) {
            return;
        }
        if ((ymax - ymin <= 20) && (xmax - xmin <= 20))
        {
            if (heightmap[xmin, ymin] == -1) {
                heightmap[xmin, ymin] = Random.Range(0f, 1f);
            }
            if (heightmap[xmin, ymax] == -1)
            {
                heightmap[xmin, ymax] = Random.Range(0f, 1f);
            }
            if (heightmap[xmax, ymin] == -1)
            {
                heightmap[xmax, ymin] = Random.Range(0f, 1f);
            }
            if (heightmap[xmax, ymax] == -1)
            {
                heightmap[xmax, ymax] = Random.Range(0f, 1f);
            }
            // 采用 Bicubic interpolation 进行采样点内部的高度图差值：s(x) = -2x^3 + 3x^2
            float dx = xmax - xmin;
            float dy = ymax - ymin;
            for (int x = xmin; x <= xmax; x++) {
                for (int y = ymin; y <= ymax; y++) {
                    if (dx == 0) {
                        dx = 1;
                    }
                    if (dy == 0) {
                        dy = 1;
                    }
                    float sxmax = -2 * Mathf.Pow((x - xmin) / dx, 3) + 3 * Mathf.Pow((x - xmin) / dx, 2);
                    float sxmin = -2 * Mathf.Pow((xmax - x) / dx, 3) + 3 * Mathf.Pow((xmax - x) / dx, 2);
                    float symax = -2 * Mathf.Pow((y - ymin) / dy, 3) + 3 * Mathf.Pow((y - ymin) / dy, 2);
                    float symin = -2 * Mathf.Pow((ymax - y) / dy, 3) + 3 * Mathf.Pow((ymax - y) / dy, 2);
                    heightmap[x, y] = heightmap[xmin, ymin] * sxmin * symin + heightmap[xmax, ymin] * sxmax * symin + heightmap[xmin, ymax] * sxmin * symax + heightmap[xmax, ymax] * sxmax * symax;
                }
            }
        }
        else {
            int xmid = (xmin + (xmax - xmin) / 2);
            int ymid = (ymin + (ymax - ymin) / 2);
            if (xmax - xmin > 20) {
                this.RandomSamplingMapHeight(heightmap, xmin, ymin, xmid, ymax, ++depth);
                this.RandomSamplingMapHeight(heightmap, xmid, ymin, xmax, ymax, ++depth);
            } else if (ymax - ymin > 20) {
                this.RandomSamplingMapHeight(heightmap, xmin, ymin, xmax, ymid, ++depth);
                this.RandomSamplingMapHeight(heightmap, xmin, ymid, xmax, ymax, ++depth);
            }
        }
        return;
    }
}
