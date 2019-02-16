using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	private Dictionary<Vector3Int, bool> data = new Dictionary<Vector3Int, bool>();

	public ChunkGenerator ChunkGenerator { get; internal set; }

	private void Start()
	{
		Vector3 chunkWorldPosition = transform.position;
		for (int x = 0; x < ChunkGenerator.CHUNK_SIZE; x++)
		{
			for (int z = 0; z < ChunkGenerator.CHUNK_SIZE; z++)
			{
				double noise = ChunkGenerator.NoiseGenerator.Eval(
					(chunkWorldPosition.x + x) / ChunkGenerator.CHUNK_SIZE,
					(chunkWorldPosition.z + z) / ChunkGenerator.CHUNK_SIZE
				);
				double scaledNoise = SimplexNoise.Scale(noise, 0, 10);
				for (int y = 0; y < scaledNoise; y++)
				{
					data.Add(new Vector3Int(x, y, z), scaledNoise > y);
				}
			}
		}

		foreach (var item in data)
		{
			if (!item.Value)
			{
				continue;
			}

			Vector3Int position = item.Key;
			List<Block.MeshFaceDirection> faceDirections = new List<Block.MeshFaceDirection>();
			bool value;

			// Check Front
			if (!data.TryGetValue(position + Vector3Int.RoundToInt(Vector3.back), out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.FRONT);
			}

			// Check Right
			if (!data.TryGetValue(position + Vector3Int.right, out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.RIGHT);
			}

			// Check Back
			if (!data.TryGetValue(position + Vector3Int.RoundToInt(Vector3.forward), out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.BACK);
			}

			// Check Left
			if (!data.TryGetValue(position + Vector3Int.left, out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.LEFT);
			}

			// Check Top
			if (!data.TryGetValue(position + Vector3Int.up, out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.TOP);
			}

			// Check Bottom
			if (!data.TryGetValue(position + Vector3Int.down, out value) || !value)
			{
				faceDirections.Add(Block.MeshFaceDirection.BOTTOM);
			}

			GameObject goBlock = new GameObject($"Block x:{position.x}, y:{position.y}, z:{position.z}");
			Block block = goBlock.AddComponent<Block>();
			goBlock.transform.parent = gameObject.transform;
			goBlock.transform.localPosition = position;
			block.Chunk = this;
			block.InitializeMesh(faceDirections);
		}
	}
}
