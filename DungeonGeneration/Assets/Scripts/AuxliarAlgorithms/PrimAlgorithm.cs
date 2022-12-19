using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

//Prim Algorithm for Minimum Spanning Tree
public static class PrimAlgorithm
{
    /// <summary>
    /// Creates a minimum spanning tree and returns its edges
    /// </summary>
    /// <param name="delaunayEdges">Delaunay edges</param>
    /// <param name="addSomeRemainingEdges">If we want to add the remainig edges of the minimum spanning tree</param>
    /// <returns>Returns a hashset the minimum spanning tree edges</returns>
    public static HashSet<Edge> RunMinimumSpanningTree(List<Edge> delaunayEdges, bool addSomeRemainingEdges)
    {
        //Minimum spanning tree edges
        List<Edge> mst = MinimumSpanningTree(delaunayEdges, delaunayEdges[0].U);
        HashSet<Edge> selectedEdges = new HashSet<Edge>(mst);

        //Remaining edges
        HashSet<Edge> remainingEdges = new HashSet<Edge>(delaunayEdges);
        remainingEdges.ExceptWith(selectedEdges);

        if(addSomeRemainingEdges)
        {
            foreach (Edge edge in remainingEdges)
            {
                if (Random.value < 0.125)
                {
                    selectedEdges.Add(edge);
                }
            }
        }

        return selectedEdges;
    }

    /// <summary>
    /// Returns the minimum spanning tree edges
    /// </summary>
    /// <param name="edges">Edges list</param>
    /// <param name="start">Start edge</param>
    /// <returns>Returns a hashset the minimum spanning tree edges</returns>
    private static List<Edge> MinimumSpanningTree(List<Edge> edges, Vertex start)
    {
        HashSet<Vertex> openSet = new HashSet<Vertex>();
        HashSet<Vertex> closedSet = new HashSet<Vertex>();

        foreach (Edge edge in edges)
        {
            openSet.Add(edge.U);
            openSet.Add(edge.V);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        while (openSet.Count > 0)
        {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (Edge edge in edges)
            {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.U)) closedVertices++;
                if (!closedSet.Contains(edge.V)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.Distance < minWeight)
                {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.Distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);
            openSet.Remove(chosenEdge.U);
            openSet.Remove(chosenEdge.V);
            closedSet.Add(chosenEdge.U);
            closedSet.Add(chosenEdge.V);
        }

        return results;
    }
}
