using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	public const byte CHUNK_SIZE = 16;

	private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

	public SimplexNoise NoiseGenerator { get; private set; }

	public long seed = 0;

	private void Awake()
	{
		Debug.Log($"Initiated Chunk Generator with seed {seed}");
		NoiseGenerator = new SimplexNoise(seed);
	}

	void Start()
	{
		Initialize();
	}

	private void LateUpdate()
	{
		foreach (Chunk chunk in chunks.Values)
		{
			chunk.Initialize();
		}
	}

	private void Initialize()
	{
		chunks.Clear();

		for (int x = -2; x <= 2; x++)
		{
			for (int z = -2; z <= 2; z++)
			{
				GameObject go = new GameObject($"Chunk x:{x}, y:{0}, z:{z}");
				Chunk chunk = go.AddComponent<Chunk>();
				chunk.ChunkGenerator = this;
				chunk.ChunkID = new Vector3Int(x, 0, z);
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3(x * CHUNK_SIZE, 0, z * CHUNK_SIZE);

				chunks.Add(chunk.ChunkID, chunk);
			}
		}
	}

	public bool TryGetChunk(Vector3Int chunkId, out Chunk chunk)
	{
		return this.chunks.TryGetValue(chunkId, out chunk);
	}
}
