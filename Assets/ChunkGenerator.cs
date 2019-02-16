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

	private void Initialize()
	{
		chunks.Clear();

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				GameObject go = new GameObject($"Chunk x:{x}, y:{0}, z:{z}");
				Chunk chunk = go.AddComponent<Chunk>();
				chunk.ChunkGenerator = this;
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3(x * CHUNK_SIZE, 0, z * CHUNK_SIZE);

				chunks.Add(new Vector3Int(x, 0, z), chunk);
			}
		}
	}
}
