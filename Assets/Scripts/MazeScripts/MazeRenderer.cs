using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField]
    [Range(1, 50)]
    private int width = 10;
    [SerializeField]
    [Range(1, 50)]
    private int height = 10;
    [SerializeField]
    private float size = 1f;

    [SerializeField]
    private Transform wallPrefab;

    [SerializeField]
    private Transform _ground;

    [SerializeField]
    private Transform _destinationColor;

    [SerializeField]
    private Transform _player;

    public static MazeRenderer Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        var maze = MazeGenerator.Generate(width, height);
       
        if ((height + width) / 3 >= 28)
            Camera.main.orthographicSize = 28;
        else
            Camera.main.orthographicSize = (height + width) / 3;

        Camera.main.orthographic = true;

        Draw(maze);

        var instantiatedPlayer = Instantiate(_player) as Transform;

        MovementManager.Instance.SetMaze(maze, width, height);

        MovementManager.Instance.SetPlayer(instantiatedPlayer);
    }

    public void DrawDestinationPoint(MazeNode node)
    {
        var ground = Instantiate(_destinationColor, transform) as Transform;
        ground.name = "Path";
        ground.localScale = new Vector3(0.06f, 1, 0.06f);        
        ground.localPosition = node.NodeCenter;
    }

    public void Draw(MazeNode[,] maze)
    {
        var ground = Instantiate(_ground, transform) as Transform;
        ground.localScale = new Vector3(width/5, 1, height/5);
        ground.localPosition = new Vector3(-.5f, 0, -.5f);

        for (var i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = maze[i, j].WallState;
                var position = new Vector3(-width / 2 + i, 0, -height / 2 + j);

                if (cell.HasFlag(WallState.UP))
                {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0, 0, size / 2);
                    topWall.localScale = new Vector3(size, topWall.localScale.y, topWall.localScale.z);
                }
                if (cell.HasFlag(WallState.LEFT))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size / 2, 0, 0);
                    leftWall.localScale = new Vector3(size, leftWall.localScale.y, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0, 90, 0);
                }
                if (i == width - 1)
                {
                    if (cell.HasFlag(WallState.RIGHT))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(size / 2, 0, 0);
                        rightWall.localScale = new Vector3(size, rightWall.localScale.y, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }
                if (j == 0)
                {
                    if (cell.HasFlag(WallState.DOWN))
                    {
                        var downWall = Instantiate(wallPrefab, transform) as Transform;
                        downWall.position = position + new Vector3(0, 0, -size / 2);
                        downWall.localScale = new Vector3(size, downWall.localScale.y, downWall.localScale.z);
                    }
                }
            }
        }
    }
}
