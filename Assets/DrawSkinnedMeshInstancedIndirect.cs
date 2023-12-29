// DrawSkinnedMeshInstancedIndirect.cs
// Created by Cui Lingzhi
// on 2023 - 10 - 23


using System;
using UnityEngine;

public class DrawSkinnedMeshInstancedIndirect : MonoBehaviour
{
    [Range(0f, 1f)] public float progress = 0.0f;
    public Mesh triangleMesh;
    private uint mTriangleCount;
    private const int TRIANGLE_SUB_MESH_INDEX = 0;
    public Material instanceMaterial;

    public SkinnedMeshRenderer smrA;
    private ComputeBuffer mVertexUvBufferA;
    //public Transform rootA;

    public SkinnedMeshRenderer smrB;
    private ComputeBuffer mVertexUvBufferB;
    //public Transform rootB;


    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private static readonly int sProgress = Shader.PropertyToID("_Progress");

    private bool initialized = false;
    private GraphicsBuffer bufferA;
    private GraphicsBuffer bufferB;
    private GraphicsBuffer bufferA_index;
    private GraphicsBuffer bufferB_index;
    private static readonly int VerticesBufferA = Shader.PropertyToID("verticesBufferA");
    private static readonly int VerticesBufferB = Shader.PropertyToID("verticesBufferB");
    private static readonly int VertexIndicesBufferA = Shader.PropertyToID("VertexIndicesBufferA");
    private static readonly int VertexIndicesBufferB = Shader.PropertyToID("VertexIndicesBufferB");
    private static readonly int VertexUvBufferA = Shader.PropertyToID("vertexUvBufferA");
    private static readonly int VertexUvBufferB = Shader.PropertyToID("vertexUvBufferB");
    private static readonly int OffsetA = Shader.PropertyToID("_OffsetA");
    private static readonly int OffsetB = Shader.PropertyToID("_OffsetB");
    private static readonly int EulerAngleA = Shader.PropertyToID("_EulerAngleA");
    private static readonly int EulerAngleB = Shader.PropertyToID("_EulerAngleB");

    void Update()
    {
        if (!initialized)
        {
            if (bufferA == null) bufferA = smrA.GetVertexBuffer();
            if (bufferB == null) bufferB = smrB.GetVertexBuffer();
            smrA.sharedMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
            smrB.sharedMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
            if (bufferA_index == null || !bufferA_index.IsValid()) bufferA_index = smrA.sharedMesh.GetIndexBuffer();
            if (bufferB_index == null || !bufferB_index.IsValid()) bufferB_index = smrB.sharedMesh.GetIndexBuffer();

          
            
            if (bufferA != null && bufferB != null && bufferA_index.IsValid() && bufferB_index.IsValid())
            {
                initialized = true;
                mTriangleCount = (uint)Mathf.Max((int)smrA.sharedMesh.GetIndexCount(0) / 3, (int)smrB.sharedMesh.GetIndexCount(0) / 3);
                InitArgsBuffer();
                //vector2 => 4byte * 2
                mVertexUvBufferA = new ComputeBuffer(smrA.sharedMesh.uv.Length, 8);
                mVertexUvBufferB = new ComputeBuffer(smrB.sharedMesh.uv.Length, 8);
                mVertexUvBufferA.SetData(smrA.sharedMesh.uv);
                mVertexUvBufferB.SetData(smrB.sharedMesh.uv);
                instanceMaterial.SetBuffer(VerticesBufferA, bufferA);
                instanceMaterial.SetBuffer(VerticesBufferB, bufferB);
                instanceMaterial.SetBuffer(VertexIndicesBufferA, bufferA_index);
                instanceMaterial.SetBuffer(VertexIndicesBufferB, bufferB_index);
                instanceMaterial.SetBuffer(VertexUvBufferA, mVertexUvBufferA);
                instanceMaterial.SetBuffer(VertexUvBufferB, mVertexUvBufferB);
                
                //找的动作太复杂了,只用一个节点数值不正确
                // instanceMaterial.SetVector(OffsetA, rootA.localPosition);
                // instanceMaterial.SetVector(OffsetB, rootB.localPosition);
                // instanceMaterial.SetVector(EulerAngleA, rootA.localEulerAngles);
                // instanceMaterial.SetVector(EulerAngleB, rootB.localEulerAngles);
            }
        }
        else
        {
            instanceMaterial.SetFloat(sProgress, progress);
            // Render
            Graphics.DrawMeshInstancedIndirect(triangleMesh, TRIANGLE_SUB_MESH_INDEX, instanceMaterial,
                new Bounds(Vector3.zero, Vector3.one * 100), argsBuffer);
        }
    }

    void InitArgsBuffer()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        
        // Indirect args
        if (triangleMesh != null)
        {
            args[0] = (uint)triangleMesh.GetIndexCount(TRIANGLE_SUB_MESH_INDEX);
            args[1] = mTriangleCount;
            args[2] = (uint)triangleMesh.GetIndexStart(TRIANGLE_SUB_MESH_INDEX);
            args[3] = (uint)triangleMesh.GetBaseVertex(TRIANGLE_SUB_MESH_INDEX);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }

        argsBuffer.SetData(args);
    }

    private void LateUpdate()
    {
        progress = Mathf.Clamp(Mathf.Sin(Time.time)*2,0,1);
    }

    void OnDisable()
    {
        if (mVertexUvBufferA != null)
            mVertexUvBufferA.Dispose();
        if (mVertexUvBufferB != null)
            mVertexUvBufferB.Dispose();

        if (bufferA != null) bufferA.Dispose();
        if (bufferB != null) bufferB.Dispose();
        if (bufferA_index != null) bufferA_index.Dispose();
        if (bufferB_index != null) bufferB_index.Dispose();

        if (argsBuffer != null)
            argsBuffer.Dispose();
    }
}