using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;
	[SerializeField] private Material _material;
	[SerializeField] private bool _drawDebug = false;

	private ComputeBuffer _vertexExtentionBuffer;

	#if UNITY_EDITOR

	private Vector2[] _vertices;
	private IDictionary<Edge, int> _edgeToTrianglesCountMap;

	#endif

	private unsafe void Awake()
	{
		//fetch geometry data from sprite
		_spriteRenderer = GetComponent<SpriteRenderer>();
		var vertices = _spriteRenderer.sprite.vertices;
		var triangles = _spriteRenderer.sprite.triangles;

		//filtering edges to get pure outline
		var edgeToTrianglesCountMap = new Dictionary<Edge, int>();
		var verticesToEdgesMap = new ISet<Edge>[vertices.Length];

		for(var i = 0; i < triangles.Length; i+=3)
		{
			var v0 = triangles[i];
			var v1 = triangles[i + 1];
			var v2 = triangles[i + 2];

			var e1 = new Edge(v0, v1);
			var e2 = new Edge(v0, v2);
			var e3 = new Edge(v1, v2);

			RegisterEdgeUsage(e1, edgeToTrianglesCountMap, verticesToEdgesMap);
			RegisterEdgeUsage(e2, edgeToTrianglesCountMap, verticesToEdgesMap);
			RegisterEdgeUsage(e3, edgeToTrianglesCountMap, verticesToEdgesMap);
		}

		
		var adjacentVertices = new (int v1, int v2)[vertices.Length];
		for(var i = 0; i < vertices.Length; i++)
		{
			adjacentVertices[i] = (-1, -1);
		}

		foreach (var e in edgeToTrianglesCountMap.Keys)
		{
			//we must ignore all non-outline edges
			if (edgeToTrianglesCountMap[e] == 1)
			{
				var (from, to) = e;
				SetAdjacencyBetweenTwoVertices(from, to, adjacentVertices);
				SetAdjacencyBetweenTwoVertices(to, from, adjacentVertices);
			}
		}

		for (var i = 0; i < vertices.Length; i++)
		{
			if(adjacentVertices[i].v1 > 0 && adjacentVertices[i].v1 == adjacentVertices[i].v2)
			{
				throw new Exception($"duplicate adjacent vertices({adjacentVertices[i].v1}) for vertex: {i}");
			}

			if(adjacentVertices[i].v1 == i || adjacentVertices[i].v2 == i)
			{
				throw new Exception($"loop-edge at vertex: {i}");
			}
		}

#if UNITY_EDITOR
		_vertices = vertices;
		_edgeToTrianglesCountMap = edgeToTrianglesCountMap;
#endif

		checked
		{
			_vertexExtentionBuffer = new ComputeBuffer(vertices.Length, sizeof(VExtention));

			_vertexExtentionBuffer.SetData(adjacentVertices.Select(d => {
				uint v1 = (uint)(d.v1 + 1);
				uint v2 = (uint)(d.v2 + 1);
				var encoded = (uint)(v1 * (vertices.Length + 1) + v2);
				var tangent = Vector2.zero;
				if(d.v1 >= 0 && d.v2 >= 0)
				{
					tangent = (vertices[d.v2] - vertices[d.v1]).normalized;
				}
				return new VExtention(encoded, tangent);
			}).ToArray());
		}

		var materialInstance = Instantiate(_material);
		_spriteRenderer.material = materialInstance;
		materialInstance.SetBuffer("_VertexExtention", _vertexExtentionBuffer);
		materialInstance.SetInt("_VerticesCount", vertices.Length + 1);
	}

	private void SetAdjacencyBetweenTwoVertices(int first, int second, (int v1, int v2)[] adjacency)
	{
		if(adjacency[first].v1 < 0)
		{
			adjacency[first].v1 = second;
			return;
		}

		if (adjacency[first].v2 < 0)
		{
			adjacency[first].v2 = second;
			return;
		}

		throw new Exception($"Slot not found for vertex {first}. Adjacent: {second}");
	}

	private void OnDestroy()
	{
		if(_vertexExtentionBuffer != null)
		{
			_vertexExtentionBuffer.Dispose();
		}
	}

	private void RegisterEdgeUsage(in Edge e, IDictionary<Edge, int> edgeToTrianglesCountMap, ISet<Edge>[] verticesToEdgesMap)
	{
		if (!edgeToTrianglesCountMap.ContainsKey(e))
		{
			edgeToTrianglesCountMap[e] = 0;
		}
		edgeToTrianglesCountMap[e]++;

		var (from, to) = e;
		verticesToEdgesMap[from] ??= new HashSet<Edge>();
		verticesToEdgesMap[to] ??= new HashSet<Edge>();

		verticesToEdgesMap[from].Add(e);
		verticesToEdgesMap[to].Add(e);
	}


#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if(_drawDebug && Application.isPlaying)
		{
			DrawEdges();
			DrawVertices();
		}
	}

	private void DrawVertices()
	{
		for(var i = 0; i < _vertices.Length; i++)
		{
			var p = transform.TransformPoint(_vertices[i]);
			UnityEditor.Handles.Label(p, i.ToString());
		}
	}

	private void DrawEdges()
	{
		foreach(var kv in _edgeToTrianglesCountMap)
		{
			if(kv.Value == 1)
			{
				var (v1, v2) = kv.Key;
				var p1 = transform.TransformPoint(_vertices[v1]);
				var p2 = transform.TransformPoint(_vertices[v2]);
				Debug.DrawLine(p1, p2, Color.blue);
			}
		}
	}

#endif

	[StructLayout(LayoutKind.Sequential)]
	private readonly struct VExtention
	{
		private readonly uint AdjacencyVertices;
		private readonly Vector2 Tangent;

		public VExtention(uint adjVertices, Vector2 tangent)
		{
			AdjacencyVertices = adjVertices;
			Tangent = tangent;
		}
	};

	private readonly struct Edge : IEquatable<Edge>
	{
		private readonly (ushort idxFrom, ushort idxTo) _raw;

		public Edge(ushort idxFrom, ushort idxTo)
		{
			_raw = (Math.Min(idxFrom, idxTo), Math.Max(idxFrom, idxTo));
		}

		public override bool Equals(object obj) => Equals((Edge)obj);
		public bool Equals(Edge other) => _raw.Equals(other._raw);
		public override int GetHashCode() => _raw.GetHashCode();

		public void Deconstruct(out ushort idxFrom, out ushort idxTo) => (idxFrom, idxTo) = _raw;
	}
}
