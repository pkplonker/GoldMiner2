using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Terrain
{
	[ExecuteAlways]
	public class ChunkDebugDrawer : MonoBehaviour
	{
		public Color gizmoColor = Color.cyan;
		public Terrain.TerrainCubeData terrainData;
		public Dictionary<Vector3Int, GameObject> chunkVisuals = new();

		private void OnDrawGizmos()
		{
			if (terrainData == null) return;

			Gizmos.color = gizmoColor;

			foreach (Transform child in transform)
			{
				Vector3 pos = child.position;
				Vector3 size = new Vector3(
					terrainData.ChunkSize.x,
					terrainData.ChunkSize.y,
					terrainData.ChunkSize.z
				);

				Gizmos.DrawWireCube(pos, size);
			}
		}
	}
}