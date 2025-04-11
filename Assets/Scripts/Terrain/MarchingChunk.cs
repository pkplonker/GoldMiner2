using System.Collections.Generic;
using System.Diagnostics;
using Runtime.Terrain.Tables;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Runtime.Terrain
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class MarchingChunk : MonoBehaviour
	{
		private float[,,] density;
		private TerrainCubeData terrainData;
		private Vector3Int chunkCoord;

		public void Generate(Vector3Int chunkCoord, TerrainCubeData terrainData)
		{
			this.chunkCoord = chunkCoord;
			this.terrainData = terrainData;

			transform.position = new Vector3(
				chunkCoord.x * terrainData.ChunkWidth,
				chunkCoord.y * terrainData.ChunkHeight,
				chunkCoord.z * terrainData.ChunkDepth
			);
			var timer = Stopwatch.StartNew();
			//GenerateDensity();
			GenerateDensityWithGPU();
			Debug.Log($"Density generated in {timer.ElapsedMilliseconds}ms");
			timer.Reset();
			timer.Start();
			GenerateMesh();
			Debug.Log($"Mesh generated in {timer.ElapsedMilliseconds}ms");
		}

		[SerializeField]
		ComputeShader densityShader;

		ComputeBuffer densityBuffer;

		private void GenerateDensityWithGPU()
		{
			int width = terrainData.ChunkWidth + 1;
			int height = terrainData.ChunkHeight + 1;
			int depth = terrainData.ChunkDepth + 1;
			int total = width * height * depth;

			densityBuffer = new ComputeBuffer(total, sizeof(float));
			densityShader.SetBuffer(0, "DensityBuffer", densityBuffer);
			densityShader.SetInts("size", width - 1, height - 1, depth - 1);
			densityShader.SetFloat("isoLevel", terrainData.IsoLevel);
			densityShader.SetFloat("baseHeight", terrainData.IsoLevel);
			densityShader.SetFloat("bumpHeight", terrainData.SurfaceNoise.HeightMultiplier);
			densityShader.SetFloat("noiseScale", terrainData.SurfaceNoise.Scale);
			densityShader.SetFloat("noiseScale", terrainData.SurfaceNoise.Scale);
			densityShader.SetInt("octaves", terrainData.SurfaceNoise.Octaves);
			densityShader.SetFloat("persistence", terrainData.SurfaceNoise.Persistence);
			densityShader.SetFloat("lacunarity", terrainData.SurfaceNoise.Lacunarity);
			float chunkWorldOffsetY = chunkCoord.y * terrainData.ChunkHeight;
			densityShader.SetFloat("chunkWorldOffsetY", chunkWorldOffsetY);
			Vector2 chunkWorldOffset = new Vector2(
				chunkCoord.x * terrainData.ChunkWidth,
				chunkCoord.z * terrainData.ChunkDepth
			);

			densityShader.SetFloats("chunkWorldOffsetXZ", chunkWorldOffset.x, chunkWorldOffset.y);
			int threadGroupsX = Mathf.CeilToInt(width / 8f);
			int threadGroupsY = Mathf.CeilToInt(height / 8f);
			int threadGroupsZ = Mathf.CeilToInt(depth / 8f);

			densityShader.Dispatch(0, threadGroupsX, threadGroupsY, threadGroupsZ);

			float[] densities = new float[total];
			densityBuffer.GetData(densities);
			densityBuffer.Release();

			density = new float[width, height, depth];
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
					for (int z = 0; z < depth; z++)
					{
						int index = x + y * width + z * width * height;
						density[x, y, z] = densities[index];
					}
		}

		private void GenerateDensity()
		{
			int w = terrainData.ChunkWidth;
			int h = terrainData.ChunkHeight;
			int d = terrainData.ChunkDepth;

			density = new float[w + 1, h + 1, d + 1];
			//var min = float.MaxValue;
			//var max = float.MinValue;
			for (int x = 0; x <= w; x++)
				for (int y = 0; y <= h; y++)
					for (int z = 0; z <= d; z++)
					{
						float worldX = x + chunkCoord.x * terrainData.ChunkWidth;
						float worldZ = z + chunkCoord.z * terrainData.ChunkDepth;
						float worldY = y + chunkCoord.y * terrainData.ChunkHeight;

						float perlinOffset = GetFractalNoise(worldX, worldZ, terrainData) - 0.5f;

						float groundY = terrainData.IsoLevel + perlinOffset;
						var densityValue = groundY - worldY;
						density[x, y, z] = densityValue;
						//if (densityValue > max) max = densityValue;
						//if (densityValue < min) min = densityValue;
					}

			//Debug.Log($"Min: {min} Max: {max}");
			//Debug.Log($"Iso: {terrainData.IsoLevel}");
		}

		float GetFractalNoise(float x, float z, TerrainCubeData data)
		{
			float amplitude = 1f;
			float frequency = 1f;
			float noiseHeight = 0f;

			for (int i = 0; i < data.SurfaceNoise.Octaves; i++)
			{
				float sampleX = (x + data.SurfaceNoise.Offset.x) * data.SurfaceNoise.Scale * frequency;
				float sampleZ = (z + data.SurfaceNoise.Offset.y) * data.SurfaceNoise.Scale * frequency;

				float perlin = Mathf.PerlinNoise(sampleX, sampleZ);
				noiseHeight += perlin * amplitude;

				amplitude *= data.SurfaceNoise.Persistence;
				frequency *= data.SurfaceNoise.Lacunarity;
			}

			return noiseHeight * data.SurfaceNoise.HeightMultiplier;
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