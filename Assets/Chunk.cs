using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour, IEquatable<Chunk>
{
	private readonly Dictionary<Vector3Int, Block> data = new Dictionary<Vector3Int, Block>();

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private MeshCollider meshCollider;

	public WorldGenerator ChunkGenerator { get; internal set; }
	public Vector3Int ChunkID { get; internal set; }
	public bool Dirty { get; set; } = false;
	private bool currentlyUpdating = false;

	private void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();

		meshRenderer.material = Resources.Load<Material>("VertexColoredWithShadow");
	}

	private void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("Ground");

		Vector3 chunkWorldPosition = transform.position;
		StartCoroutine(BuildChunk(chunkWorldPosition));
	}

	private void LateUpdate()
	{
		if (Dirty == false)
		{
			// No updates todo
			return;
		}

		Dirty = false;

		if (currentlyUpdating == true)
		{
			//Debug.Log("Skip updating the chunk mesh as an update is already in progress");
			return;
		}
		currentlyUpdating = true;

		StartCoroutine(UpdateMesh());
	}

	private void OnDestroy()
	{
		//Debug.Log($"Destroy Chunk {ChunkID}");
		meshFilter.mesh.Clear();
	}

	private IEnumerator BuildChunk(Vector3 chunkWorldPosition)
	{
		byte i = 0;
		for (byte x = 0; x < WorldGenerator.CHUNK_SIZE; x++)
		{
			float blockWorldPositionX = chunkWorldPosition.x + x * WorldGenerator.BLOCK_SIZE;
			float noiseX = blockWorldPositionX / WorldGenerator.CHUNK_SIZE;
			for (byte z = 0; z < WorldGenerator.CHUNK_SIZE; z++)
			{
				if (i > 8)
				{
					i = 0;
					yield return null;
				}
				i++;
				float blockWorldPositionZ = chunkWorldPosition.z + z * WorldGenerator.BLOCK_SIZE;
				float noiseZ = blockWorldPositionZ / WorldGenerator.CHUNK_SIZE;

				/*
				 * TODO: We can optimize this later by first checking all 4 chunk pages.
				 * If all are in the same region, all blocks in that chunk are in the same region.
				 */
				float deltaVoronoi = (float)ChunkGenerator.NoiseGenerator
					.Eval(noiseX / 4, noiseZ / 4) * 32;
				Region region = ChunkGenerator.GetNearestRegion(
					blockWorldPositionX + deltaVoronoi,
					blockWorldPositionZ + deltaVoronoi
				);

				float noise = (float)ChunkGenerator.NoiseGenerator.Octave(
					numIterations: 6,
					x: noiseX,
					y: noiseZ,
					persistence: 0.6f,
					scale: 0.05f,
					low: 0,
					high: 64
				);

				for (byte y = 0; y < noise; y++)
				{
					Block block = new Block(
						name: $"Block x:{x}, y:{y}, z:{z}",
						chunk: this,
						blockID: new Vector3Int(x, y, z),
						region: region
					);

					data.Add(block.BlockID, block);
				}
			}
		}

		Dirty = true;
	}

	private IEnumerator UpdateMesh()
	{
		List<CombineInstance> meshes = new List<CombineInstance>();

		byte i = 0;
		foreach (KeyValuePair<Vector3Int, Block> item in data)
		{
			if (i > 64)
			{
				i = 0;
				yield return null;
			}
			i++;
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
						if (!chunk.data.TryGetValue(new Vector3Int(blockId.x, blockId.y, WorldGenerator.CHUNK_SIZE - 1), out neighbor))
						{
							faceDirections.Add(MeshFaceDirection.Front);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.Front);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.Front);
				}
			}

			// Check Right
			if (!data.TryGetValue(blockId + Vector3Int.right, out neighbor))
			{
				if (blockId.x == WorldGenerator.CHUNK_SIZE - 1)
				{
					if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.right, out Chunk chunk))
					{
						if (!chunk.data.TryGetValue(new Vector3Int(0, blockId.y, blockId.z), out neighbor))
						{
							faceDirections.Add(MeshFaceDirection.Right);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.Right);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.Right);
				}
			}

			// Check Back
			Vector3Int v3iForward = Vector3Int.RoundToInt(Vector3.forward);
			if (!data.TryGetValue(blockId + v3iForward, out neighbor))
			{
				if (blockId.z == WorldGenerator.CHUNK_SIZE - 1)
				{
					if (this.ChunkGenerator.TryGetChunk(this.ChunkID + v3iForward, out Chunk chunk))
					{
						if (!chunk.data.TryGetValue(new Vector3Int(blockId.x, blockId.y, 0), out neighbor))
						{
							faceDirections.Add(MeshFaceDirection.Back);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.Back);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.Back);
				}
			}

			// Check Left
			if (!data.TryGetValue(blockId + Vector3Int.left, out neighbor))
			{
				if (blockId.x == 0)
				{
					if (this.ChunkGenerator.TryGetChunk(this.ChunkID + Vector3Int.left, out Chunk chunk))
					{
						if (!chunk.data.TryGetValue(new Vector3Int(WorldGenerator.CHUNK_SIZE - 1, blockId.y, blockId.z), out neighbor))
						{
							faceDirections.Add(MeshFaceDirection.Left);
						}
					}
					else
					{
						faceDirections.Add(MeshFaceDirection.Left);
					}
				}
				else
				{
					faceDirections.Add(MeshFaceDirection.Left);
				}
			}

			// Check Top
			if (!data.TryGetValue(blockId + Vector3Int.up, out neighbor))
			{
				faceDirections.Add(MeshFaceDirection.Top);
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
					mesh = CreateBlockMesh(ref faceDirections, ref block)
				};
				meshes.Add(ci);
			}
		}

		meshFilter.mesh.Clear();
		meshFilter.mesh.CombineMeshes(meshes.ToArray(), true, false, false);

		meshCollider.sharedMesh = meshFilter.mesh;

		currentlyUpdating = false;
	}

	private static Mesh CreateBlockMesh(ref List<MeshFaceDirection> faceDirections, ref Block block)
	{
		if (faceDirections.Count == 0)
		{
			//Debug.Log("No MeshFaceDirections provided");
			return null;
		}

		Mesh mesh = new Mesh();

		bool renderFront = faceDirections.Contains(MeshFaceDirection.Front);
		bool renderRight = faceDirections.Contains(MeshFaceDirection.Right);
		bool renderBack = faceDirections.Contains(MeshFaceDirection.Back);
		bool renderLeft = faceDirections.Contains(MeshFaceDirection.Left);
		bool renderTop = faceDirections.Contains(MeshFaceDirection.Top);
		bool renderBottom = faceDirections.Contains(MeshFaceDirection.Bottom);

		Vector3 offset = block.BlockID;
		Vector3 _000 = (new Vector3(0, 0, 0) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _100 = (new Vector3(1, 0, 0) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _110 = (new Vector3(1, 1, 0) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _010 = (new Vector3(0, 1, 0) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _001 = (new Vector3(0, 0, 1) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _101 = (new Vector3(1, 0, 1) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _111 = (new Vector3(1, 1, 1) + offset) * WorldGenerator.BLOCK_SIZE;
		Vector3 _011 = (new Vector3(0, 1, 1) + offset) * WorldGenerator.BLOCK_SIZE;

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
			// TODO: This is currently only for debugging
			Color color = Color.HSVToRGB(block.Region.RelativeVoronoiPoint.x, 0.9f, 1f);
			Color32 grass = color;
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

	public override bool Equals(object obj) => Equals(obj as Chunk);

	public bool Equals(Chunk other) => other != null && base.Equals(other) && ChunkID.Equals(other.ChunkID);

	public override int GetHashCode()
	{
		int hashCode = -1785486011;
		hashCode = hashCode * -1521134295 + base.GetHashCode();
		hashCode = hashCode * -1521134295 + EqualityComparer<Vector3Int>.Default.GetHashCode(ChunkID);
		return hashCode;
	}

	public static bool operator ==(Chunk left, Chunk right) => Equals(left, right);

	public static bool operator !=(Chunk left, Chunk right) => !Equals(left, right);

}
