using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

// 스프라이트의 실루엣(윤곽선)을 따라가는 아웃라인을 그리기 위해,
// 스프라이트 메시의 버텍스들이 서로 어떻게 인접해있는지(윤곽선 상에서 누구 옆에 누가 있는지)를
// 미리 계산해서 GPU(ComputeBuffer)로 넘겨주는 스크립트.
// 실제로 바깥으로 밀어내서 그리는 건 셰이더(SpriteOutlineOnly.shader)의 지오메트리 셰이더가 담당함.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;
	[SerializeField] private Material _material;
	[SerializeField] private bool _drawDebug = false;

	private ComputeBuffer _vertexExtentionBuffer;

	// 애니메이션으로 스프라이트가 바뀌는 걸 감지하기 위해 마지막으로 계산했던 스프라이트를 저장해둠.
	// (지금은 idle 애니메이션이 없어서 스프라이트가 안 바뀌지만, 나중에 애니메이션이 붙으면
	//  프레임마다 다른 Sprite 에셋으로 교체되므로 그때마다 아래 데이터를 다시 계산해줘야 함)
	private Sprite _lastSprite;

	#if UNITY_EDITOR

	private Vector2[] _vertices;
	private IDictionary<Edge, int> _edgeToTrianglesCountMap;

	#endif

	private unsafe void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		// 시작할 때 한 번 계산해서 아웃라인을 만들어둔다.
		RebuildOutline();
	}

	// 스프라이트가 바뀌었는지 매 프레임 확인한다.
	// 지금은 스프라이트가 고정이라 이 체크가 딱 한 번(맨 처음) 걸리고 끝나지만,
	// 나중에 Animator로 idle/walk 등 애니메이션을 재생하면 SpriteRenderer.sprite가
	// 프레임마다 다른 Sprite 에셋으로 바뀌기 때문에, 그때마다 버텍스 인접 정보를
	// 새로 계산해줘야 아웃라인이 그 프레임의 실루엣이랑 맞게 그려진다.
	// (버텍스 개수/배치가 프레임마다 다를 수 있어서, 그냥 값만 갱신하는 게 아니라
	//  통째로 다시 계산 + ComputeBuffer도 새로 만들어야 함)
	private void Update()
	{
		if (_spriteRenderer.sprite != _lastSprite)
		{
			RebuildOutline();
		}
	}

	// Awake()에 있던 계산 로직을 그대로 옮겨온 것.
	// "지금 SpriteRenderer에 물려있는 스프라이트" 기준으로:
	// 1. 메시의 버텍스/삼각형 정보를 가져오고
	// 2. 삼각형 하나당 한 번씩만 등장하는 엣지(=윤곽선 엣지)를 골라내고
	// 3. 각 버텍스마다 "윤곽선 상에서 내 옆에 있는 버텍스가 누구인지"를 계산해서
	// 4. GPU가 읽을 수 있는 ComputeBuffer 형태로 만들어 머티리얼에 넘겨준다.
	private unsafe void RebuildOutline()
	{
		_lastSprite = _spriteRenderer.sprite;

		// 이전에 만들어둔 버퍼가 있으면 새로 만들기 전에 반드시 해제한다.
		// (ComputeBuffer는 GC가 안 치워주는 네이티브 리소스라, Dispose 안 하고 새로 만들면
		//  메모리 누수가 생김 - 애니메이션으로 매 프레임 스프라이트가 바뀔 수 있으니 특히 중요)
		if (_vertexExtentionBuffer != null)
		{
			_vertexExtentionBuffer.Dispose();
			_vertexExtentionBuffer = null;
		}

		//fetch geometry data from sprite
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

		// 머티리얼도 매번 새로 인스턴스화해서 이 오브젝트 전용으로 만든다.
		// (같은 머티리얼을 여러 유닛이 공유하면 서로 다른 버퍼를 덮어써버리기 때문에
		//  반드시 인스턴스를 따로 만들어야 함)
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
