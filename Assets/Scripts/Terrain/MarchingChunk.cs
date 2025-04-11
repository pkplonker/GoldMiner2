using System.Collections.Generic;
using Runtime.Terrain.Tables;
using UnityEngine;

namespace Runtime.Terrain
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class MarchingChunk : MonoBehaviour
	{
		private float[,,] density;
		private TerrainCubeData terrainData;
		private Vector3Int chunkCoord;
		private float baseHeight;

		public void Generate(Vector3Int chunkCoord, TerrainCubeData terrainData)
		{
			this.chunkCoord = chunkCoord;
			this.terrainData = terrainData;

			transform.position = new Vector3(
				chunkCoord.x * terrainData.ChunkWidth,
				chunkCoord.y * terrainData.ChunkHeight,
				chunkCoord.z * terrainData.ChunkDepth
			);
			baseHeight = terrainData.MaxTerrainHeightOffset;

			GenerateDensity();
			GenerateMesh();
		}

		private void GenerateDensity()
		{
			int w = terrainData.ChunkWidth;
			int h = terrainData.ChunkHeight;
			int d = terrainData.ChunkDepth;

			density = new float[w + 1, h + 1, d + 1];
			var min = float.MaxValue;
			var max = float.MinValue;
			for (int x = 0; x <= w; x++)
				for (int y = 0; y <= h; y++)
					for (int z = 0; z <= d; z++)
					{
						float worldX = x + chunkCoord.x * terrainData.ChunkWidth;
						float worldZ = z + chunkCoord.z * terrainData.ChunkDepth;
						float worldY = y + chunkCoord.y * terrainData.ChunkHeight;

						float perlinOffset =
							(Mathf.PerlinNoise(worldX * terrainData.SurfaceNoiseScale,
								worldZ * terrainData.SurfaceNoiseScale) - 0.5f) * 2f * terrainData.GroundBumpHeight;
						float groundY = baseHeight + perlinOffset;
						var value = groundY - worldY;
						density[x, y, z] = value;
						if (value > max) max = value;
						if (value < min) min = value;
					}

			Debug.Log($"Min: {min}\nMax: {max}");
		}

		private void GenerateMesh()
		{
			List<Vector3> vertices = new();
			List<int> triangles = new();

			int w = terrainData.ChunkWidth;
			int h = terrainData.ChunkHeight;
			int d = terrainData.ChunkDepth;

			for (int x = 0; x < w; x++)
				for (int y = 0; y < h; y++)
					for (int z = 0; z < d; z++)
					{
						float[] cube = new float[8];
						for (int i = 0; i < 8; i++)
						{
							Vector3Int c = CornerOffset(i);
							cube[i] = density[x + c.x, y + c.y, z + c.z];
						}

						int cubeIndex = 0;
						for (int i = 0; i < 8; i++)
							if (cube[i] < terrainData.IsoLevel)
								cubeIndex |= 1 << i;

						int edgeFlags = MarchingCubesLookup.Edges[cubeIndex];
						if (edgeFlags == 0) continue;

						Vector3[] edgeVertices = new Vector3[12];
						for (int i = 0; i < 12; i++)
						{
							if ((edgeFlags & (1 << i)) != 0)
							{
								(int a0, int b0) = EdgeIndexToCorners(i);
								Vector3 p1 = CornerOffset(a0);
								Vector3 p2 = CornerOffset(b0);
								float d1 = cube[a0];
								float d2 = cube[b0];

								float t = Mathf.Clamp01((terrainData.IsoLevel - d1) / (d2 - d1));
								edgeVertices[i] = Vector3.Lerp(p1, p2, t) + new Vector3(x, y, z) -
								                  new Vector3(terrainData.ChunkWidth, terrainData.ChunkHeight,
									                  terrainData.ChunkDepth) * 0.5f;
							}
						}

						for (int i = 0; MarchingCubesLookup.Triangles[cubeIndex, i] != -1; i += 3)
						{
							int a = MarchingCubesLookup.Triangles[cubeIndex, i];
							int b = MarchingCubesLookup.Triangles[cubeIndex, i + 1];
							int c = MarchingCubesLookup.Triangles[cubeIndex, i + 2];

							vertices.Add(edgeVertices[a]);
							vertices.Add(edgeVertices[b]);
							vertices.Add(edgeVertices[c]);

							int baseIndex = vertices.Count - 3;
							triangles.Add(baseIndex);
							triangles.Add(baseIndex + 2);
							triangles.Add(baseIndex + 1);
						}
					}

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.RecalculateNormals();

			GetComponent<MeshFilter>().mesh = mesh;
		}

		private Vector3Int CornerOffset(int i)
		{
			return i switch
			{
				0 => new Vector3Int(0, 0, 0),
				1 => new Vector3Int(1, 0, 0),
				2 => new Vector3Int(1, 0, 1),
				3 => new Vector3Int(0, 0, 1),
				4 => new Vector3Int(0, 1, 0),
				5 => new Vector3Int(1, 1, 0),
				6 => new Vector3Int(1, 1, 1),
				7 => new Vector3Int(0, 1, 1),
				_ => Vector3Int.zero
			};
		}

		private (int, int) EdgeIndexToCorners(int edge)
		{
			int[,] cornerIndexAFromEdge =
			{
				{0, 1}, {1, 2}, {2, 3}, {3, 0},
				{4, 5}, {5, 6}, {6, 7}, {7, 4},
				{0, 4}, {1, 5}, {2, 6}, {3, 7}
			};

			return (cornerIndexAFromEdge[edge, 0], cornerIndexAFromEdge[edge, 1]);
		}
	}
}