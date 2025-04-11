using UnityEngine;

namespace Runtime.Terrain
{
	[CreateAssetMenu(menuName = "SO/NoiseData", fileName = "NoiseData", order = 0)]
	public class NoiseSettings : ScriptableObject
	{
		public float Scale = 0.01f;
		public int Octaves = 4;
		public float Persistence = 0.5f;
		public float Lacunarity = 2f;
		public float HeightMultiplier = 1f;
		public Vector2 Offset = Vector2.zero;
	}
}