using UnityEngine;
using System.Collections.Generic;

public enum FindNeighboursMode {
    NEIGHBOURS_4,  //4 Neightbours
    NEIGHBOURS_8   //8 Neightbours
}

public class CavesGenerator {
    private Vector2 mapSize;
    private MapGenerator mapGenerator;
    private int[,] map;

    public CavesGenerator (MapGenerator mapGenerator) {
        mapSize = mapGenerator.mapSize;
        this.mapGenerator = mapGenerator;
        map = mapGenerator.GetMap();
    }

    //Old code
    //public List<Vector2> GenerateCaveOld () {
    //    int x = (int)(Random.value * mapSize.x);
    //    int y = (int)(Random.value * mapSize.y);

    //    mapGenerator.map[x, y] = 99;

    //    //Debug.Log("X-Y: " + x + "-" + y);

    //    List<Vector2> blocks = new List<Vector2>();
    //    List<Vector2> newBlocks = new List<Vector2>();
    //    newBlocks.Add(new Vector2(x, y));

    //    List<Vector2> selectedBlocks = new List<Vector2>();

    //    while (newBlocks.Count > 0) {
    //        for (int i = 0; i < newBlocks.Count; i++) {
    //            selectedBlocks = new List<Vector2>();
    //            //Vector2 actBlock = newBlocks[i];
    //            List<Vector2> nearest = GetNearestBlocks(newBlocks[i]);

    //            List<Vector2> validBlocks = new List<Vector2>();

    //            //Check validity
    //            for (int j = 0; j < nearest.Count; j++) {
    //                if (!ContainsBlock(validBlocks, nearest[j]) && IsInsideMap(nearest[j])) {
    //                    validBlocks.Add(nearest[j]);
    //                    //Debug.Log("IsVallid: " + j);
    //                }
    //            }

    //            //for (int a = 0; a < validBlocks.Count; a++) {
    //            //    mapGenerator.map[(int)validBlocks[a].x, (int)validBlocks[a].y] = 98;
    //            //}

    //            //break;
    //            //Debug.Break();

    //            //Add it the list
    //            for (int j = 0; j < validBlocks.Count; j++) {
    //                if (Random.value > .3f) {
    //                    selectedBlocks.Add(validBlocks[j]);
    //                }
    //            }
    //        }

    //        blocks.AddRange(newBlocks);
    //        newBlocks = new List<Vector2>();
    //        newBlocks.AddRange(selectedBlocks);
    //    }

    //    //print("Length: " + blocks.Count);

    //    return blocks;
    //}

    //private List<Vector2> GetNearestBlocks (Vector2 pos) {
    //    List<Vector2> positions = new List<Vector2>();

    //    positions.Add(new Vector2(pos.x - 1, pos.y));
    //    positions.Add(new Vector2(pos.x + 1, pos.y));
    //    positions.Add(new Vector2(pos.x, pos.y - 1));
    //    positions.Add(new Vector2(pos.x, pos.y + 1));

    //    return positions;
    //}

    //private bool ContainsBlock (List<Vector2> allBlocks, Vector2 block) {
    //    for (int i = 0; i < allBlocks.Count; i++) {
    //        if (allBlocks[i].x == block.x && allBlocks[i].y == block.y) {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    private bool IsInsideMap (Vector2 block) {
        return ((block.x >= 0) && (block.x < mapSize.x)) && ((block.y >= 0) && (block.y < mapSize.y));
    }

    public void GenerateCave (int blockId, int voidBlockId, int steps, float density, FindNeighboursMode findMode = FindNeighboursMode.NEIGHBOURS_4, bool unique = false) {
        //Select the first point
        int randX = (int)(Random.value * mapSize.x);
        int randY = (int)(Random.value * mapSize.y);

        while (map[randX, randY] == 0) {
            randX = (int)(Random.value * mapSize.x);
            randY = (int)(Random.value * mapSize.y);
        }

        int uniqueId = 0;
        if (unique) {
            uniqueId = blockId;
            blockId = Random.Range(int.MaxValue-1000, int.MaxValue); 
        }

        //Create the first point
        mapGenerator.map[randX, randY] = blockId;

        //Create the cave
        for (int step = 0; step < steps; step++) {
            for (int x = 0; x < (int)mapSize.x; x++) {
                for (int y = 0; y < (int)mapSize.y; y++) {
                    if (map[x, y] != blockId && map[x,y] != voidBlockId) {
                        if (FindNeighbours(x, y, blockId, findMode) >= 1) {
                            if (Random.Range(0f, 1f) > density) {
                                map[x, y] = blockId;
                            }
                        }
                    }
                }
            }
        }

        if (unique) {
            for (int x = 0; x < (int)mapSize.x; x++) {
                for (int y = 0; y < (int)mapSize.y; y++) {
                    if (map[x, y] == blockId) {
                        map[x, y] = uniqueId;
                    }
                }
            }
        }

        mapGenerator.SetMap(map);
    }

    //Neighbours
    private int FindNeighbours (int x, int y, int blockId, FindNeighboursMode mode) {
        List<Vector2> neigh = new List<Vector2>();
        int count = 0;

        //Mode: 1 = 4 neighbours (Up, down, right, left)
        //Mode: 1 = 8 neighbours (Up, down, right, left, Up-Right, Up-Left, Down-Right, Down-Left)

        if (mode == FindNeighboursMode.NEIGHBOURS_4) { //4 Neightbours
            neigh.Add(new Vector2(x - 1, y));
            neigh.Add(new Vector2(x + 1, y));
            neigh.Add(new Vector2(x, y - 1));
            neigh.Add(new Vector2(x, y + 1));

        } else { //8 Neightbours
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (!(i == 0 && j == 0)) {
                        neigh.Add(new Vector2(x + i, y + j));
                    }
                }
            }
        }

        //Check for outside blocks and remove it from the list
        List<Vector2> newNeigh = new List<Vector2>();
        for (int i = 0; i < neigh.Count; i++) {
            if (IsInsideMap(neigh[i])) {
                newNeigh.Add(neigh[i]);
            }
        }
        neigh = newNeigh;

        //Count the neighbours
        for (int i = 0; i < neigh.Count; i++) {
            if (map[(int)neigh[i].x, (int)neigh[i].y] == blockId) {
                count++;
            }
        }

        return count;
    }

    public void FillGaps (int steps, int blockId, FindNeighboursMode findMode, int minNeighboursToFill) {
        map = mapGenerator.GetMap();

        for (int step = 0; step < steps; step++) {
            for (int x = 0; x < (int)mapSize.x; x++) {
                for (int y = 0; y < (int)mapSize.y; y++) {
                    if (map[x, y] != blockId) {
                        if (FindNeighbours(x, y, blockId, findMode) >= minNeighboursToFill) {
                            map[x, y] = blockId;
                        }
                    }
                }
            }
        }

        mapGenerator.SetMap(map);
    }
}
