using UnityEngine;
using DUJAL.DungeonCreator;
using DUJAL.DungeonCreator.Types;
using DUJAL.Systems.Loading;

public class TestDungeon : MonoBehaviour
{
    private SquaredDungeon squareDungeon;
    private Labyrinth labyrinth;

    public int labyrinthSize = 65;
    public int dungeonSize = 16;
    public int dungeonRooms = 8;
    
    private void Start()
    {
        GeneratePrimDungeons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GeneratePrimDungeons();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GenerateDFSDungeons();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GeneratePrimLabyrinths();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateDFSLabyrinths();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneLoader.Instance.LoadScene(SceneIndex.ExampleScene1, 1);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SceneLoader.Instance.LoadScene(SceneIndex.ExampleScene2, 1);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            SceneLoader.Instance.LoadScene(SceneIndex.ExampleScene3, 1);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            SceneLoader.Instance.LoadScene(SceneIndex.ExampleScene4, 1);
        }
    }

    [ContextMenu("GeneratePrimDungeons")]
    public void GeneratePrimDungeons()
    {
        Clean();

        squareDungeon = new(dungeonSize);
        float startingCoord = dungeonSize / 2.0f;
        squareDungeon.Generate(new Vector2Int((int)startingCoord, (int)startingCoord), dungeonRooms, GenerationAlgorithm.Prim);

        var squareTiles = squareDungeon.GetTileDescription();
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = new Vector3(startingCoord, -1.0f, startingCoord);
        floor.transform.localScale = new Vector3(labyrinthSize * 2, 1, labyrinthSize * 2);

        Camera.main.transform.position = new Vector3(startingCoord, Camera.main.transform.position.y, startingCoord);
        for (int i = 0; i < squareDungeon.Size; i++)
        {
            for (int j = 0; j < squareDungeon.Size; j++)
            {
                if (squareTiles[i, j] != 0)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.GetComponent<MeshRenderer>().material.color = Color.green;
                    cube.transform.position = new Vector3(i, 0, j);
                    cube.name = "Prim Dungeon: (" + i + ", " + j + ")";
                }
            }
        }
    }

    [ContextMenu("GeneratePrimDungeons")]
    public void GenerateDFSDungeons()
    {
        Clean();
        float startingCoord = dungeonSize / 2.0f;
        squareDungeon = new (dungeonSize);
        squareDungeon.Generate(new Vector2Int((int) startingCoord, (int) startingCoord), dungeonRooms, GenerationAlgorithm.DFS);
        
        var squareTiles = squareDungeon.GetTileDescription();
        
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = new Vector3(startingCoord, -1.0f, startingCoord);
        floor.transform.localScale = new Vector3(labyrinthSize * 2, 1, labyrinthSize * 2);

        squareTiles = squareDungeon.GetTileDescription();
        for (int i = 0; i < squareDungeon.Size; i++)
        {
            for (int j = 0; j < squareDungeon.Size; j++)
            {
                if (squareTiles[i, j] != 0)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.GetComponent<MeshRenderer>().material.color = Color.red;
                    cube.transform.position = new Vector3(i, 0, j);
                    cube.name = "DFS Dungeon: (" + i + ", " + j + ")";
                }
            }
        }   
    }

    [ContextMenu("GeneratePrimLabyrinths")]
    public void GeneratePrimLabyrinths()
    {
        Clean();
        labyrinth = new(labyrinthSize);
        float startingCoord = labyrinthSize / 2.0f;
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = new Vector3(startingCoord * 2, -1.0f, startingCoord * 2);
        floor.transform.localScale = new Vector3(labyrinthSize * 2 + 1, 1, labyrinthSize * 2 + 1);

        labyrinth.Generate(new Vector2Int((int)startingCoord, (int)startingCoord), GenerationAlgorithm.Prim);

        var labyrinthTiles = labyrinth.GetTileDescription(out int size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (labyrinthTiles[i, j] != 0)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(i, 0.5f, j);
                    cube.transform.localScale = new Vector3(cube.transform.localScale.x, 2, cube.transform.localScale.z);
                    cube.name = "Prim Labyrinth: (" + i + ", " + j + ")";
                }
            }
        }
    }

    [ContextMenu("GenerateDFSLabyrinths")]
    public void GenerateDFSLabyrinths()
    {
        Clean();
        labyrinth = new(labyrinthSize);
        float startingCoord = labyrinthSize / 2.0f;

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.position = new Vector3(startingCoord * 2, -1.0f, startingCoord * 2);
        floor.transform.localScale = new Vector3(labyrinthSize * 2 + 1, 1, labyrinthSize * 2 + 1);

        labyrinth.Generate(new Vector2Int((int)startingCoord, (int)startingCoord), GenerationAlgorithm.DFS);
        
        var labyrinthTiles = labyrinth.GetTileDescription(out int size);

        labyrinthTiles = labyrinth.GetTileDescription(out size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (labyrinthTiles[i, j] != 0)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.GetComponent<MeshRenderer>().material.color = Color.red;
                    cube.transform.localScale = new Vector3(cube.transform.localScale.x, 2, cube.transform.localScale.z);
                    cube.transform.position = new Vector3(i, 0.0f, j);
                    cube.name = "DFS Dungeon: (" + i + ", " + j + ")";
                }
            }
        }
    }

    [ContextMenu("Clean")]
    public void Clean() 
    {
        var objects = FindObjectsOfType<BoxCollider>();
        foreach (var obj in objects) { Destroy(obj.gameObject); }
    }
}
