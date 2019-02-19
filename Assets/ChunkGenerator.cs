using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	public const byte CHUNK_SIZE = 16;

	private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
	private readonly Queue<Chunk> chunkLoadingQueue = new Queue<Chunk>();

	public SimplexNoise NoiseGenerator { get; private set; }

	public long seed = 0;

	private void Awake()
	{
		Debug.Log($"Initiated Chunk Generator with seed {seed}");
		NoiseGenerator = new SimplexNoise(seed);
	}

	private void Start()
	{
		Initialize();
	}

	private void LateUpdate()
	{
		if (chunkLoadingQueue.Count > 0)
		{
			Chunk chunk = chunkLoadingQueue.Dequeue();
			chunk.Dirty = true;
		}
	}

	private void Initialize()
	{
		chunks.Clear();

		for (int x = -2; x <= 20; x++)
		{
			for (int z = -2; z <= 20; z++)
			{
				Chunk chunk = GenerateChunk(new Vector3Int(x, 0, z));
				chunkLoadingQueue.Enqueue(chunk);
			}
		}
	}

	public Chunk GenerateChunk(Vector3Int position)
	{
		if (TryGetChunk(position, out Chunk chunk))
		{
			return chunk;
		}

		GameObject go = new GameObject($"Chunk x:{position.x}, y:{position.y}, z:{position.z}");
		chunk = go.AddComponent<Chunk>();
		chunk.ChunkGenerator = this;
		chunk.ChunkID = position;
		go.transform.parent = gameObject.transform;
		go.transform.localPosition = new Vector3(position.x * CHUNK_SIZE, position.y * CHUNK_SIZE, position.z * CHUNK_SIZE);

		chunks.Add(chunk.ChunkID, chunk);

		return chunk;
	}

	public bool TryGetChunk(Vector3Int chunkId, out Chunk chunk)
	{
		return this.chunks.TryGetValue(chunkId, out chunk);
	}
}
