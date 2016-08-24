using UnityEngine;
using System.Collections;

public enum DestroyMode {
    CIRCLE,
    SQUARE
}

public class MapDestroyer {
    private MapGenerator mapGenerator;
    private Vector2 mapSize;
    private int[,] map;

    public MapDestroyer (MapGenerator mapGenerator) {
        this.mapGenerator = mapGenerator;
        mapSize = mapGenerator.mapSize;
        map = mapGenerator.GetMap();
    }

    public void Destroy (Vector2 block, int range, DestroyMode destroyMode, int newBlockId) {
        if (destroyMode == DestroyMode.CIRCLE) {
            for (int x = 0; x < (int)mapSize.x; x++) {
                for (int y = 0; y < (int)mapSize.y; y++) {
                    if (Vector2.Distance(new Vector2(x, y), block) <= range) {
                        map[x, y] = newBlockId;
                    }
                }
            }
        } else if (destroyMode == DestroyMode.SQUARE) {
            range /= 2;

            for (int x = 0; x < (int)mapSize.x; x++) {
                for (int y = 0; y < (int)mapSize.y; y++) {
                    if (((x >= block.x - range) && (x <= block.x + range)) && ((y >= block.y - range) && (y <= block.y + range))) {
                        map[x, y] = newBlockId;
                    }
                }
            }
        }

        mapGenerator.SetMap(map);
    }
}
