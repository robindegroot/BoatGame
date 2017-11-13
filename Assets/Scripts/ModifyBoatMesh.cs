﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyBoatMesh : MonoBehaviour
{

        private Transform boatTrans;
        Vector3[] boatVertices;
        int[] boatTriangles;

        
        public Vector3[] boatVerticesGlobal;
        float[] allDistancesToWater;

       
        public List<TriangleData> underWaterTriangleData = new List<TriangleData>();

        public ModifyBoatMesh(GameObject boatObj)
        {
            
            boatTrans = boatObj.transform;
            boatVertices = boatObj.GetComponent<MeshFilter>().mesh.vertices;
            boatTriangles = boatObj.GetComponent<MeshFilter>().mesh.triangles;
            boatVerticesGlobal = new Vector3[boatVertices.Length];
            allDistancesToWater = new float[boatVertices.Length];
        }

        public void GenerateUnderwaterMesh()
        {
            underWaterTriangleData.Clear();

            for (int j = 0; j < boatVertices.Length; j++)
            {
                Vector3 globalPos = boatTrans.TransformPoint(boatVertices[j]);
                boatVerticesGlobal[j] = globalPos;
                allDistancesToWater[j] = WaterController.current.DistanceToWater(globalPos, Time.time);
            }

            AddTriangles();
        }

        private void AddTriangles()
        {
            List<VertexData> vertexData = new List<VertexData>();

            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());

            int i = 0;
            while (i < boatTriangles.Length)
            {
                for (int x = 0; x < 3; x++)
                {
                    vertexData[x].distance = allDistancesToWater[boatTriangles[i]];

                    vertexData[x].index = x;

                    vertexData[x].globalVertexPos = boatVerticesGlobal[boatTriangles[i]];

                    i++;
                }

                if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance > 0f)
                {
                    continue;
                }


                if (vertexData[0].distance < 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    Vector3 p1 = vertexData[0].globalVertexPos;
                    Vector3 p2 = vertexData[1].globalVertexPos;
                    Vector3 p3 = vertexData[2].globalVertexPos;

                    underWaterTriangleData.Add(new TriangleData(p1, p2, p3));
                }
                else
                {
                    vertexData.Sort((x, y) => x.distance.CompareTo(y.distance));

                    vertexData.Reverse();

                    if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesOneAboveWater(vertexData);
                    }
                    else if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesTwoAboveWater(vertexData);
                    }
                }
            }
        }

        private void AddTrianglesOneAboveWater(List<VertexData> vertexData)
        {
            Vector3 H = vertexData[0].globalVertexPos;

            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }

            float h_H = vertexData[0].distance;
            float h_M = 0f;
            float h_L = 0f;

            Vector3 M = Vector3.zero;
            Vector3 L = Vector3.zero;

            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].globalVertexPos;
                L = vertexData[2].globalVertexPos;

                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].globalVertexPos;
                L = vertexData[1].globalVertexPos;

                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }

            Vector3 MH = H - M;

            float t_M = -h_M / (h_H - h_M);

            Vector3 MI_M = t_M * MH;

            Vector3 I_M = MI_M + M;

            Vector3 LH = H - L;

            float t_L = -h_L / (h_H - h_L);

            Vector3 LI_L = t_L * LH;

            Vector3 I_L = LI_L + L;

            underWaterTriangleData.Add(new TriangleData(M, I_M, I_L));
            underWaterTriangleData.Add(new TriangleData(M, I_L, L));
        }

        private void AddTrianglesTwoAboveWater(List<VertexData> vertexData)
        {
            Vector3 L = vertexData[2].globalVertexPos;

            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }

            float h_L = vertexData[2].distance;
            float h_H = 0f;
            float h_M = 0f;

            Vector3 H = Vector3.zero;
            Vector3 M = Vector3.zero;

            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].globalVertexPos;
                M = vertexData[0].globalVertexPos;

                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].globalVertexPos;
                M = vertexData[1].globalVertexPos;

                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }

            Vector3 LM = M - L;

            float t_M = -h_L / (h_M - h_L);

            Vector3 LJ_M = t_M * LM;

            Vector3 J_M = LJ_M + L;

            Vector3 LH = H - L;

            float t_H = -h_L / (h_H - h_L);

            Vector3 LJ_H = t_H * LH;

            Vector3 J_H = LJ_H + L;

            underWaterTriangleData.Add(new TriangleData(L, J_H, J_M));
        }

        private class VertexData
        {
            public float distance;
            public int index;
            public Vector3 globalVertexPos;
        }

        public void DisplayMesh(Mesh mesh, string name, List<TriangleData> triangesData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < triangesData.Count; i++)
            {
                Vector3 p1 = boatTrans.InverseTransformPoint(triangesData[i].p1);
                Vector3 p2 = boatTrans.InverseTransformPoint(triangesData[i].p2);
                Vector3 p3 = boatTrans.InverseTransformPoint(triangesData[i].p3);

                vertices.Add(p1);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p2);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p3);
                triangles.Add(vertices.Count - 1);
            }

            mesh.Clear();

            mesh.name = name;

            mesh.vertices = vertices.ToArray();

            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
        }
    }