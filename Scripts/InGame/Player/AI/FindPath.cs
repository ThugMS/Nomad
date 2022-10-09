using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node
{
    public bool walkable;
    public Vector2 pos;
    public Vector2 index;
    public float fCost = -1;
    public Node parentNode;

    public Node(bool _walkable, Vector2 _pos, Vector2 _idx)
    {
        walkable = _walkable;
        pos = _pos;
        index = _idx;
    }
}

public class PathFinder
{
    #region PrivateMethod
    private (Node[,], (Node, Node)) CreateGrid(Vector2 _start, Vector2 _end)
    {
        float gridLength = 1f;
        int addNodeLength = 50;

        float diffX = _end.x - _start.x;
        float diffY = _end.y - _start.y;

        int xCount = ((int)Mathf.Ceil(Mathf.Abs(diffX) / gridLength)) + addNodeLength;
        int yCount = ((int)Mathf.Ceil(Mathf.Abs(diffY) / gridLength)) + addNodeLength;

        int xSign = (int)Mathf.Sign(diffX);
        int ySign = (int)Mathf.Sign(diffY);

        Node[,] grid = new Node[xCount, yCount];

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                float x = _start.x + xSign * (i - addNodeLength / 2) * gridLength;
                float y = _start.y + ySign * (j - addNodeLength / 2) * gridLength;
                Vector2 pos = new Vector2(x, y);
                Vector2 index = new Vector2(i, j);

                grid[i, j] = new Node(true, pos, index);

                if (i == xCount - 1 && j == yCount - 1)
                    grid[i, j].walkable = true;
                else
                    DecideWalkable(i, j, ref grid);
            }
        }

        Node originNode = grid[addNodeLength / 2, addNodeLength / 2];
        Node destiNode = grid[xCount - 1 - addNodeLength / 2, yCount - 1 - addNodeLength / 2];

        return (grid, (originNode, destiNode));
    }
    private void DecideWalkable(int i, int k, ref Node[,] grid)
    {
        Vector2 pos = grid[i, k].pos;
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, 1f, 1 << 10);
        if (cols.Length <= 0)
            grid[i, k].walkable = true;
        else
            grid[i, k].walkable = false;
    }
    private List<Node> GetNeighborhood(Node _node, Node[,] _grid)
    {
        List<Node> nbd = new List<Node>();
        int[,] dir = { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
        bool[] w = new bool[4];

        //한방향
        for (int i = 0; i < 4; i++)
        {
            int nextX = (int)_node.index.x + dir[i, 0];
            int nextY = (int)_node.index.y+ dir[i, 1];

            if (0 <= nextX && nextX < _grid.GetLength(0) && 0 <= nextY && nextY < _grid.GetLength(1))
            { 
                if (_grid[nextX, nextY].walkable)
                {
                    nbd.Add(_grid[nextX, nextY]);
                    w[i] = true;
                }
            }
        }

        //대각선
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;

            if (w[i] && w[j])
            {
                int nextX = (int)_node.index.x + dir[i, 0] + dir[j, 0];
                int nextY = (int)_node.index.y + dir[i, 1] + dir[j, 1];

                if (0 <= nextX && nextX < _grid.GetLength(0) && 0 <= nextY && nextY < _grid.GetLength(1))
                {
                    if (_grid[nextX, nextY].walkable)
                        nbd.Add(_grid[nextX, nextY]);
                }
            }
        }
        return nbd;
    }
    private int GetDistance(Node _nodeA, Node _nodeB)
    {
         return (int)Vector2.SqrMagnitude(_nodeA.index - _nodeB.index);
    }
    #endregion
    public List<Node> CalculatePath(Vector2 _start, Vector2 _end)
    {
        (Node[,], (Node, Node)) map = CreateGrid(_start, _end);
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        Node[,] grid = map.Item1;
        Node origin = map.Item2.Item1;
        Node desti = map.Item2.Item2;
        List<Node> path = new List<Node>();
        openSet.Add(origin);

        while (openSet.Count > 0)
        {
            Node currNode = openSet[0];

            for (int i = 0; i < openSet.Count; i++)
            {
                float currgCost = GetDistance(currNode, origin);
                float currhCost = GetDistance(currNode, desti);
                float currfCost = currgCost + currhCost;
                currNode.fCost = currfCost;

                float gCost = GetDistance(openSet[i], origin);
                float hCost = GetDistance(openSet[i], desti);
                float fCost = gCost + hCost;
                openSet[i].fCost = fCost;

                if (fCost < currfCost || (fCost == currfCost && hCost < currhCost))
                    currNode = openSet[i];
            }

            openSet.Remove(currNode);
            closedSet.Add(currNode);

            if (currNode.index == desti.index)
            {
                while (currNode.index != origin.index)
                {
                    path.Add(currNode);

                    currNode = currNode.parentNode;
                }

                path.Reverse();
                return path;

            }

            List<Node> nbd;
            nbd = GetNeighborhood(currNode, grid);

            foreach (var n in nbd)
            {
                if (!n.walkable || closedSet.Contains(n))
                    continue;

                float newCostToNBD = GetDistance(currNode, origin) + GetDistance(currNode, n);
                if (newCostToNBD > GetDistance(n, origin) || !openSet.Contains(n))
                {
                    n.parentNode = currNode;

                    if (!openSet.Contains(n))
                    {
                        openSet.Add(n);
                    }
                }
            }

        }
        return path;
    }
}