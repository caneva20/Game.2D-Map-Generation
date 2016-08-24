using UnityEngine;
using System.Collections;

public class PhysicsManager {
    public static void Simulate (int[,] map, int nullBlock) {
        int lastCount = 1;

        while (lastCount > 0) {
            lastCount = 0;
            for (int x = 0; x < map.GetLength(0); x++) {
                for (int y = 1; y < map.GetLength(1); y++) {
                    if (map[x,y] != nullBlock) {
                        if (map[x, y - 1] == nullBlock) {
                            map[x, y - 1] = map[x, y];
                            map[x, y] = nullBlock;

                            lastCount++;
                        }
                    }
                }
            }
        }
    }
}
