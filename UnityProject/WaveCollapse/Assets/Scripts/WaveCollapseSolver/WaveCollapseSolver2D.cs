using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveCollapseSolver2D : MonoBehaviour
{
    public WFCDataSet2D dataSet;

    [Header("Generation Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform tileHolder;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private Vector2Int chunkSize;
    [SerializeField] private float chunkGenerationDelay = 0.1f;

    [Header("Preview Settigns")]
    public Transform previewHolder;
    public Vector2 previewSpacing;

    //vars
    public GameObject[] LookupTable { get; private set; }
    private Tile2D[][] grid;
    private bool isGenerating;

    private void Start()
    {
        GenerateLookupTable();
        Generate();
    }

    private void Update()
    {
        if (!isGenerating && Input.GetKeyDown(KeyCode.Space)) { Generate(); }
    }

    //=========== Initialize Vars =================
    public void GenerateLookupTable()
    {
        LookupTable = new GameObject[dataSet.tiles.Length];
        for (int i = 0; i < dataSet.tiles.Length; i++) {
            LookupTable[i] = dataSet.tiles[i].prefab;
        }
    }

    //========================== Wave Function Collapse Algorithm ================================
    public void Generate()
    {
        isGenerating = true;
        InitializeGrid();
        StartCoroutine(GenerateChunksCo());
    }

    private void InitializeGrid()
    {
        ClearGrid(); //make sure grid is empty

        grid = new Tile2D[gridSize.x][];
        //fill grid with default tile
        for (int i = 0; i < gridSize.x; i++) {
            grid[i] = new Tile2D[gridSize.y];

            for (int j = 0; j < gridSize.y; j++) {
                Transform tile = Instantiate(tilePrefab, tileHolder).transform;
                tile.position = new Vector3(i, j, 0);
                Tile2D tileData = tile.GetComponent<Tile2D>();
                tileData.Initialize(this);
                grid[i][j] = tileData; //store entry in grid
                tileData.posInGrid = new Vector2Int(i, j); //tell tile where it is in grid
            }
        }
    }
    private void ClearGrid()
    {
        for (int i = tileHolder.childCount - 1; i >= 0; i--) {
            Destroy(tileHolder.GetChild(i).gameObject);
        }
    }

    private IEnumerator GenerateChunksCo()
    {
        for (int i = 0; i < gridSize.x; i += chunkSize.x - 1) {
            for (int j = 0; j < gridSize.y; j += chunkSize.y - 1) {
                yield return GenerateChunk(new Vector2Int(i, j));
                yield return new WaitForSeconds(chunkGenerationDelay);
            }
        }
        //generation finished
        isGenerating = false;
    }

    //================= Chunk Generation ======================
    private IEnumerator GenerateChunk(Vector2Int chunkPos)
    {
        //step 1, compile chunk contents
        Tile2D[] chunkContents = CompileChunkContents(chunkPos);
        //step 2, reset chunk
        ResetChunk(chunkContents, chunkPos);
        //step 3, recursive content creation
        while (true) {
            List<Tile2D> lowestEntropyTiles = GetLowestEntropyTiles(chunkContents);
            //check error condition
            if (lowestEntropyTiles[0].Entropy == 0) {
                ResetChunk(chunkContents, chunkPos); //Tile has 0 possible states left, retry entire chunk
                yield return new WaitForSeconds(chunkGenerationDelay);
                continue;
            }
            //collapse random tile
            CollapseRandomTile(lowestEntropyTiles);
            //done check
            if (ChunkIsDoneGenerating(chunkContents)) {
                break; //exit loop
            }

            //optional
            yield return null;
        }
    }

    private Tile2D[] CompileChunkContents(Vector2Int chunkPos)
    {
        Tile2D[] contents = new Tile2D[chunkSize.x * chunkSize.y];

        int indexer = 0;
        for (int i = chunkPos.x; i < Mathf.Min(chunkPos.x + chunkSize.x, gridSize.x); i++) {
            for (int j = chunkPos.y; j < Mathf.Min(chunkPos.y + chunkSize.y, gridSize.y); j++) {
                contents[indexer] = grid[i][j];
                indexer++;
            }
        }
        
        //return results
        if (indexer < contents.Length) { //part of chunk is out of bounds, remove empty slots in array
            return contents.Take(indexer).ToArray();
        }
        else { 
            return contents;
        }
    }

    private void ResetChunk(Tile2D[] chunkContent, Vector2Int chunkPos)
    {
        foreach (Tile2D tile in chunkContent) {
            tile.ResetTile();
        }
        //perpetuate chunk edges
        int checkLevel = chunkPos.y + chunkSize.y + 1;
        //top bound
        if (checkLevel < gridSize.y) {
            for (int i = chunkPos.x; i < Mathf.Min(chunkPos.x + chunkSize.x, gridSize.x); i++) {
                WFCTileData2D checkTileData = dataSet.tiles[grid[i][checkLevel].id];
                grid[i][checkLevel - 1].Perpetuate(checkTileData, Direction.south);
            }
        }
        //right bound
        checkLevel = chunkPos.x + chunkSize.x + 1;
        if (checkLevel < gridSize.x) {
            for (int i = chunkPos.y; i < Mathf.Min(chunkPos.y + chunkSize.y, gridSize.y); i++) {
                WFCTileData2D checkTileData = dataSet.tiles[grid[checkLevel][i].id];
                grid[checkLevel - 1][i].Perpetuate(checkTileData, Direction.west);
            }
        }
        //bottom bound
        checkLevel = chunkPos.y - 1;
        if (checkLevel >= 0) {
            for (int i = chunkPos.x; i < Mathf.Min(chunkPos.x + chunkSize.x, gridSize.x); i++) {
                WFCTileData2D checkTileData = dataSet.tiles[grid[i][checkLevel].id];
                grid[i][checkLevel + 1].Perpetuate(checkTileData, Direction.north);
            }
        }
        //left bound
        checkLevel = chunkPos.x - 1;
        if (checkLevel >= 0) {
            for (int i = chunkPos.y; i < Mathf.Min(chunkPos.y + chunkSize.y, gridSize.y); i++) {
                WFCTileData2D checkTileData = dataSet.tiles[grid[checkLevel][i].id];
                grid[checkLevel + 1][i].Perpetuate(checkTileData, Direction.east);
            }
        }
    }

    //================== Wave Collapse ====================
    private List<Tile2D> GetLowestEntropyTiles(Tile2D[] contents)
    {
        List<Tile2D> lowestEntropyTiles = new List<Tile2D>();
        int lowestFound = 1000; //unreasonably high number || don't pick element 0, tile may be collapsed

        for (int i = 0; i < contents.Length; i++) {
            if (!contents[i].isCollapsed) { //make sure tile is valid
                int curEntropy = contents[i].Entropy;
                if (curEntropy < lowestFound)
                {
                    //lower entropy found, reset list
                    lowestEntropyTiles.Clear();
                    lowestFound = contents[i].Entropy;

                    lowestEntropyTiles.Add(contents[i]);
                }
                else if (curEntropy == lowestFound)
                {
                    //current is also lowest entropy found so far
                    lowestEntropyTiles.Add(contents[i]);
                }
            }
        }
        return lowestEntropyTiles;
    }

    private void CollapseRandomTile(List<Tile2D> options)
    {
        Tile2D pickedTile = options[Random.Range(0, options.Count)];
        //collapse Tile
        pickedTile.Collapse();
        //perpetuate
        for (int i = 0; i < 4; i++) { //perpetuate to all direct neighbours
            Vector2Int pos = pickedTile.posInGrid + DirUtil.DirToV2((Direction)i);
            //bounds check
            if (PosIsInBounds(pos)) {
                grid[pos.x][pos.y].Perpetuate(dataSet.tiles[pickedTile.id], (Direction)i);
            }
        }
    }
    private bool PosIsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x
            && pos.y >= 0 && pos.y < gridSize.y;
    }

    private bool ChunkIsDoneGenerating(Tile2D[] contents)
    {
        for (int i = 0; i < contents.Length; i++) {
            if (!contents[i].isCollapsed) {
                return false;
            }
        }
        return true;
    }
}
