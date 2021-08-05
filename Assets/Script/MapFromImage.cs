using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFromImage
{
    public static MapSurfaceData[,] fromImage(Texture2D img, int row, int col)
    {
        MapSurfaceData[,] res = new MapSurfaceData[row, col];

        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < col; y++)
            {
                Color c = img.GetPixel(img.width * x / row, img.height * y / col);
                res[x, y] = new MapSurfaceData();
                res[x, y].height = c.a;
                if (c.r * 2 > c.g + c.b)
                    res[x, y].type = SurfaceTypes.ROAD;
                if (c.b * 2 > c.r + c.g)
                    res[x, y].type = SurfaceTypes.WATER;
                if (c.g * 2 > c.r + c.b)
                    res[x, y].type = SurfaceTypes.GRASS_BLOCK;
            }
        }

        return res;
    }
}
