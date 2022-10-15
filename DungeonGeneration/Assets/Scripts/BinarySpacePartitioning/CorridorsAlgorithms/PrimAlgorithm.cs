using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class PrimAlgorithm
{

    public static HashSet<Edge> RunMinimumSpanningTree(List<Edge> delaunayEdges, bool addSomeRemainingEdges)
    {
        List<Edge> edges = new List<Edge>();

        HashSet<Edge> selectedEdges = new HashSet<Edge>();

        foreach (var edge in delaunayEdges)
        {
            edges.Add(new Edge(edge.U, edge.V));
        }

        List<Edge> mst = MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Edge>(mst);
        var remainingEdges = new HashSet<Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        if(addSomeRemainingEdges)
        {
            foreach (var edge in remainingEdges)
            {
                if (Random.value < 0.125)
                {
                    selectedEdges.Add(edge);
                }
            }
        }

        return selectedEdges;
    }

    private static List<Edge> MinimumSpanningTree(List<Edge> edges, Vertex start)
    {
        HashSet<Vertex> openSet = new HashSet<Vertex>();
        HashSet<Vertex> closedSet = new HashSet<Vertex>();

        foreach (var edge in edges)
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

            foreach (var edge in edges)
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
