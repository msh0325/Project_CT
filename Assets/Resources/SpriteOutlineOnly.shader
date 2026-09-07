Shader "SpriteOutlineOnly"
{
    Properties
    {
        _OutlineEnable("Outline Enable", Float) = 1
        _OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineExtrusion("Outline extrusion", Float) = 0.1
        [KeywordEnum(2D,25D)] _Mode("Mode", int) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Tags { "LightMode"="Universal2D" }

            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Off

            CGPROGRAM

            #pragma target 4.0
            #pragma shader_feature _MODE_2D _MODE_25D

            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v_extention
            {
                uint adjacency_vertices;
                float2 tangent;
            };

            float4 _OutlineColor;
            float _OutlineExtrusion;
            float _OutlineEnable;
            uniform StructuredBuffer<v_extention> _VertexExtention;
            uniform uint _VerticesCount;

            struct appdata
            {
                float4 vertex : POSITION0;
                uint vertexId : SV_VertexID;
            };

            struct v2g
            {
                float4 vertex : POSITION0;
                uint vertexId : TEXCOORD0;
            };

            struct g2f
            {
                float4 vertex : POSITION0;
            };


            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.vertexId = v.vertexId;
                return o;
            }

            inline void Fill(inout g2f o, in float4 vertex)
            {
                o.vertex = UnityObjectToClipPos(vertex);
            }

            void EmitEdgeGeometry(in v2g in1, in v2g in2, inout TriangleStream<g2f> triStream)
            {
                g2f o_11_m, o_11_p;
                g2f o_21_m, o_21_p;
                g2f o_1, o_2;

                uint index1 = in1.vertexId;
                uint index2 = in2.vertexId;

                v_extention ve1 = _VertexExtention[index1];
                v_extention ve2 = _VertexExtention[index2];

                float4 v1 = in1.vertex;
                float4 v2 = in2.vertex;

                float3 tangent1 = float3(ve1.tangent, 0.0f);
                float3 tangent2 = float3(ve2.tangent, 0.0f);


                #if _MODE_2D

                float3 viewDir1 = float3(0.0f, 0.0f, 1.0f);
                float3 viewDir2 = float3(0.0f, 0.0f, 1.0f);

                #endif


                #if _MODE_25D

                float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;

                float3 viewDir1 = normalize(objSpaceCameraPos - v1);
                float3 viewDir2 = normalize(objSpaceCameraPos - v2);

                #endif

                float3 height1 = normalize(cross(viewDir1, tangent1));
                float3 height2 = normalize(cross(viewDir2, tangent2));

                float3 extrusionVec11 = height1 * _OutlineExtrusion;
                float3 extrusionVec21 = height2 * _OutlineExtrusion;


                extrusionVec21 *= lerp(1.0f, -1.0f, (dot(extrusionVec11, extrusionVec21) < 0));

                Fill(o_1, v1);
                Fill(o_2, v2);

                Fill(o_11_p, float4(v1.xyz + extrusionVec11, 1.0f));
                Fill(o_11_m, float4(v1.xyz - extrusionVec11, 1.0f));

                Fill(o_21_p, float4(v2.xyz + extrusionVec21, 1.0f));
                Fill(o_21_m, float4(v2.xyz - extrusionVec21, 1.0f));

                triStream.Append(o_1);
                triStream.Append(o_2);
                triStream.Append(o_11_p);
                triStream.RestartStrip();

                triStream.Append(o_21_p);
                triStream.Append(o_2);
                triStream.Append(o_11_p);
                triStream.RestartStrip();

                triStream.Append(o_1);
                triStream.Append(o_2);
                triStream.Append(o_11_m);
                triStream.RestartStrip();

                triStream.Append(o_21_m);
                triStream.Append(o_2);
                triStream.Append(o_11_m);
                triStream.RestartStrip();

            }

            inline bool CheckEdge(in v2g in1, in v2g in2)
            {
                uint index1 = in1.vertexId;
                uint index2 = in2.vertexId;

                v_extention ve1 = _VertexExtention[index1];
                v_extention ve2 = _VertexExtention[index2];

                uint edgeEncoded1 = ve1.adjacency_vertices;
                uint edgeEncoded2 = ve2.adjacency_vertices;

                uint adj11 = edgeEncoded1 % _VerticesCount;
                uint adj12 = edgeEncoded1 / _VerticesCount;

                uint adj21 = edgeEncoded2 % _VerticesCount;
                uint adj22 = edgeEncoded2 / _VerticesCount;

                bool isValidEdge = (adj11 > 0) && (adj12 > 0) && (adj21 > 0) && (adj22 > 0);

                adj11--;
                adj12--;
                adj21--;
                adj22--;

                bool isOutlineEdge = (adj11 == index2) || (adj12 == index2) || (index1 == adj21) || (index1 == adj22);

                return  isValidEdge && isOutlineEdge;
            }


            void EmitEdgeGeometryIfNeed(in v2g in1, in v2g in2, inout TriangleStream<g2f> triStream)
            {
                if (CheckEdge(in1, in2))
                {
                    EmitEdgeGeometry(in1, in2, triStream);
                }
            }


            [maxvertexcount(39)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                // 원본 삼각형을 그대로 한 번 더 그려서 메시 안쪽을 채운다.
                g2f o0, o1, o2;
                Fill(o0, IN[0].vertex);
                Fill(o1, IN[1].vertex);
                Fill(o2, IN[2].vertex);
                triStream.Append(o0);
                triStream.Append(o1);
                triStream.Append(o2);
                triStream.RestartStrip();

                // 윤곽선(실루엣) 엣지만 바깥으로 확장해서 테두리를 만든다.
                EmitEdgeGeometryIfNeed(IN[0], IN[1], triStream);
                EmitEdgeGeometryIfNeed(IN[0], IN[2], triStream);
                EmitEdgeGeometryIfNeed(IN[1], IN[2], triStream);
            }


            float4 frag(g2f i) : SV_Target
            {
                if(_OutlineEnable <= 0) discard;

                return _OutlineColor;
            }
            ENDCG
        }
    }
}
