using UnityEngine;

namespace Runtime.Terrain
{
	[CreateAssetMenu(menuName = "SO/Create TerrainData", fileName = "TerrainData", order = 0)]
	public class TerrainCubeData : ScriptableObject
	{
		public Vector3Int ChunkSize = new(32, 32, 32);
		public int Width => ChunkSize.x;
		public int Height => ChunkSize.y;
		public int Depth => ChunkSize.z;
		public float GroundBumpHeight = 4f;

		public float IsoLevel = 0f;
		public float SurfaceNoiseScale = 0.1f;
	}
}