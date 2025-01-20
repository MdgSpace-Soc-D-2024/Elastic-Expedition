using UnityEngine;
using UnityEngine.Tilemaps;
public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] int width,height;
    [SerializeField] int minStoneheight, maxStoneHeight;
    [SerializeField] GameObject dirt,grass,stone;
    [SerializeField] float heightValue, smoothness;
    [SerializeField]float seed;

    void Start()
    {
        Generation();
    }

    void Generation()
    {
        for (int x = 0; x < width; x++)//This will help spawn a tile on the x axis
        {
            // now for procedural generation we need to gradually increase and decrease the height value
            int height = Mathf.RoundToInt (heightValue * Mathf.PerlinNoise(x / smoothness, seed));
            int minStoneSpawnDistance = height - minStoneheight;
            int maxStoneSpawnDistance = height - maxStoneHeight;
            int totalStoneSpawnDistance = Random.Range(minStoneSpawnDistance, maxStoneSpawnDistance);
            //Perlin noise.
            for (int y = 0; y < height; y++)//This will help spawn a tile on the y axis
            {
                if (y < totalStoneSpawnDistance)
                {
                    spawnObj(stone, x, y);
                }
                else
                {
                    spawnObj(dirt, x, y);
                }
               
            }
            if(totalStoneSpawnDistance == height)
            {
                spawnObj(stone, x, height);
            }
            else
            {
                spawnObj(grass, x, height);
            }
            
        }
    }
    
    void spawnObj(GameObject obj,int width,int height)//What ever we spawn will be a child of our procedural generation gameObj
    {
        obj = Instantiate(obj, new Vector2(width, height), Quaternion.identity);
        obj.transform.parent = this.transform;
    }
}
