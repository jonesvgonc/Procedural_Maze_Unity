using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField]
    private float _speed = 6.0f;
    private MazeNode[,] _maze;
    private int _mazeHeight;
    private int _mazeWidth;
    private Stack<MazeNode> _route;
    [SerializeField]
    private bool _canMove = false;
    [SerializeField]
    private Vector3 _direction;
    [SerializeField]
    private bool _moving = false;
    private Transform _player;
    [SerializeField]
    private Vector3 _destination;

    public static MovementManager Instance;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    public void SetMaze(MazeNode[,] maze, int width, int height)
    {
        _maze = maze;
        _mazeHeight = height;
        _mazeWidth = width;
    }

    public void SetPlayer(Transform player)
    {
        _player = player;
        _player.position = new Vector3(_maze[0, 0].NodeCenter.x, 0.5f, _maze[0, 0].NodeCenter.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_canMove && !_moving)
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            var destination = GetMazeNodeDestination(hit.point);
            MazeRenderer.Instance.DrawDestinationPoint(destination);

            _route = SetBestRoute(destination);
            _canMove = true;
        }else
        if(_canMove && !_moving)
        {
            if(_route.Count() > 0)
            {
                var nextNode = _route.Pop();
                _direction = nextNode.NodeCenter - _player.position;
                _destination = nextNode.NodeCenter;
                _moving = true;
            }else
            {
                _canMove = false;
            }
        }else
        if(_moving)
        {
            _player.position += _direction * Time.deltaTime * _speed;

            if(Vector3.Distance(_player.position, _destination) < 0.2f)
            {
                _moving = false;
            }
        }
    }

    public Stack<MazeNode> SetBestRoute(MazeNode destination)
    {
        Stack<MazeNode> route = new Stack<MazeNode>();
        var pos = StartNode(_player.position);
        
        route.Push(_maze[pos.X, pos.Y]);
        var makeRoute = MakeRoute(_maze[pos.X, pos.Y], destination, WallState.NONE, ref route);
        var bestRoute = new Stack<MazeNode>();
        //Draw Path
        if (_player != null)
        {
            ClearDestinationPath();
            while (route.Count() > 0)
            {
                var path = route.Pop();
                MazeRenderer.Instance.DrawDestinationPoint(path);
                bestRoute.Push(path);
            }
        }
        return bestRoute;
    }

    private Position StartNode(Vector3 playerPos)
    {
        var indexX = Mathf.RoundToInt(playerPos.x);
        if (indexX < 0)
        {
            indexX = indexX + ((_mazeWidth / 2));
        }
        else
        {
            indexX += (_mazeWidth / 2);
        }
        var indexY = Mathf.RoundToInt(playerPos.z);
        if (indexY < 0)
        {
            indexY = indexY + ((_mazeHeight / 2));
        }
        else
        {
            indexY += (_mazeHeight / 2);
        }

        return new Position() { X = indexX, Y = indexY };
    }

    private void ClearDestinationPath()
    {
        var objectsPath = GameObject.FindGameObjectsWithTag("Path");
        foreach(var path in objectsPath)
        {
            Destroy(path);
        }
    }

    private bool MakeRoute(MazeNode node, MazeNode destination, WallState comeFrom, ref Stack<MazeNode> route)
    {
        var neighbours = MazeGenerator.GetOpenNeighbours(node.NodePosition, _maze, _mazeWidth, _mazeHeight);

        if (node.NodePosition.X == destination.NodePosition.X && node.NodePosition.Y == destination.NodePosition.Y)
        {
            return true;
        }

        if (neighbours.Count() > 1 || neighbours[0].SharedWall != comeFrom)
        {
            foreach (var neighbour in neighbours)
            {                
                if (neighbour.SharedWall != comeFrom)
                {
                    var nextnode = _maze[neighbour.Position.X, neighbour.Position.Y];
                    route.Push(nextnode);
                    if (MakeRoute(nextnode, destination, MazeGenerator.GetOppositeWall(neighbour.SharedWall), ref route))
                    {
                       return true;
                    }
                }
            }
        }        
        route.Pop();
        return false;
    }

    private MazeNode GetMazeNodeDestination(Vector3 destinationPoint)
    {
        var node = new MazeNode();

        var indexX = Mathf.RoundToInt(destinationPoint.x);
        
            indexX = indexX + ((_mazeWidth / 2));
        
        var indexY = Mathf.RoundToInt(destinationPoint.z);
        if(indexY < 0)
        {
            indexY = indexY + ((_mazeHeight / 2));
        }
        else
        {
            indexY += (_mazeHeight / 2);
        }

        node = _maze[indexX, indexY];

        return node;
    }
}
