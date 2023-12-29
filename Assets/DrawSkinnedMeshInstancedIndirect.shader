Shader "DrawMeshInstancedIndirect"
{
    Properties
    {
        _MainTexA ("AlbedoA (RGB)", 2D) = "white" {}
        _MainTexB ("AlbedoB (RGB)", 2D) = "white" {}
        
          _OffsetA("_OffsetA",vector) = (0,0,0)
          _OffsetB("_OffsetB",vector) = (0,0,0)
          _EulerAngleA("_EulerAngleA",vector) = (0,0,0)
          _EulerAngleB("_EulerAngleB",vector) = (0,0,0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"
            #include "Assets/Transform.hlsl"


            //Vertex buffers
            ByteAddressBuffer verticesBufferA;
            ByteAddressBuffer verticesBufferB;

            //Index buffers
            ByteAddressBuffer VertexIndicesBufferA;
            ByteAddressBuffer VertexIndicesBufferB;

            //顶点的Uv
            StructuredBuffer<float2> vertexUvBufferA;
            StructuredBuffer<float2> vertexUvBufferB;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTexA;
            sampler2D _MainTexB;
            float3 _OffsetA;
            float3 _OffsetB;
            float3 _EulerAngleA;
            float3 _EulerAngleB;
            float _Progress;

            //获取顶点的坐标
            //vertexId:0,1,2 instanceId:0,1,2,...,maxCount 
            float3 GetVertexData_Position(ByteAddressBuffer vertexBuffer, ByteAddressBuffer indexBuffer, uint vertexId,
                                          uint instanceId)
            {
                //instanceId * 3 + vertexId => 第几个顶点
                // *4 => IndexFormat需要选32bits,就是4Byte
                const int vertexAddress = indexBuffer.Load((instanceId * 3 + vertexId) * 4);


                //float3 position
                //float3 normal
                //float4 tangent
                //10byte * 4 = 40byte
                const int vertexBufferIndex = vertexAddress * 40;
                float3 data = asfloat(vertexBuffer.Load3(vertexBufferIndex));
                return data;
            }

            float2 GetVertexData_UV(ByteAddressBuffer indexBuffer, StructuredBuffer<float2> vUvBuffer
                                    , uint vertexId, uint instanceId)
            {
                const int vertexAddress = indexBuffer.Load((instanceId * 3 + vertexId) * 4);

                return vUvBuffer[vertexAddress];
            }


            v2f vert(appdata v, uint vid : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;


                float3 posA = GetVertexData_Position(verticesBufferA, VertexIndicesBufferA, vid, instanceID);
                float3 posB = GetVertexData_Position(verticesBufferB, VertexIndicesBufferB, vid, instanceID);
                posA = rotate(posA, _EulerAngleA) + _OffsetA;
                posB = rotate(posB, _EulerAngleB) + _OffsetB;
                float3 pos = lerp(posA, posB, _Progress);
                float2 uv = lerp(GetVertexData_UV(VertexIndicesBufferA, vertexUvBufferA, vid, instanceID),
                                 GetVertexData_UV(VertexIndicesBufferB, vertexUvBufferB, vid, instanceID),
                                 _Progress);

                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Texture
                fixed4 albedoA = tex2D(_MainTexA, i.uv);
                fixed4 albedoB = tex2D(_MainTexB, i.uv);

                return lerp(albedoA, albedoB, _Progress);
            }
            ENDCG
        }
    }
}