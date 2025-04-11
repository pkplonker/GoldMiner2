using UnityEngine;

namespace Runtime.Terrain
{
	[CreateAssetMenu(menuName = "SO/Create TerrainData", fileName = "TerrainData", order = 0)]
	public class TerrainCubeData : ScriptableObject
	{
		public Vector3Int ChunkSize = new(32, 32, 32);
		public int ChunkWidth => ChunkSize.x;
		public int ChunkHeight => ChunkSize.y;
		public int ChunkDepth => ChunkSize.z;

		[Tooltip("Estimated vertical chunk count for terrain centering")]
		public int MaxVerticalChunks = 6;

		public float MaxTerrainHeightOffset => (MaxVerticalChunks * ChunkSize.y) / 2f;

		public float GroundBumpHeight = 12f;

		public float IsoLevel => MaxTerrainHeightOffset;

		public float SurfaceNoiseScale = 0.1f;
	}
}