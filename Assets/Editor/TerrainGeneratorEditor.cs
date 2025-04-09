using System;
using Runtime;
using Runtime.Terrain;
using UnityEngine;
using UnityEditor;

public class TerrainGeneratorEditor : EditorWindow
{
	private TerrainCubeData selectedTerrainData;
	private Vector3Int selectedTerrainDataPosition;
	private Vector3Int multiGeneration;
	public TerrainGeneratorEditor() { }

	[MenuItem("SH/Terrain")]
	public static void GoldStatusWindow() => GetWindow<TerrainGeneratorEditor>();

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Select Terrain Data", EditorStyles.boldLabel);
		selectedTerrainData = (TerrainCubeData) EditorGUILayout.ObjectField(
			"Terrain Data", selectedTerrainData, typeof(TerrainCubeData), false);

		GUILayout.Space(10);

		var chunkManager = ServiceLocator.Instance.GetService<ChunkManager>();

		GUI.enabled = selectedTerrainData != null;
		selectedTerrainDataPosition = EditorGUILayout.Vector3IntField("Spawn Location", selectedTerrainDataPosition);
		if (GUILayout.Button("Generate Single"))
		{
			chunkManager.GenerateChunkAtWorldPosition(selectedTerrainData, selectedTerrainDataPosition);
		}

		multiGeneration = EditorGUILayout.Vector3IntField("Multi Chunk Generation", multiGeneration);

		if (GUILayout.Button("Generate Radius"))
		{
			chunkManager.GenerateChunksAround(selectedTerrainData, selectedTerrainDataPosition, multiGeneration);
		}
		if (GUILayout.Button("Clear"))
		{
			chunkManager.Clear();
		}

		GUI.enabled = true;
	}
}