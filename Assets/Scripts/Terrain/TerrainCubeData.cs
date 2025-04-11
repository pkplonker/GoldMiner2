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

		[Tooltip("World-space Y position of the terrain surface center")]
		public float MaxTerrainHeightOffset => (MaxVerticalChunks * ChunkSize.y + ChunkSize.y) / 2f;

		[Tooltip("Global isolevel used to generate surface at the terrain height")]
		public float IsoLevel => MaxTerrainHeightOffset;

		[Tooltip("Controls noise-based surface generation")]
		public NoiseSettings SurfaceNoise;
	}
}