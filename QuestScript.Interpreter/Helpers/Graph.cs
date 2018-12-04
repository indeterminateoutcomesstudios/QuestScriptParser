using System;
using System.Collections;
using System.Collections.Generic;
using QuestScript.Interpreter.Extensions;

namespace QuestScript.Interpreter.Helpers
{
    //simple graph implementation for internal purposes
    public class Graph<TData> : IEnumerable<TData>
        where TData : IEquatable<TData>
    {             
        private readonly Dictionary<TData, HashSet<TData>> _adjacencyList = new Dictionary<TData, HashSet<TData>>();
        private readonly IEqualityComparer<TData> _equalityComparer;

        public Graph(IEqualityComparer<TData> equalityComparer = null)
        {
            _equalityComparer = equalityComparer;
        }

        public void ForEach(Action<(TData Vertex, IReadOnlyCollection<TData> AdjacentVertices)> action)
        {
            foreach (var item in _adjacencyList)
                action((item.Key, item.Value));
        }

        public IEnumerator<TData> GetEnumerator() => _adjacencyList.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"Graph<{typeof(TData).Name}> (Count = {_adjacencyList.Count})";

        public void MergeWith(Graph<TData> otherGraph)
        {
            foreach (var kvp in otherGraph._adjacencyList)
            {
                if (_adjacencyList.TryGetValue(kvp.Key, out var adjacentVertices))
                {                    
                    adjacentVertices.UnionWith(kvp.Value);
                }
                else
                {
                    _adjacencyList.Add(kvp.Key,kvp.Value);
                }
            }
        }

        public void AddVertex(TData data) => _adjacencyList.Add(data, new HashSet<TData>());
        public bool HasVertex(TData data) => _adjacencyList.ContainsKey(data);
        public void RemoveVertex(TData data) => _adjacencyList.Remove(data);

        public void Connect(TData from, IEnumerable<TData> toCollection) => toCollection.ForEach(to => Connect(from, to));

        public void Connect(TData from, TData to)
        {
            if (_adjacencyList.TryGetValue(from, out var adjacentVertices))
            {
                adjacentVertices.Add(to);
            }
            else
            {
                _adjacencyList.Add(from, new HashSet<TData>{ to });
            }
        }        

        public void Disconnect(TData from, TData to)
        {
            if (_adjacencyList.TryGetValue(from, out var adjacentVertices))
            {
                adjacentVertices.Remove(to);
            }
        }

        public IEnumerable<TData> Traverse(TData vertex) => new Bfs(_adjacencyList,vertex,_equalityComparer).Traverse();

        public class Bfs
        {
            private readonly Dictionary<TData, HashSet<TData>> _adjacencyList;
            private readonly TData _startingPoint;
            private readonly HashSet<TData> _alreadyVisited;

            public Bfs(Dictionary<TData,HashSet<TData>> adjacencyList,TData startingPoint, IEqualityComparer<TData> equalityComparer = null)
            {
                _adjacencyList = adjacencyList;
                _startingPoint = startingPoint;
                _alreadyVisited = equalityComparer == null ? new HashSet<TData>() : new HashSet<TData>(equalityComparer);
            }

            public IEnumerable<TData> Traverse() => Traverse(_startingPoint);

            private IEnumerable<TData> Traverse(TData start)
            {
                if(!_adjacencyList.TryGetValue(start, out var adjacentVertices) || 
                   _alreadyVisited.Contains(start))
                    yield break;

                _alreadyVisited.Add(start);                
                yield return start;

                foreach(var vertex in adjacentVertices)
                foreach (var fetchedVertex in Traverse(vertex))
                    yield return fetchedVertex;
            }
        }
    }
}
