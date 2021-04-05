using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum WallState
{
    LEFT = 1 << 0,
    RIGHT = 1 << 1,
    UP = 1 << 2,
    DOWN = 1 << 3,
    NONE = 1 << 4,
    VISITED = 1 << 7
}

public struct MazeNode
{
    public Vector3 NodeCenter;
    public Position NodePosition;
    public WallState WallState;
}

public struct Position
{
    public int X;
    public int Y;
}

public struct NeighBour
{
    public Position Position;
    public WallState SharedWall;
}

public static class MazeGenerator 
{
    public static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
        {
            case WallState.LEFT:
                return WallState.RIGHT;
            case WallState.RIGHT:
                return WallState.LEFT;
            case WallState.UP:
                return WallState.DOWN;
            case WallState.DOWN:
                return WallState.UP;
            default:
                return WallState.UP;
        }
    }

    private static MazeNode[,] ApplyRecursivebackTracker(MazeNode[,] maze, int width, int height)
    {
        var rng = new System.Random();
        var positionStack = new Stack<Position>();
        var position = new Position { X = rng.Next(0, width), Y = rng.Next(0, height) };
        maze[position.X, position.Y].WallState |= WallState.VISITED;
        positionStack.Push(position);

        while(positionStack.Count() > 0)
        {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighBours(current, maze, width, height);

            if(neighbours.Count() > 0)
            {
                positionStack.Push(current);

                var readIndex = rng.Next(0, neighbours.Count());
                var randomNeighbour = neighbours[readIndex];

                var nPosition = randomNeighbour.Position;
                maze[current.X, current.Y].WallState &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y].WallState &= ~GetOppositeWall(randomNeighbour.SharedWall);

                maze[nPosition.X, nPosition.Y].WallState |= WallState.VISITED;

                positionStack.Push(nPosition);
            }
        }

        return maze;
    }

    public static List<NeighBour> GetOpenNeighbours(Position p, MazeNode[,] maze, int width, int height)
    {
        var list = new List<NeighBour>();
        if (p.X > 0)
        {
            if (!maze[p.X, p.Y].WallState.HasFlag(WallState.LEFT))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.LEFT
                });
            }
        }
        if (p.Y > 0)
        {
            if (!maze[p.X, p.Y].WallState.HasFlag(WallState.DOWN))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallState.DOWN
                });
            }
        }
        if (p.Y < height - 1)
        {
            if (!maze[p.X, p.Y].WallState.HasFlag(WallState.UP))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallState.UP
                });
            }
        }
        if (p.X < width - 1)
        {
            if (!maze[p.X, p.Y].WallState.HasFlag(WallState.RIGHT))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.RIGHT
                });
            }
        }

        return list;
    }

    private static List<NeighBour> GetUnvisitedNeighBours(Position p, MazeNode[,] maze, int width, int height)
    {
        var list = new List<NeighBour>();
        if(p.X > 0)
        {
            if(!maze[p.X - 1, p.Y].WallState.HasFlag(WallState.VISITED))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.LEFT
                });
            }
        }
        if (p.Y > 0)
        {
            if (!maze[p.X, p.Y - 1].WallState.HasFlag(WallState.VISITED))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X ,
                        Y = p.Y - 1
                    },
                    SharedWall = WallState.DOWN
                });
            }
        }
        if (p.Y < height - 1)
        {
            if (!maze[p.X, p.Y + 1].WallState.HasFlag(WallState.VISITED))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallState.UP
                });
            }
        }
        if (p.X < width -1)
        {
            if (!maze[p.X + 1, p.Y].WallState.HasFlag(WallState.VISITED))
            {
                list.Add(new NeighBour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.RIGHT
                });
            }
        }

        return list;
    }

    public static MazeNode[,] Generate(int width, int height)
    {
        MazeNode[,] maze = new MazeNode[width, height];
        WallState initialState = WallState.LEFT | WallState.RIGHT | WallState.UP | WallState.DOWN;
        for(int indexX = 0, positionX = -width/2; indexX < width; indexX ++, positionX++)
        {
            for(int indexY = 0, positionY = -height/2; indexY < height; indexY ++, positionY++)
            {
                maze[indexX, indexY].WallState = initialState;
                maze[indexX, indexY].NodePosition = new Position() { X = indexX, Y = indexY };
                maze[indexX, indexY].NodeCenter = new Vector3(positionX, .25f, positionY);
            }
        }
        maze[0, 0].WallState &= ~WallState.LEFT;
        maze[width - 1, height - 1].WallState &= ~WallState.RIGHT;
        maze = ApplyRecursivebackTracker(maze, width, height);
        return maze;
    }
}
