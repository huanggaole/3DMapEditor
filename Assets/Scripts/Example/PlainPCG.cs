using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainPCG: MonoBehaviour
{
    public GameObject ground_grass;

    public int mapWidth;
    public int mapHeight;

    public void GenerateMap(){
        // 使用算法生成地图数组
        int[,] map = PlainPCG.GeneratePlainMap(mapWidth, mapHeight);
        // 根据地图数组建模游戏场景
        this.RenderMap(map);
    }

    public void RenderMap(int[,] map){
        // 首先把之前已经生成的地图完全清除
        int childCount = this.gameObject.transform.childCount;
        for (int i = childCount - 1; i >=0; i-- )
        {
            DestroyImmediate(this.gameObject.transform.GetChild(i).gameObject);
        }
        // 根据地图数据装配新地图
        for(int y = 0; y < mapHeight; y++){
            for(int x = 0; x < mapWidth; x++){
                if(map[x,y] == 1){
                    Vector3 position = new Vector3(x, 0, y);
                    Quaternion rotation = new Quaternion();
                    Instantiate(this.ground_grass, position, rotation, this.gameObject.transform);
                }
            }
        }
    }

    public static int[,] GeneratePlainMap(int mapWidth, int mapHeight){
        int[,] plainMap = new int[mapWidth, mapHeight];
        for(int y = 0; y < mapHeight; y++){
            for(int x = 0; x < mapWidth; x++){
                plainMap[x,y] = 1;
            }
        }
        return plainMap;
    }
}
