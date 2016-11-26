using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI22
{
    class Program
    {
        static void Main(string[] args)
        {

            Problem theProblem = new Problem();



            AStarSearch(theProblem);

            Console.WriteLine("Solution Found");
            Console.ReadLine();
        }


        public static bool AStarSearch(Problem problem/*, List<Actions> solutionList*/)
        {

            //bool solutionFound = false;
            List<Node> frontier = new List<Node>();
            List<Node> explored = new List<Node>();

            Node rootNode = new Node(problem.startPosX, problem.startPosY, problem.startMap);
            if (problem.GoalTest(rootNode))
            {
                return true;
            }

            frontier.Add(rootNode);

            //Loop
            while (frontier.Count != 0)
            {
                Node expandNode = frontier.First();

                foreach (Node node in frontier)
                {
                    if (node.EstimatedCost() < expandNode.EstimatedCost())
                    {
                        expandNode = node;
                    }
                }

                if (problem.GoalTest(expandNode))
                {
                    Console.WriteLine(expandNode.pathCost.ToString());

                    int w = expandNode.stateMap.GetLength(0); // width
                    int h = expandNode.stateMap.GetLength(1); // height

                    for (int x = 0; x < w; ++x)
                    {
                        Console.WriteLine();
                        for (int y = 0; y < h; ++y)
                        {
                            Console.Write(expandNode.stateMap[x, y]);
                        }
                    }


                    Console.ReadLine();
                    return true;
                }

                frontier.Remove(expandNode);
                explored.Add(expandNode);

                foreach (Actions action in Enum.GetValues(typeof(Actions)))
                {
                    Node newNode = problem.DoAction(action, expandNode);

                    if (newNode.GetType() != typeof(NullNode))
                    {
                        bool alreadyInExplored = false;
                        foreach (Node node in explored)
                        {
                            if (Extensions.CompareArrays(node.stateMap, newNode.stateMap))
                            {
                                alreadyInExplored = true;
                                break;
                            }
                        }

                        if (!alreadyInExplored)
                        {
                            frontier.Add(newNode);
                        }

                    }

                }
            }

            return false;
        }


        public static bool DepthFirstSearch(Problem problem, Node currentNode, List<Actions> solutionList)
        {
            //List<Node> frontier = new List<Node>();
            //List<Node> explored = new List<Node>();

            //frontier.Add(rootNode);

            //Problem.Actions solutionAction;

            //Loop
            //if (frontier.Count == 0) { return null; }

            //Node expandNode = frontier.Last();
            //frontier.Remove(frontier.Last());
            bool solutionFound = false;

            if (problem.GoalTest(currentNode))
            {
                return true;
            }

            //explored.Add(expandNode);

            foreach (Actions action in Enum.GetValues(typeof(Actions)))
            {

                Node childNode = problem.DoAction(action, currentNode);
                if (childNode.GetType() != typeof(NullNode))
                {
                    solutionFound = DepthFirstSearch(problem, childNode, solutionList);
                }

                if (solutionFound)
                {
                    if (!childNode.rootNode)
                    {
                        solutionList.Add(childNode.parentNodeAction);
                        Console.WriteLine(Extensions.ToFriendlyString(childNode.parentNodeAction));
                    }
                    return solutionFound;
                }
            }

            return false;

        }

    }
}

public enum Actions { MoveLeft, MoveRight, MoveUp, MoveDown };


class Node
{
    public int blankPosX;
    public int blankPosY;
    public int[,] stateMap;
    public int pathCost;
    public Actions parentNodeAction;
    public Node parentNode;
    public bool rootNode;

    public Node() { }

    public Node(int posX, int posY, int[,] map)
    {
        blankPosX = posX;
        blankPosY = posY;
        stateMap = map;
        pathCost = 0;
        parentNode = new NullNode();
        rootNode = true;
    }

    public Node(int posX, int posY, int[,] map, Actions action, Node myParentNode)
    {
        blankPosX = posX;
        blankPosY = posY;
        stateMap = map;
        pathCost = myParentNode.pathCost + 1;
        parentNodeAction = action;
        parentNode = myParentNode;
        rootNode = false;
    }

    private int HeuristicFunction()
    {

        int totalH = 0;

        for (int number = 0; number <= 8; number++)
        {
            Tuple<int, int> currentCoords = Extensions.CoordinatesOf(stateMap, number);
            Tuple<int, int> goalCoords = Extensions.CoordinatesOf(Problem.goalMap, number);

            int xDist = Math.Abs(currentCoords.Item1 - goalCoords.Item1);
            int yDist = Math.Abs(currentCoords.Item2 - goalCoords.Item2);

            totalH += xDist;
            totalH += yDist;
        }

        return totalH;
    }

