using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
    public Vector2 mapSize = new Vector2(250, 50);

    [SerializeField]private AnimationCurve mapCurve;
    public SpriteRenderer sprite;

    public int[,] map;

    [Header("MapGeneration")]
    //public bool generateAll;
    public bool generateMap;
    public bool newSeed;
    public bool generateGrass;
    public bool generateCave;
    public bool fillGaps;

    [Header("Cave Generation")]
    [Range(0f, 1f)]public float density;
    [Range(1, 10)]public int steps = 3;
    public FindNeighboursMode findNeighboursMode = FindNeighboursMode.NEIGHBOURS_4;
    public int cavesAmmount = 2;
    
    [Header("Cave Gaps")]
    [Range(1, 10)]public int gapSteps = 3;
    public FindNeighboursMode gapsMode = FindNeighboursMode.NEIGHBOURS_4;
    [Range(1, 8)]public int minNeighboursToFill = 7;

    [Header("Destroyer")]
    public int range = 4;
    public DestroyMode destroyMode;
    public bool destroy;

    [Header("Physics")]
    public bool simulatePhysics;

    [Header("Shortcuts")]
    public bool generateCavesAndFill;
    public bool generateMapCavesAndFill;

    [Header("Seed")]
    public int seed;
    public bool randomizeSeed;

    private void Update () {
        if(randomizeSeed) {
            RandomizeSeed();
            randomizeSeed = false;
        }

        if (generateCavesAndFill) {
            generateCavesAndFill = false;
            generateCave = true;
            fillGaps = true;
        }

        if (generateMapCavesAndFill) {
            generateMapCavesAndFill = false;
            generateMap = true;
            generateCave = true;
            fillGaps = true;
        }

        if (generateMap) {
            generateMap = false;

            if(newSeed) { RandomizeSeed(); }

            GenerateCurve(seed, 10, 30);
            GenerateMap();
        }

        if (generateGrass) {
            generateGrass = false;
            GenerateGrass();
        }

        if (generateCave) {
            generateCave = false;
            GenerateCaves(cavesAmmount, 3, 0, steps, 1f - density, findNeighboursMode);
        }

        if (fillGaps) {
            fillGaps = false;
            FillCaveGaps(gapSteps, minNeighboursToFill, 3, gapsMode);
        }

        if (destroy) {
            destroy = false;
            DestroyMap(range, destroyMode);
        }

        if (simulatePhysics) {
            simulatePhysics = false;
            SimulatePhysics();
        }
    }

    public int[,] GetMap () {
        return map;
    }

    public void SetMap (int[,] map) {
        this.map = map;
    }

    public void RandomizeSeed () {
        seed = Random.Range(int.MinValue, int.MaxValue);
    }

    private void GenerateCurve (int seed, int minKeys, int maxKeys) {
        int keys = RandomHelper.Range(seed, minKeys, maxKeys);

        //Debug.Log("Keys: " + keys);

        mapCurve = new AnimationCurve();

        for(int key = 0; key <= keys; key++) {
            float time = (float)key / (float)keys;
            //Debug.Log("Time: " + time);
            mapCurve.AddKey(time, RandomHelper.Percent(seed + key));
        }
    }

    private void GenerateImg () {
        Texture2D texture = new Texture2D((int)mapSize.x, (int)mapSize.y);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                Color color = Color.white;

                if (map[x, y] == 0) {           //None
                    color = new Color(0, 0, 0, 0);

                } else if (map[x, y] == 1) {    //BasicBlock "Stone"
                    color = Color.gray;

                } else if (map[x, y] == 2) {    //Grass
                    color = new Color(0, 204f / 255f, 0);

                } else if (map[x, y] == 3) {    //Caves
                    //color = Color.black;
                    color = new Color(0, 0, 0, 0);

                } else if (map[x, y] == 4) {    //Destroyed
                    color = Color.cyan;

                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        sprite.sprite = Sprite.Create(texture, new Rect(0, 0, (int)mapSize.x, (int)mapSize.y), Vector2.zero, 1);
    }

    private void GenerateMap () {
        map = new int[(int)mapSize.x, (int)mapSize.y];

        for (int x = 0; x < map.GetLength(0); x++) {
            for (int y = 0; y < map.GetLength(1); y++) {
                float yHeight = y / mapSize.y;
                float height = mapCurve.Evaluate(x / mapSize.x);

                if (height >= yHeight) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = 0;
                }
            }
        }

        GenerateImg();
    }

    private void GenerateGrass () {
        for (int x = 0; x < (int)mapSize.x; x++) {
            for (int y = 0; y < (int)mapSize.y - 1; y++) {
                if(y == 0 && map[x, y] == 0) {
                    break;
                }
                
                if(x > 0 && x < mapSize.x - 1) {
                    if(map[x - 1, y] == 0) {
                        map[x, y] = 2;
                        //break;
                    } else if(map[x + 1, y] == 0) {
                        map[x, y] = 2;
                        //break;
                    }
                }
                
                if(map[x, y + 1] == 0) {
                    map[x, y] = 2;
                    break;
                }

                if (y == (int)mapSize.y - 2 && map[x, y] == 1) {
                    map[x, y + 1] = 2;
                    break;
                }
            }
        }

        GenerateImg();
    }

    //Old code
    //private void GenerateCaves (int caves = 1) {
    //    for (int i = 0; i < caves; i++) {
    //        CavesGenerator caveGenerator = new CavesGenerator(this);
    //        List<Vector2> caveBlocks = caveGenerator.GenerateCave();

    //        for (int j = 0; j < caveBlocks.Count; j++) {
    //            int x = (int)caveBlocks[j].x;
    //            int y = (int)caveBlocks[j].y;

    //            //print("X|Y: " + x + "|" + y);

    //            if (map[x,y] == 1) {
    //                map[x, y] = 3;
    //            }
    //        }
    //    }

    //    GenerateImg();
    //}

    private void GenerateCaves (int cavesAmmount = 1, int blockId = 3, int voidBlockId = 0, int stps = 3, float density = .5f, FindNeighboursMode findMode = FindNeighboursMode.NEIGHBOURS_4) {
        CavesGenerator caveGenerator = new CavesGenerator(this);
        for (int i = 0; i < cavesAmmount; i++) {
            caveGenerator.GenerateCave(blockId, voidBlockId, steps, density, findMode, true);
        }

        GenerateImg();
    }

    private void FillCaveGaps (int steps, int minNeighboursToFill, int blockId, FindNeighboursMode findMode = FindNeighboursMode.NEIGHBOURS_4) {
        CavesGenerator caveGenerator = new CavesGenerator(this);
        caveGenerator.FillGaps(steps, blockId, findMode, minNeighboursToFill);
        GenerateImg();
    }

    private void DestroyMap (int range, DestroyMode mode) {
        MapDestroyer mapDestroyer = new MapDestroyer(this);

        Vector2 block = new Vector2(Random.Range(0, (int)mapSize.x), Random.Range(0, (int)mapSize.y));
        mapDestroyer.Destroy(block, range, mode, 4);

        GenerateImg();
    }

    private void SimulatePhysics () {
        PhysicsManager.Simulate(map, 3);
        GenerateImg();
    }
}