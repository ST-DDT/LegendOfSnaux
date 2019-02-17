using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	private Dictionary<BlockID, Block> data = new Dictionary<BlockID, Block>();
	private bool initialized = false;

	public ChunkGenerator ChunkGenerator { get; internal set; }
	public Vector3Int ChunkID { get; internal set; }

	public static TimeSpan loadingTime;

	private void Start()
	{
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		stopWatch.Start();

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
					block.BlockID = new BlockID(x, y, z);
					goBlock.transform.parent = gameObject.transform;
					goBlock.transform.localPosition = block.BlockID;

					data.Add(block.BlockID, block);
				}
			}
		}

		stopWatch.Stop();

		TimeSpan ts = stopWatch.Elapsed;
		loadingTime += ts;

		string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
			ts.Hours, ts.Minutes, ts.Seconds,
			ts.Milliseconds / 10);

		string totalElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
			loadingTime.Hours, loadingTime.Minutes, loadingTime.Seconds,
			loadingTime.Milliseconds / 10);
		Debug.Log($"Startup Time {elapsedTime}, total: {totalElapsedTime}");
	}

	public void Initialize()
	{
		if (!initialized)
		{
			initialized = true;

			foreach (KeyValuePair<BlockID, Block> item in data)
			{
				BlockID blockId = item.Key;
				Block block = item.Value;

				List<Block.MeshFaceDirection> faceDirections = new List<Block.MeshFaceDirection>();
				Block neighbor;

				// Check Front
				if (!data.TryGetValue(blockId + BlockID.back, out neighbor))
				{
					if (blockId.z == 0)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + BlockID.back, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new BlockID(blockId.x, blockId.y, ChunkGenerator.CHUNK_SIZE - 1), out neighbor))
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
				if (!data.TryGetValue(blockId + BlockID.right, out neighbor))
				{
					if (blockId.x == ChunkGenerator.CHUNK_SIZE - 1)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.right, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new BlockID(0, blockId.y, blockId.z), out neighbor))
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
				if (!data.TryGetValue(blockId + BlockID.forward, out neighbor))
				{
					if (blockId.z == ChunkGenerator.CHUNK_SIZE - 1)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + BlockID.forward, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new BlockID(blockId.x, blockId.y, 0), out neighbor))
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
				if (!data.TryGetValue(blockId + BlockID.left, out neighbor))
				{
					if (blockId.x == 0)
					{
						if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.left, out Chunk chunk))
						{
							if (!chunk.data.TryGetValue(new BlockID(ChunkGenerator.CHUNK_SIZE - 1, blockId.y, blockId.z), out neighbor))
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
				if (!data.TryGetValue(blockId + BlockID.up, out neighbor))
				{
					faceDirections.Add(Block.MeshFaceDirection.TOP);
				}

				// Check Bottom
				// Currently not in use
				//if (!data.TryGetValue(blockId + BlockID.down, out neighbor))
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
