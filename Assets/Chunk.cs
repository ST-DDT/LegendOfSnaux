using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	private Dictionary<Vector3Int, Block> data = new Dictionary<Vector3Int, Block>();
	private bool initialized = false;

	public ChunkGenerator ChunkGenerator { get; internal set; }
	public Vector3Int ChunkID { get; internal set; }

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
					GameObject goBlock = new GameObject($"Block x:{x}, y:{y}, z:{z}");
					Block block = goBlock.AddComponent<Block>();
					block.Chunk = this;
					block.BlockID = new Vector3Int(x, y, z);
					goBlock.transform.parent = gameObject.transform;
					goBlock.transform.localPosition = block.BlockID;

					data.Add(block.BlockID, block);
				}
			}
		}
	}

	public void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			foreach (KeyValuePair<Vector3Int, Block> item in data)
			{
				Vector3Int blockId = item.Key;
				Block block = item.Value;

				List<Block.MeshFaceDirection> faceDirections = new List<Block.MeshFaceDirection>();
				Block neighbor;

				// Check Front
				Vector3Int v3iBack = Vector3Int.RoundToInt(Vector3.back);
				if (!data.TryGetValue(blockId + v3iBack, out neighbor))
				{
					if (blockId.z == 0)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + v3iBack, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new Vector3Int(blockId.x, blockId.y, ChunkGenerator.CHUNK_SIZE - 1), out neighbor))
							{
								faceDirections.Add(Block.MeshFaceDirection.FRONT);
							}
						}
						else
						{
							faceDirections.Add(Block.MeshFaceDirection.FRONT);
						}
					}
					else
					{
						faceDirections.Add(Block.MeshFaceDirection.FRONT);
					}
				}

				// Check Right
				if (!data.TryGetValue(blockId + Vector3Int.right, out neighbor))
				{
					if (blockId.x == ChunkGenerator.CHUNK_SIZE - 1)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.right, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new Vector3Int(0, blockId.y, blockId.z), out neighbor))
							{
								faceDirections.Add(Block.MeshFaceDirection.RIGHT);
							}
						}
						else
						{
							faceDirections.Add(Block.MeshFaceDirection.RIGHT);
						}
					}
					else
					{
						faceDirections.Add(Block.MeshFaceDirection.RIGHT);
					}
				}

				// Check Back
				Vector3Int v3iForward = Vector3Int.RoundToInt(Vector3.forward);
				if (!data.TryGetValue(blockId + v3iForward, out neighbor))
				{
					if (blockId.z == ChunkGenerator.CHUNK_SIZE - 1)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + v3iForward, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new Vector3Int(blockId.x, blockId.y, 0), out neighbor))
							{
								faceDirections.Add(Block.MeshFaceDirection.BACK);
							}
						}
						else
						{
							faceDirections.Add(Block.MeshFaceDirection.BACK);
						}
					}
					else
					{
						faceDirections.Add(Block.MeshFaceDirection.BACK);
					}
				}

				// Check Left
				if (!data.TryGetValue(blockId + Vector3Int.left, out neighbor))
				{
					if (blockId.x == 0)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.left, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new Vector3Int(ChunkGenerator.CHUNK_SIZE - 1, blockId.y, blockId.z), out neighbor))
							{
								faceDirections.Add(Block.MeshFaceDirection.LEFT);
							}
						}
						else
						{
							faceDirections.Add(Block.MeshFaceDirection.LEFT);
						}
					}
					else
					{
						faceDirections.Add(Block.MeshFaceDirection.LEFT);
					}
				}

				// Check Top
				if (!data.TryGetValue(blockId + Vector3Int.up, out neighbor))
				{
					faceDirections.Add(Block.MeshFaceDirection.TOP);
				}

				// Check Bottom
				// Currently not in use
				//if (!data.TryGetValue(blockId + Vector3Int.down, out neighbor))
				//{
				//	faceDirections.Add(Block.MeshFaceDirection.BOTTOM);
				//}

				if (faceDirections.Count > 0)
				{
					block.InitializeMesh(faceDirections);
				}
			}
		}
	}
}
