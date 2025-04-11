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

			// Offset the chunk grid so that the center chunk aligns with world Y = 0
			Vector3Int offset = new Vector3Int(
				Mathf.FloorToInt(dimensions.x / 2f),
				Mathf.FloorToInt(dimensions.y / 2f),
				Mathf.FloorToInt(dimensions.z / 2f)
			);

			Vector3Int startCoord = new Vector3Int(
				centerCoord.x - offset.x,
				centerCoord.y - offset.y + (dimensions.y % 2 == 0 ? 1 : 0),
				centerCoord.z - offset.z
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


						if (!chunks.ContainsKey(chunkCoord))
						{
							GameObject chunkGO = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
							MarchingChunk chunk = chunkGO.GetComponent<MarchingChunk>();
							chunk.Generate(chunkCoord, data);
							chunks.Add(chunkCoord, chunk);
						}
					}
		}

		private Vector3Int WorldToChunkCoordCentered(TerrainCubeData data, Vector3 worldPosition)
		{
			Vector3 chunkHalfSize = new Vector3(
				data.ChunkSize.x / 2f,
				data.ChunkSize.y / 2f,
				data.ChunkSize.z / 2f
			);

			Vector3 adjustedPos = worldPosition - chunkHalfSize;

			return new Vector3Int(
				Mathf.FloorToInt(adjustedPos.x / data.ChunkSize.x),
				Mathf.FloorToInt(adjustedPos.y / data.ChunkSize.y),
				Mathf.FloorToInt(adjustedPos.z / data.ChunkSize.z)
			);
		}
	}
}