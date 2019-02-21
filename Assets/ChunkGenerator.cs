using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	public const byte CHUNK_SIZE = 16;

	private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
	private readonly Queue<Chunk> chunkLoadingQueue = new Queue<Chunk>();
	private Vector3 oldTargetPosition;
	private bool targetPositionTrigger = true;

	public SimplexNoise NoiseGenerator { get; private set; }

	public long seed = 0;
	public Transform target;

	private void Awake()
	{
		Debug.Log($"Initiated Chunk Generator with seed {seed}");
		NoiseGenerator = new SimplexNoise(seed);
	}

	private void Start()
	{
		oldTargetPosition = target.position;
	}

	private void Update()
	{
		if (Vector3.Distance(oldTargetPosition, target.position) > CHUNK_SIZE / 2.0)
		{
			oldTargetPosition = target.position;
			targetPositionTrigger = true;
		}

		if (targetPositionTrigger == true)
		{
			targetPositionTrigger = false;

			Vector3 playerChunkPosition = target.position / CHUNK_SIZE;

			// Unload Chunks that are too far away
			List<Vector3Int> unloadChunks = new List<Vector3Int>();
			foreach (KeyValuePair<Vector3Int, Chunk> item in chunks)
			{
				Vector3Int pos = item.Key;
				Chunk chunk = item.Value;
				if (Vector3.Distance(pos, playerChunkPosition) > 16)
				{
					unloadChunks.Add(pos);
					Component.Destroy(chunk);
				}
			}
			foreach (Vector3Int chunkId in unloadChunks)
			{
				chunks.Remove(chunkId);
			}
			chunkLoadingQueue.Clear();

			// Generate new Chunks
			int minX = (int)playerChunkPosition.x - 8;
			int maxX = (int)playerChunkPosition.x + 8;
			int minZ = (int)playerChunkPosition.z - 8;
			int maxZ = (int)playerChunkPosition.z + 8;
			SortedSet<Chunk> chunksSortedNearByPlayer = new SortedSet<Chunk>(Comparer<Chunk>.Create((chunk1, chunk2) =>
			{
				float chunk1DistanceToPlayer = Vector3.Distance(chunk1.ChunkID, playerChunkPosition);
				float chunk2DistanceToPlayer = Vector3.Distance(chunk2.ChunkID, playerChunkPosition);
				return chunk1DistanceToPlayer.CompareTo(chunk2DistanceToPlayer);
			}));
			for (int x = minX; x < maxX; x++)
			{
				for (int z = minZ; z < maxZ; z++)
				{
					Chunk chunk = GenerateChunk(new Vector3Int(x, 0, z));
					chunksSortedNearByPlayer.Add(chunk);
				}
			}
			foreach (Chunk chunk in chunksSortedNearByPlayer)
			{
				chunkLoadingQueue.Enqueue(chunk);
			}
		}
	}

	private void LateUpdate()
	{
		if (chunkLoadingQueue.Count > 0)
		{
			Chunk chunk = chunkLoadingQueue.Dequeue();
			chunk.Dirty = true;
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
