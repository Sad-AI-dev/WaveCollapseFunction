using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveCollapseSolver3D : MonoBehaviour
{
    public WFCDataSet3D dataSet;

    [Header("Generation Settings")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Transform nodeHolder;
    [SerializeField] private Vector3Int gridSize;
    [SerializeField] private Vector3Int chunkSize;
    [SerializeField] private int maxFailNum = 100;
    [SerializeField] private float chunkGenerationDelay = 0.1f;

    [Header("Preview Settigns")]
    public Transform previewHolder;
    public Vector2 previewSpacing;

    //vars
    public GameObject[] LookupTable { get; private set; }
    private Node3D[][][] grid;
    private bool isGenerating;

    private int failCounter = 0;

    private void Start()
    {
        InitializeDataSet();
        Generate();
    }

    private void Update()
    {
        if (!isGenerating && Input.GetKeyDown(KeyCode.Space)) { Generate(); }
    }

    //========================= Generate Lookup Table =======================
    public void InitializeDataSet()
    {
        LookupTable = new GameObject[dataSet.nodes.Length];
        for (int i = 0; i < dataSet.nodes.Length; i++) {
            LookupTable[i] = dataSet.nodes[i].prefab;
        }
    }

    //========================= Wave Collapse Function Algorithm =============================
    private void Generate()
    {
        isGenerating = true;
        //create base case
        InitializeGrid();
        //start Generating chunks
        StartCoroutine(GenerateChunksCo());
    }

    private void InitializeGrid()
    {
        ClearGrid(); //make sure grid is empty
        grid = new Node3D[gridSize.x][][];

        for (int x = 0; x < gridSize.x; x++) {
            grid[x] = new Node3D[gridSize.y][]; //initialize grid var

            for (int y = 0; y < gridSize.y; y++) {
                grid[x][y] = new Node3D[gridSize.z]; //initialize grid var

                for (int z = 0; z < gridSize.z; z++) {
                    InitializeNode(new Vector3Int(x, y, z));
                }
            }
        }
    }
    private void InitializeNode(Vector3Int pos)
    {
        Node3D node = Instantiate(nodePrefab, nodeHolder).GetComponent<Node3D>();
        //set position
        node.transform.position = pos;
        //set vars
        node.Initialize(this);
        node.CreateNode(pos.y > 0 ? dataSet.airNode : dataSet.defaultFloorNode);
        node.posInGrid = pos;
        //record created node
        grid[pos.x][pos.y][pos.z] = node;
    }

    //============================== Generate Chunks ==================================
    private IEnumerator GenerateChunksCo()
    {
        for (int y = 0; y < gridSize.y; y += chunkSize.y - 1) {
            for (int x = 0; x < gridSize.x; x += chunkSize.x - 1) {
                for (int z = 0; z < gridSize.z; z += chunkSize.z - 1) {
                    yield return GenerateChunkCo(new Vector3Int(x, y, z));
                    yield return new WaitForSeconds(chunkGenerationDelay);
                }
            }
        }
        //done generating, clean up
        HandleGenerationEnd();
    }
    private void HandleGenerationEnd()
    {
        RemoveAirNodes(); //delete invisible objects
        isGenerating = false;
    }

    private IEnumerator GenerateChunkCo(Vector3Int chunkPos)
    {
        bool firstTry = true;
        //1: compile nodes in chunk
        Node3D[] contents = CompileChunkContents(chunkPos);
        //2: reset chunk
        ResetChunk(contents, chunkPos);
        yield return null; //test
        //3: recursize node collapse
        while (true) {
            List<Node3D> lowestEntropyTiles = GetLowestEntropyTiles(contents);
            //check fail condition
            if (lowestEntropyTiles[0].Entropy == 0) {
                if (firstTry) { failCounter = maxFailNum; }
                yield return HandleGenerationFailCo(contents, chunkPos);
            }
            else {
                CollapseRandomTile(lowestEntropyTiles);
                if (ChunkIsGenerated(contents)) {
                    break; //done generating chunk
                }
            }
            yield return null; //optional
            firstTry = false;
        }
        //4: reset for next chunk
        failCounter = 0;
    }

    private Node3D[] CompileChunkContents(Vector3Int chunkPos)
    {
        Node3D[] contents = new Node3D[chunkSize.x * chunkSize.y * chunkSize.z];
        int indexer = 0;
        //compile nodes in chunk
        for (int y = chunkPos.y; y < Mathf.Min(chunkPos.y + chunkSize.y, gridSize.y); y++) {
            for (int z = chunkPos.z; z < Mathf.Min(chunkPos.z + chunkSize.z, gridSize.z); z++) {
                for (int x = chunkPos.x; x < Mathf.Min(chunkPos.x + chunkSize.x, gridSize.x); x++) {
                    contents[indexer] = grid[x][y][z];
                    indexer++;
                }
            }
        }
        //return results, if part falls out of bounds, cull empty entries
        return indexer < contents.Length ? contents.Take(indexer).ToArray() : contents;
    }

    //====================== WFC Algorightm Helper Functions =============================
    private List<Node3D> GetLowestEntropyTiles(Node3D[] contents)
    {
        List<Node3D> lowestEntropyTiles = new List<Node3D>();
        int lowestFound = 1000; //unreasonably high number || don't pick element 0, tile may be collapsed

        for (int i = 0; i < contents.Length; i++) {
            if (!contents[i].isCollapsed) { //make sure tile is valid
                int curEntropy = contents[i].Entropy;
                if (curEntropy <= lowestFound) {
                    if (curEntropy < lowestFound) {
                        //lower entropy found, reset list
                        lowestEntropyTiles.Clear();
                        lowestFound = curEntropy;
                    }
                    //add tile to list
                    lowestEntropyTiles.Add(contents[i]);
                }
            }
        }
        return lowestEntropyTiles;
    }

    private IEnumerator HandleGenerationFailCo(Node3D[] contents, Vector3Int chunkPos)
    {
        ResetChunk(contents, chunkPos); //Tile has 0 possible states left, reset chunk
        failCounter++;
        yield return new WaitForSeconds(chunkGenerationDelay);
        if (failCounter >= maxFailNum) {
            failCounter = 0; //reset fail counter
            yield return GenerateChunkCo(GetLastChunkPos(chunkPos)); //regenerate last chunk
            ResetChunk(contents, chunkPos); //retry current chunk generation
        } 
    }
    private Vector3Int GetLastChunkPos(Vector3Int chunkPos)
    {
        if (chunkPos.x > 0) {
            chunkPos.x -= chunkSize.x - 1;
        }
        else if (chunkPos.z > 0) {
            chunkPos.z -= chunkSize.z - 1;
        }
        else if (chunkPos.y > 0) { //try move back down
            chunkPos.y -= chunkSize.y - 1;
        }
        return chunkPos;
    }

    private void CollapseRandomTile(List<Node3D> options)
    {
        Node3D pickedTile = options[Random.Range(0, options.Count)];
        //collapse Tile
        pickedTile.Collapse();
        //perpetuate
        for (int i = 0; i < 6; i++) { //perpetuate to all direct neighbours
            Vector3Int pos = pickedTile.posInGrid + DirUtil.DirToV3((Direction)i);
            //bounds check
            if (PosIsInBounds(pos)) {
                grid[pos.x][pos.y][pos.z].Perpetuate(dataSet.nodes[pickedTile.id], (Direction)i);
            }
        }
    }
    private bool PosIsInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x
            && pos.y >= 0 && pos.y < gridSize.y
            && pos.z >= 0 && pos.z < gridSize.z;
    }

    private bool ChunkIsGenerated(Node3D[] contents)
    {
        for (int i = 0; i < contents.Length; i++) {
            if (!contents[i].isCollapsed) {
                return false;
            }
        }
        return true;
    }
    //========================= Clear / Reset Functions =======================
    private void ClearGrid()
    {
        for (int i = nodeHolder.childCount - 1; i >= 0; i--) {
            Destroy(nodeHolder.GetChild(i).gameObject);
        }
    }

    private void RemoveAirNodes()
    {
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                for (int z = 0; z < gridSize.z; z++) {
                    //check if position is air
                    if (grid[x][y][z].id == dataSet.airNode) {
                        Destroy(grid[x][y][z].gameObject); //destroy air objects
                    }
                }
            }
        }
    }

    //============================ Clear / Reset Chunk Functions ================================
    private void ResetChunk(Node3D[] contents, Vector3Int chunkPos)
    {
        foreach (Node3D node in contents) {
            node.ResetTile(); //reset to uncollapsed state
            //node is on edge, perpetuate values from generated bordered nodes
            if (IsEdgePos(node.posInGrid, chunkPos)) {
                PerpetuateFromChunkEdge(node, chunkPos);
            }
        }
    }
    //======== Chunk Reset helper functions =================
    private bool IsEdgePos(Vector3Int pos, Vector3Int chunkPos)
    {
        Vector3 endPos = chunkPos + chunkSize;
        return pos.x == chunkPos.x || pos.y == chunkPos.y || pos.z == chunkPos.z ||
            pos.x == endPos.x || pos.y == endPos.y || pos.z == endPos.z;
    }
    private void PerpetuateFromChunkEdge(Node3D node, Vector3Int chunkPos) 
    {
        List<Direction> edgeDirs = GetEdgeDirections(node.posInGrid, chunkPos);
        foreach (Direction dir in edgeDirs) {
            Vector3Int edgeNodePos = node.posInGrid + DirUtil.DirToV3(dir);
            if (PosIsInBounds(edgeNodePos) && grid[edgeNodePos.x][edgeNodePos.y][edgeNodePos.z].isCollapsed) {
                node.Perpetuate(dataSet.nodes[grid[edgeNodePos.x][edgeNodePos.y][edgeNodePos.z].id], DirUtil.GetOpposite(dir));
            }
        }
    }
    private List<Direction> GetEdgeDirections(Vector3Int pos, Vector3Int chunkPos) 
    {
        List<Direction> dirs = new();
        Vector3Int endPos = chunkPos + chunkSize - new Vector3Int(1, 1, 1);

        if (pos.x == chunkPos.x) { dirs.Add(Direction.west); }
        if (pos.y == chunkPos.y) { dirs.Add(Direction.down); }
        if (pos.z == chunkPos.z) { dirs.Add(Direction.south); }
        if (pos.x == endPos.x) { dirs.Add(Direction.east); }
        if (pos.y == endPos.y) { dirs.Add(Direction.up); }
        if (pos.z == endPos.z) { dirs.Add(Direction.north); }

        return dirs;
    }
}
