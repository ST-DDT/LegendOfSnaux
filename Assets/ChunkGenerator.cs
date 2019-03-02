using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using xxHashSharp;

public class ChunkGenerator : MonoBehaviour
{
	public const int REGION_SIZE = 512;
	public const byte CHUNK_SIZE = 16;
	public const float BLOCK_SIZE = 0.5f;

	public readonly Dictionary<Vector2Int, Region> regions = new Dictionary<Vector2Int, Region>();
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
		Vector3 targetPosition = target.position;
		if (Vector3.Distance(oldTargetPosition, targetPosition) > CHUNK_SIZE * BLOCK_SIZE / 2.0)
		{
			oldTargetPosition = targetPosition;
			targetPositionTrigger = true;
		}

		if (targetPositionTrigger == true)
		{
			targetPositionTrigger = false;

			Vector3 playerChunkPositionExact = targetPosition * (1 / BLOCK_SIZE) / CHUNK_SIZE;
			Vector3Int playerChunkPosition = new Vector3Int((int)playerChunkPositionExact.x, (int)playerChunkPositionExact.y, (int)playerChunkPositionExact.z);

			// Unload Chunks that are too far away
			List<Vector3Int> unloadChunks = new List<Vector3Int>();
			// TODO: Allow to set this variable from externally (such as an options menu)
			int numChunksInEachDirection = 8;
			int numChunksAppartFromTargetInDirection = numChunksInEachDirection + 4;
			foreach (KeyValuePair<Vector3Int, Chunk> item in chunks)
			{
				Vector3Int pos = item.Key;
				Chunk chunk = item.Value;
				if (Vector3.Distance(pos, playerChunkPosition) > numChunksAppartFromTargetInDirection)
				{
					unloadChunks.Add(pos);
					Destroy(chunk.gameObject);
				}
			}
			foreach (Vector3Int chunkId in unloadChunks)
			{
				chunks.Remove(chunkId);
			}
			chunkLoadingQueue.Clear();

			// Generate new Chunks
			int minX = playerChunkPosition.x - numChunksInEachDirection;
			int maxX = playerChunkPosition.x + numChunksInEachDirection;
			int minZ = playerChunkPosition.z - numChunksInEachDirection;
			int maxZ = playerChunkPosition.z + numChunksInEachDirection;
			SortedSet<Chunk> chunksSortedNearByPlayer = new SortedSet<Chunk>(Comparer<Chunk>.Create((chunk1, chunk2) =>
			{
				float chunk1DistanceToPlayer = Vector3.Distance(chunk1.transform.position, targetPosition);
				float chunk2DistanceToPlayer = Vector3.Distance(chunk2.transform.position, targetPosition);
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
		go.transform.localPosition = (Vector3)position * CHUNK_SIZE * BLOCK_SIZE;

		chunks.Add(chunk.ChunkID, chunk);

		return chunk;
	}

	public Region GetOrGenerateRegion(Vector2Int position)
	{
		if (this.regions.TryGetValue(position, out Region region))
		{
			return region;
		}

		byte[] buf = BitConverter.GetBytes(position.x).Concat(BitConverter.GetBytes(position.y)).ToArray();
		uint valueX = XXHash.CalculateHash(buf, buf.Length, seed: Convert.ToUInt32(seed));
		uint valueZ = XXHash.CalculateHash(buf, buf.Length, seed: Convert.ToUInt32(seed + 1));
		region = new Region()
		{
			Name = "",
			RegionID = position,
			RelativeVoronoiPoint = new Vector2(valueX, valueZ) / uint.MaxValue
		};
		regions.Add(region.RegionID, region);
		// TODO: Remove this later, it is for debugging
		// GameObject go = new GameObject($"Voronoi {x} {z}");
		// go.transform.position = new Vector3(region.VoronoiWorldPoint.x, 0, region.VoronoiWorldPoint.y);
		return region;
	}

	public bool TryGetChunk(Vector3Int chunkId, out Chunk chunk)
	{
		return this.chunks.TryGetValue(chunkId, out chunk);
	}

	public Region GetNearestRegion(float x, float z)
	{
		Vector2 worldPosition = new Vector2(x, z);
		Vector2Int regionId = Vector2Int.FloorToInt(worldPosition / REGION_SIZE);
		Region? nearestRegion = null;
		float nearestDistance = float.MaxValue;
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dz = -1; dz <= 1; dz++)
			{
				Region region = GetOrGenerateRegion(regionId + new Vector2Int(dx, dz));

				if (nearestRegion == null)
				{
					nearestRegion = region;
					nearestDistance = Vector2.Distance(region.VoronoiWorldPoint, worldPosition);
					continue;
				}

				float distance = Vector2.Distance(region.VoronoiWorldPoint, worldPosition);

				if (nearestDistance > distance)
				{
					nearestDistance = distance;
					nearestRegion = region;
				}
			}
		}
		return (Region)nearestRegion;
	}
}