    public int EstimatedCost()
    {
        return pathCost + HeuristicFunction();
    }


}

class NullNode : Node
{
    public NullNode()
    {

    }
}

public static class Extensions
{
    public static string ToFriendlyString(this Actions me)
    {
        switch (me)
        {
            case Actions.MoveDown:
                return "Move Down /n";
            case Actions.MoveUp:
                return "Move Up /n";
            case Actions.MoveLeft:
                return "Move Left /n";
            case Actions.MoveRight:
                return "Move Right /n";
        }

        return "";
    }

    public static Tuple<int, int> CoordinatesOf<T>(this T[,] matrix, T value)
    {
        int w = matrix.GetLength(0); // width
        int h = matrix.GetLength(1); // height

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                if (matrix[x, y].Equals(value))
                    return Tuple.Create(x, y);
            }
        }

        return Tuple.Create(-1, -1);
    }

    public static int[,] Clone(int[,] array)
    {
        var newArray = new int[array.GetLength(0), array.GetLength(1)];
        for (var i = 0; i < array.GetLength(0); i++)
        {
            for (var j = 0; j < array.GetLength(1); j++)
            {
                newArray[i, j] = array[i, j];
            }

        }

        return newArray;
    }

    public static bool CompareArrays(int[,] array1, int[,] array2)
    {
        bool equal =
            array1.Rank == array2.Rank && //Same number of rows
            Enumerable.Range(0, array1.Rank).All(dimension => array1.GetLength(dimension) == array2.GetLength(dimension)) && //Each row is same length
            array1.Cast<int>().SequenceEqual(array2.Cast<int>()); //Enumerate all values and check equivalence

        return equal;

    }


}



class Problem
{
    const int width = 3;
    const int height = 3;
    public static int[,] goalMap = new int[3, 3] { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } };
    int[,] map = new int[3, 3] { { 7, 2, 4 }, { 5, 0, 6 }, { 8, 3, 7 } };

    private int pathCost = 0;
    private int searchCost = 0;

    #region "Getters/Setters"
    public int[,] Map
    {
        get { return map; }
    }
    public int PathCost
    {
        get { return pathCost; }
    }
    public int SearchCost
    {
        get { return searchCost; }
    }
    #endregion

    public int[,] startMap = { { 7, 2, 4 }, { 5, 0, 6 }, { 8, 3, 1 } };
    public int startPosX = 1;
    public int startPosY = 1;


    public bool GoalTest(Node node)
    {
        return Extensions.CompareArrays(node.stateMap, goalMap);
    }



    #region "Actions/Operators"

    public Node MoveUp(Node node)
    {
        searchCost++;
        if (node.blankPosY == 0)
        {
            return new NullNode();
        }

        node.stateMap[node.blankPosY, node.blankPosX] = node.stateMap[node.blankPosY - 1, node.blankPosX];
        node.blankPosY -= 1;
        node.stateMap[node.blankPosY, node.blankPosX] = 0;
        return node;
    }

    public Node MoveDown(Node node)
    {
        searchCost++;
        if (node.blankPosY == 2)
        {
            return new NullNode();
        }

        node.stateMap[node.blankPosY, node.blankPosX] = node.stateMap[node.blankPosY + 1, node.blankPosX];
        node.blankPosY += 1;
        node.stateMap[node.blankPosY, node.blankPosX] = 0;
        return node;
    }

    public Node MoveLeft(Node node)
    {
        searchCost++;
        if (node.blankPosX == 0)
        {
            return new NullNode();
        }

        node.stateMap[node.blankPosY, node.blankPosX] = node.stateMap[node.blankPosY, node.blankPosX - 1];
        node.blankPosX -= 1;
        node.stateMap[node.blankPosY, node.blankPosX] = 0;
        return node;
    }

    public Node MoveRight(Node node)
    {
        searchCost++;
        if (node.blankPosX == 2)
        {
            return new NullNode();
        }

        node.stateMap[node.blankPosY, node.blankPosX] = node.stateMap[node.blankPosY, node.blankPosX + 1];
        node.blankPosX += 1;
        node.stateMap[node.blankPosY, node.blankPosX] = 0;
        return node;
    }


    #endregion

    public Node DoAction(Actions action, Node node)
    {
        Node newNode = new Node(node.blankPosX, node.blankPosY, Extensions.Clone(node.stateMap), action, node);
        switch (action)
        {
            case Actions.MoveDown:
                return MoveDown(newNode);
            case Actions.MoveUp:
                return MoveUp(newNode);
            case Actions.MoveLeft:
                return MoveLeft(newNode);
            case Actions.MoveRight:
                return MoveRight(newNode);
        }

        return new NullNode();

    }




}





