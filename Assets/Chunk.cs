using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshFaceDirection
{
	FRONT,
	RIGHT,
	BACK,
	LEFT,
	TOP,
	BOTTOM
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
	private Dictionary<Vector3Int, Block> data = new Dictionary<Vector3Int, Block>();

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private MeshCollider meshCollider;

	public ChunkGenerator ChunkGenerator { get; internal set; }
	public Vector3Int ChunkID { get; internal set; }
	public bool Dirty { get; set; } = false;

	private void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();

		meshRenderer.material = Resources.Load<Material>("VertexColoredWithShadow");
	}

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
					Block block = new Block()
					{
						Name = $"Block x:{x}, y:{y}, z:{z}",
						Chunk = this,
						BlockID = new Vector3Int(x, y, z)
					};

					data.Add(block.BlockID, block);
				}
			}
		}
	}

	private void LateUpdate()
	{
		if (Dirty == false)
		{
			// No updates todo
			return;
		}

		UpdateMesh();

		Dirty = false;
	}

	private void UpdateMesh()
	{
		List<CombineInstance> meshes = new List<CombineInstance>();

		foreach (KeyValuePair<Vector3Int, Block> item in data)
		{
			Vector3Int blockId = item.Key;
			Block block = item.Value;

			List<MeshFaceDirection> faceDirections = new List<MeshFaceDirection>();
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
							faceDirections.Add(MeshFaceDirection.FRONT);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.FRONT);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.FRONT);
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
							faceDirections.Add(MeshFaceDirection.RIGHT);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.RIGHT);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.RIGHT);
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
							faceDirections.Add(MeshFaceDirection.BACK);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.BACK);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.BACK);
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
							faceDirections.Add(MeshFaceDirection.LEFT);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.LEFT);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.LEFT);
				}
			}

			// Check Top
			if (!data.TryGetValue(blockId + Vector3Int.up, out neighbor))
			{
				faceDirections.Add(MeshFaceDirection.TOP);
			}

			// Check Bottom
			// Currently not in use
			//if (!data.TryGetValue(blockId + Vector3Int.down, out neighbor))
			//{
			//	faceDirections.Add(MeshFaceDirection.BOTTOM);
			//}

			if (faceDirections.Count > 0)
			{
				CombineInstance ci = new CombineInstance
				{
					mesh = CreateBlockMesh(faceDirections, block.BlockID)
				};
				meshes.Add(ci);
			}
		}

		meshFilter.mesh.Clear();
		meshFilter.mesh.CombineMeshes(meshes.ToArray(), true, false, false);

		meshCollider.sharedMesh = meshFilter.mesh;
	}

	private static Mesh CreateBlockMesh(List<MeshFaceDirection> faceDirections, Vector3 offset)
	{
		if (faceDirections.Count == 0)
		{
			//Debug.Log("No MeshFaceDirections provided");
			return null;
		}

		Mesh mesh = new Mesh();

		bool renderFront = faceDirections.Contains(MeshFaceDirection.FRONT);
		bool renderRight = faceDirections.Contains(MeshFaceDirection.RIGHT);
		bool renderBack = faceDirections.Contains(MeshFaceDirection.BACK);
		bool renderLeft = faceDirections.Contains(MeshFaceDirection.LEFT);
		bool renderTop = faceDirections.Contains(MeshFaceDirection.TOP);
		bool renderBottom = faceDirections.Contains(MeshFaceDirection.BOTTOM);

		Vector3 _000 = new Vector3(0, 0, 0) + offset;
		Vector3 _100 = new Vector3(1, 0, 0) + offset;
		Vector3 _110 = new Vector3(1, 1, 0) + offset;
		Vector3 _010 = new Vector3(0, 1, 0) + offset;
		Vector3 _001 = new Vector3(0, 0, 1) + offset;
		Vector3 _101 = new Vector3(1, 0, 1) + offset;
		Vector3 _111 = new Vector3(1, 1, 1) + offset;
		Vector3 _011 = new Vector3(0, 1, 1) + offset;

		List<Vector3> vertices = new List<Vector3>();

		if (renderFront)
		{
			// Front
			vertices.Add(_000);
			vertices.Add(_010);
			vertices.Add(_110);
			vertices.Add(_100);
		}

		if (renderRight)
		{
			// Right
			vertices.Add(_100);
			vertices.Add(_110);
			vertices.Add(_111);
			vertices.Add(_101);
		}

		if (renderBack)
		{
			// Back
			vertices.Add(_101);
			vertices.Add(_111);
			vertices.Add(_011);
			vertices.Add(_001);
		}

		if (renderLeft)
		{
			// Left
			vertices.Add(_001);
			vertices.Add(_011);
			vertices.Add(_010);
			vertices.Add(_000);
		}

		if (renderTop)
		{
			// Top
			vertices.Add(_010);
			vertices.Add(_011);
			vertices.Add(_111);
			vertices.Add(_110);
		}

		if (renderBottom)
		{
			// Bottom
			vertices.Add(_100);
			vertices.Add(_101);
			vertices.Add(_001);
			vertices.Add(_000);
		}

		mesh.vertices = vertices.ToArray();

		List<int> triangles = new List<int>();
		for (int i = 0; i < faceDirections.Count; i++)
		{
			triangles.Add(i * 4 + 0);
			triangles.Add(i * 4 + 1);
			triangles.Add(i * 4 + 2);
			triangles.Add(i * 4 + 0);
			triangles.Add(i * 4 + 2);
			triangles.Add(i * 4 + 3);
		}

		mesh.triangles = triangles.ToArray();

		Vector3[] normals = new Vector3[vertices.Count];
		int normalIndex = 0;

		if (renderFront)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.back;
				normalIndex++;
			}
		}

		if (renderRight)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.right;
				normalIndex++;
			}
		}

		if (renderBack)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.forward;
				normalIndex++;
			}
		}

		if (renderLeft)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.left;
				normalIndex++;
			}
		}

		if (renderTop)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.up;
				normalIndex++;
			}
		}

		if (renderBottom)
		{
			for (int i = 0; i < 4; i++)
			{
				normals[normalIndex] = Vector3.down;
				normalIndex++;
			}
		}

		mesh.normals = normals;

		Vector2[] uv = new Vector2[vertices.Count];

		for (int i = 0; i < faceDirections.Count; i += 4)
		{
			uv[i + 0] = new Vector2(0, 0);
			uv[i + 1] = new Vector2(1, 0);
			uv[i + 2] = new Vector2(1, 1);
			uv[i + 3] = new Vector2(0, 1);
		}

		mesh.uv = uv;

		Color32[] colors = new Color32[vertices.Count];
		int colorIndex = 0;

		Color32 dirt = new Color32(75, 49, 49, 255);
		Color32 grass = new Color32(150, 219, 137, 255);

		if (renderFront)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderRight)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderBack)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderLeft)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderTop)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = grass;
				colorIndex++;
			}
		}

		if (renderBottom)
		{
			for (int i = 0; i < 4; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		mesh.colors32 = colors;

		return mesh;
	}
}
