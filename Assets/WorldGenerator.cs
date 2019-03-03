using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RandomGenerator;

public class WorldGenerator : MonoBehaviour
{
	public const ushort REGION_SIZE = 512;
	public const byte CHUNK_SIZE = 16;
	public const float BLOCK_SIZE = 0.5f;

	public readonly Dictionary<Vector2Int, Region> regions = new Dictionary<Vector2Int, Region>();
	private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
	private readonly Queue<Chunk> chunkLoadingQueue = new Queue<Chunk>();
	private Vector3 oldPlayerPosition;
	private bool playerPositionTrigger = true;

	public SimplexNoise NoiseGenerator { get; private set; }

	[Tooltip("The world seed used for any random generators.")]
	public long seed = 0;

	[Tooltip("The player of the current session.")]
	public Transform player;

	[Range(2, 20)]
	[Tooltip("The number of chunks to be generated from the player in any direction.")]
	public int viewDistance = 8;

	private void Awake()
	{
		Debug.Log($"Initiated World with seed {seed}");
		NoiseGenerator = new SimplexNoise(seed);
	}

	private void Start()
	{
		oldPlayerPosition = player.position;
	}

	private void Update()
	{
		Vector3 playerPosition = player.position;
		if (Vector3.Distance(oldPlayerPosition, playerPosition) > CHUNK_SIZE * BLOCK_SIZE / 2f)
		{
			oldPlayerPosition = playerPosition;
			playerPositionTrigger = true;
		}

		if (playerPositionTrigger == true)
		{
			playerPositionTrigger = false;

			Vector3 playerChunkPositionExact = playerPosition * (1f / BLOCK_SIZE) / CHUNK_SIZE;
			Vector3Int playerChunkPosition = new Vector3Int((int)playerChunkPositionExact.x, (int)playerChunkPositionExact.y, (int)playerChunkPositionExact.z);

			// Unload Chunks that are too far away
			List<Vector3Int> unloadChunks = new List<Vector3Int>();
			int numChunksInEachDirection = viewDistance;
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
				float chunk1DistanceToPlayer = Vector3.Distance(chunk1.transform.position, playerPosition);
				float chunk2DistanceToPlayer = Vector3.Distance(chunk2.transform.position, playerPosition);
				return chunk1DistanceToPlayer.CompareTo(chunk2DistanceToPlayer);
			}));
			for (int x = minX; x < maxX; x++)
			{
				for (int z = minZ; z < maxZ; z++)
				{
					Chunk chunk = GetOrGenerateChunk(new Vector3Int(x, 0, z), out CacheOrigin cache);
					switch (cache)
					{
						case CacheOrigin.NewlyGenerated:
							chunksSortedNearByPlayer.Add(chunk);
							break;
						case CacheOrigin.FetchedFromLocal:
							Debug.Log($"Chunk {chunk.ChunkID} already loaded");
							break;
						case CacheOrigin.FetchedFromFile:
							Debug.LogError($"CacheType {cache} currently not implemented");
							break;
						case CacheOrigin.FetchedFromServer:
							Debug.LogError($"CacheType {cache} currently not implemented");
							break;
						default:
							Debug.LogError($"CacheType {cache} currently not implemented");
							break;
					}
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

	public Chunk GetOrGenerateChunk(Vector3Int position, out CacheOrigin cache)
	{
		if (this.chunks.TryGetValue(position, out Chunk chunk))
		{
			cache = CacheOrigin.FetchedFromLocal;
			return chunk;
		}

		GameObject go = new GameObject($"Chunk x:{position.x}, y:{position.y}, z:{position.z}");
		chunk = go.AddComponent<Chunk>();
		chunk.ChunkGenerator = this;
		chunk.ChunkID = position;
		go.transform.parent = gameObject.transform;
		go.transform.localPosition = (Vector3)position * CHUNK_SIZE * BLOCK_SIZE;

		chunks.Add(chunk.ChunkID, chunk);

		cache = CacheOrigin.NewlyGenerated;
		return chunk;
	}

	public bool TryGetChunk(Vector3Int chunkId, out Chunk chunk)
	{
		return this.chunks.TryGetValue(chunkId, out chunk);
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
		Vector2 relativeVoronoiPoint = new Vector2(valueX, valueZ) / uint.MaxValue;
		Vector2 voronoiPoint = (position + relativeVoronoiPoint);
		float humidity = (float)SimplexNoise.Scale(NoiseGenerator.Eval(voronoiPoint.x, voronoiPoint.y), 0d, 1d);
		float temperature = (float)SimplexNoise.Scale(NoiseGenerator.Eval(voronoiPoint.y, voronoiPoint.x), 0d, 1d);
		region = new Region()
		{
			Name = "",
			RegionID = position,
			RelativeVoronoiPoint = relativeVoronoiPoint,
			Humidity = humidity,
			Temperature = temperature
		};
		Debug.Log($"Generated Region {region.RegionID} with Humidity: {region.Humidity} and Temperature: {region.Temperature}");
		regions.Add(region.RegionID, region);
		// TODO: Remove this later, it is for debugging
		// GameObject go = new GameObject($"Voronoi {x} {z}");
		// go.transform.position = new Vector3(region.VoronoiWorldPoint.x, 0, region.VoronoiWorldPoint.y);
		return region;
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
