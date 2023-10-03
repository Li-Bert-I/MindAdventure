using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.DateTime;
using static System.String;

public class Maze : MonoBehaviour
{
	public MazeCell cellPrefab;
    public MazeFloor floorPrefab;
    public int sizeMazeR;
    public int outSizeMaze;
    public float noizeScale;
    public float maxShift;
    public float noiseThreshold;
    public int chunkSize;
    
    private int sizeX, sizeZ;
	private MazeCell[,] cells;
    public Vector3 scale;
    private int sizeChunkX, sizeChunkZ;
    private int[,] chuncks;
    private int[,] mazeScheme;
    private MazeFloor floor;
    private Pathfinding pathfinding;

    public void Generate() {
        sizeX = (sizeMazeR + outSizeMaze) * 2 + 1;
        sizeZ = (sizeMazeR + outSizeMaze) * 2 + 1;

        Random.InitState(System.DateTime.Now.Second);
        scale = cellPrefab.GetComponent<Renderer>().bounds.size;

        floor = Instantiate(floorPrefab) as MazeFloor;
        floor.transform.parent = transform;
        floor.transform.localScale = 0.1f * Vector3.Scale(new Vector3(sizeX, 1, sizeZ), scale);

        sizeChunkX = (int)(sizeX / chunkSize);
        sizeChunkZ = (int)(sizeZ / chunkSize);
        chuncks = new int[sizeChunkX, sizeChunkZ];

        mazeScheme = new int[sizeX, sizeZ];
        for (int i = 0; i < sizeX; ++i) {
            for (int j = 0; j < sizeZ; ++j) {
                mazeScheme[i, j] = -1;
            }
        }

        DFSMaze(sizeMazeR + outSizeMaze, sizeMazeR + outSizeMaze);
        PerlinMaze();

        cells = new MazeCell[sizeX, sizeZ];
        for (int i = 0; i < sizeX; ++i) {
            for (int j = 0; j < sizeZ; ++j) {
                if ((i == (sizeMazeR * 2 + outSizeMaze) || i == outSizeMaze) && j <= (sizeMazeR * 2 + outSizeMaze) && j >= outSizeMaze
                    || (j == (sizeMazeR * 2 + outSizeMaze) || j == outSizeMaze) && i <= (sizeMazeR * 2 + outSizeMaze) && i >= outSizeMaze) {
                    mazeScheme[i, j] = 1;
                }
                if (mazeScheme[i, j] == -1) {
                    mazeScheme[i, j] = 1;
                }
            }
        }

        pathfinding = new Pathfinding(sizeX, sizeZ);
        
        for (int x = 0; x < sizeX; ++x) {
            for (int z = 0; z < sizeZ; ++z) {
                if (mazeScheme[x, z] == 1) {
                    pathfinding.GetGrid().GetGridObject(x, z).SetIsWalkable(false);
                }
            }
        }
    }

    private void PerlinMaze()
    {
        Vector2 randomShift = Random.insideUnitCircle.normalized * Random.Range(-maxShift, maxShift);
        for (int i = 0; i < sizeX; ++i) {
            for (int j = 0; j < sizeZ; ++j) {
                if (Mathf.PerlinNoise(noizeScale * i + randomShift.x, noizeScale * j + randomShift.y) < noiseThreshold && mazeScheme[i, j] == -1) {
                    mazeScheme[i, j] = 0;
                }
            }
        }

    }

    private void DFSMaze(int x, int z)
    {
        List<int[]> stack = new List<int[]>();
        List<int[]> neighbours = new List<int[]>();
        List<int[]> neighboursUnvisited = new List<int[]>();

        stack.Add(new int[2]{x, z});
        mazeScheme[x, z] = 0;

        int numStack = 1;
        int iters = sizeX * sizeZ;

        while (numStack > 0 && iters > 0) {
            iters -= 1;
            int[] frontier = stack[numStack - 1];
            stack.RemoveAt(numStack - 1);
            numStack -= 1;

            neighbours.Add(new int[2]{frontier[0] + 2, frontier[1]});
            neighbours.Add(new int[2]{frontier[0] - 2, frontier[1]});
            neighbours.Add(new int[2]{frontier[0], frontier[1] + 2});
            neighbours.Add(new int[2]{frontier[0], frontier[1] - 2});
            foreach(int[] neighbour in neighbours) {
                if (neighbour[0] >= sizeX || neighbour[1] >= sizeZ || neighbour[0] < 0 || neighbour[1] < 0) {
                    continue;
                }
                if (mazeScheme[neighbour[0], neighbour[1]] == -1) {
                    neighboursUnvisited.Add(neighbour);
                    continue;
                }
            }

            neighbours.Clear();

            int numNeighboursUnvisited = neighboursUnvisited.Count;
            if (numNeighboursUnvisited > 0) {
                stack.Add(frontier);
                numStack += 1;
                int[] neighbourUnvisited = neighboursUnvisited[Random.Range(0, numNeighboursUnvisited)];
                mazeScheme[(neighbourUnvisited[0] + frontier[0]) / 2, (neighbourUnvisited[1] + frontier[1]) / 2] = 0;
                mazeScheme[neighbourUnvisited[0], neighbourUnvisited[1]] = 0;
                stack.Add(neighbourUnvisited);
                numStack += 1;
            }
            neighboursUnvisited.Clear();
        }
    }

