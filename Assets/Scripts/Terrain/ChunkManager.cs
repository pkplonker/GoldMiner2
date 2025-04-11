using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Terrain
{
	public class ChunkManager : MonoBehaviour, IService
	{
		public GameObject chunkPrefab;

		private Dictionary<Vector3Int, MarchingChunk> chunks = new();

		private void Awake()
		{
			ServiceLocator.Instance.RegisterService(this);
		}

		public void Clear()
		{
			foreach (var (pos, chunk) in chunks)
			{
				Destroy(chunk.gameObject);
			}

			chunks.Clear();
		}

		public void Initialize() { }

		public void GenerateChunkAtWorldPosition(TerrainCubeData data, Vector3 worldPosition)
		{
			Vector3Int centerCoord = WorldToChunkCoordCentered(data, worldPosition);

			if (chunks.ContainsKey(centerCoord))
				return;

			GameObject chunkGO = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
			var chunk = chunkGO.GetComponent<MarchingChunk>();
			chunk.Generate(centerCoord, data);
			chunks.Add(centerCoord, chunk);
		}

		public void GenerateChunksAround(TerrainCubeData data, Vector3Int worldPosition, Vector3Int dimensions)
		{
			Vector3Int centerCoord = WorldToChunkCoordCentered(data, worldPosition);
			Vector3Int startCoord = new Vector3Int(
				centerCoord.x - Mathf.FloorToInt(dimensions.x / 2f),
				centerCoord.y - Mathf.FloorToInt(dimensions.y / 2f),
				centerCoord.z - Mathf.FloorToInt(dimensions.z / 2f)
			);

			for (int x = 0; x < dimensions.x; x++)
				for (int y = 0; y < dimensions.y; y++)
					for (int z = 0; z < dimensions.z; z++)
					{
						Vector3Int chunkCoord = new Vector3Int(
							startCoord.x + x,
							startCoord.y + y,
							startCoord.z + z
						);

						Vector3 chunkPosition = new Vector3(
							chunkCoord.x * data.ChunkSize.x,
							chunkCoord.y * data.ChunkSize.y,
							chunkCoord.z * data.ChunkSize.z
						);

						if (!chunks.ContainsKey(chunkCoord))
						{
							GameObject chunkGO = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity,
								transform);
							MarchingChunk chunk = chunkGO.GetComponent<MarchingChunk>();
							chunk.Generate(chunkCoord, data);
							chunks.Add(chunkCoord, chunk);
						}
					}
		}

		private Vector3Int WorldToChunkCoordCentered(TerrainCubeData data, Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x / data.ChunkSize.x),
				Mathf.FloorToInt(worldPosition.y / data.ChunkSize.y),
				Mathf.FloorToInt(worldPosition.z / data.ChunkSize.z)
			);
		}
	}
}