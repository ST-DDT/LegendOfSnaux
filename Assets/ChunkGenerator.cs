using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

	void Awake()
	{
		for (int x = 0; x < 4; x++)
		{
			for (int z = 0; z < 4; z++)
			{
				GameObject go = new GameObject();
				Chunk chunk = go.AddComponent<Chunk>();
				go.transform.parent = gameObject.transform;
				go.transform.localPosition = new Vector3(x * 16, 0, z * 16);

				chunks.Add(new Vector3Int(x, 0, z), chunk);
			}
		}
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