    private void PrimsMaze(int x, int z) 
    {
        List<int[]> frontiers = new List<int[]>();
        List<int[]> neighbours = new List<int[]>();
        List<int[]> neighbourPassages = new List<int[]>();

        frontiers.Add(new int[2]{x, z});

        int numFrontiers = frontiers.Count;
        int iters = sizeX * sizeZ;

        while (numFrontiers > 0 && iters > 0) {
            iters -= 1;
            int frontierI = Random.Range(0, numFrontiers);
            int[] frontier = frontiers[frontierI];

            frontiers.RemoveAt(frontierI);
            mazeScheme[frontier[0], frontier[1]] = 0;

            neighbours.Add(new int[2]{frontier[0] + 2, frontier[1]});
            neighbours.Add(new int[2]{frontier[0] - 2, frontier[1]});
            neighbours.Add(new int[2]{frontier[0], frontier[1] + 2});
            neighbours.Add(new int[2]{frontier[0], frontier[1] - 2});
            foreach(int[] neighbour in neighbours) {
                if (neighbour[0] >= sizeX || neighbour[1] >= sizeZ || neighbour[0] < 0 || neighbour[1] < 0) {
                    continue;
                }
                if (mazeScheme[neighbour[0], neighbour[1]] == -1) {
                    frontiers.Add(neighbour);
                    continue;
                }
                if (mazeScheme[neighbour[0], neighbour[1]] == 0) {
                    neighbourPassages.Add(neighbour);
                    continue;
                }
            }
            neighbours.Clear();

            int numNeighbourPassages = neighbourPassages.Count;
            if (numNeighbourPassages > 0) {
                int[] neighbourPassage = neighbourPassages[Random.Range(0, numNeighbourPassages)];
                mazeScheme[(neighbourPassage[0] + frontier[0]) / 2, (neighbourPassage[1] + frontier[1]) / 2] = 0;
            }
            neighbourPassages.Clear();

            numFrontiers = frontiers.Count;
        }
    }

    private MazeCell CreateCell(int x, int z) {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        newCell.name = "Maze Cell " + (x + (sizeMazeR + outSizeMaze)) + ", " + (z + (sizeMazeR + outSizeMaze));
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(x * scale.x, scale.y * 0.5f, z * scale.z);
        return newCell;
    }

    public void StartLoadChunks()
    {
        for (int chunkI = 0; chunkI < sizeChunkX; ++chunkI) {
            for (int chunkJ = 0; chunkJ < sizeChunkZ; ++chunkJ) {
                if (chuncks[chunkI, chunkJ] == 1) {
                    chuncks[chunkI, chunkJ] = -1;
                }
            }
        }
    }

    public void LoadChunks(Vector3 loaderPosistion)
    {   
        int chunkX = (int)((loaderPosistion.x + sizeX * scale.x * 0.5f) / chunkSize);
        int chunkZ = (int)((loaderPosistion.z + sizeZ * scale.z * 0.5f) / chunkSize);
        for (int chunkI = 0; chunkI < sizeChunkX; ++chunkI) {
            for (int chunkJ = 0; chunkJ < sizeChunkZ; ++chunkJ) {
                if (chunkI >= chunkX - 1 && chunkI <= chunkX + 1 && chunkJ >= chunkZ - 1 && chunkJ <= chunkZ + 1) {
                    if (chuncks[chunkI, chunkJ] == 0) {
                        for (int i = Mathf.Max(chunkI * chunkSize, 0); i < Mathf.Min((chunkI + 1) * chunkSize, sizeX); ++i) {
                            for (int j = Mathf.Max(chunkJ * chunkSize, 0); j < Mathf.Min((chunkJ + 1) * chunkSize, sizeZ); ++j) {
                                if (mazeScheme[i, j] == 1) {
                                    cells[i, j] = CreateCell(i - (sizeMazeR + outSizeMaze), j - (sizeMazeR + outSizeMaze));
                                }
                            }
                        }
                    }
                    chuncks[chunkI, chunkJ] = 1;
                }
            }
        }
    }

    public void UnLoadChunks()
    {
        for (int chunkI = 0; chunkI < sizeChunkX; ++chunkI) {
            for (int chunkJ = 0; chunkJ < sizeChunkZ; ++chunkJ) {
                if (chuncks[chunkI, chunkJ] == -1) {
                    for (int i = Mathf.Max(chunkI * chunkSize, 0); i < Mathf.Min((chunkI + 1) * chunkSize, sizeX); ++i) {
                        for (int j = Mathf.Max(chunkJ * chunkSize, 0); j < Mathf.Min((chunkJ + 1) * chunkSize, sizeZ); ++j) {
                            if (mazeScheme[i, j] == 1) {
                                Destroy(cells[i, j].gameObject);
                                cells[i, j] = null;
                            }
                        }
                    }
                    chuncks[chunkI, chunkJ] = 0;
                }
            }
        }
    }

    public Vector3 GetClosestPassage(Vector3 position)
    {
        int i = (int)(position.x + sizeX * scale.x * 0.5f);
        int j = (int)(position.z + sizeZ * scale.z * 0.5f);
        
        while (mazeScheme[i, j] != 0) {
            i += Random.Range(-1, 2);
            j += Random.Range(-1, 2);
        }

        return new Vector3((i - (sizeMazeR + outSizeMaze)) * scale.x, position.y, (j - (sizeMazeR + outSizeMaze)) * scale.z) + transform.position;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        int startI = (int)(start.x + sizeX * scale.x * 0.5f);
        int startJ = (int)(start.z + sizeZ * scale.z * 0.5f);
        int endI = (int)(end.x + sizeX * scale.x * 0.5f);
        int endJ = (int)(end.z + sizeZ * scale.z * 0.5f);

        List<PathNode> path = new List<PathNode>();
        pathfinding.FindPath(startI, startJ, endI, endJ, ref path);
        List<Vector3> result = new List<Vector3>();
        foreach (PathNode pathNode in path) {
            result.Add(new Vector3((pathNode.x - (sizeMazeR + outSizeMaze)) * scale.x, start.y, (pathNode.y - (sizeMazeR + outSizeMaze)) * scale.z));
        }

        return result;
    }
}
