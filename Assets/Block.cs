using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[ExecuteInEditMode]
public class Block : MonoBehaviour
{
	public enum MeshFaceDirection
	{
		FRONT,
		RIGHT,
		BACK,
		LEFT,
		TOP,
		BOTTOM
	}

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public void Awake()
	{
		if (!GetComponent<MeshFilter>())
		{
			gameObject.AddComponent<MeshFilter>();
		}
		meshFilter = GetComponent<MeshFilter>();

		if (!GetComponent<MeshRenderer>())
		{
			gameObject.AddComponent<MeshRenderer>();
		}
		meshRenderer = GetComponent<MeshRenderer>();

		meshRenderer.material = Resources.Load<Material>("VertexColoredWithShadow");

		//InitMesh(Enum.GetValues(typeof(MeshFaceDirection)).Cast<MeshFaceDirection>().ToList());

		//meshRenderer.materials = new Material[2] {
		//    Resources.Load<Material>("Dirt"),
		//    Resources.Load<Material>("Grass")
		//};
	}

	public void InitMesh(List<MeshFaceDirection> faceDirections)
	{
		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;

		if (faceDirections.Count == 0)
		{
			//Debug.Log("No MeshFaceDirections provided");
			return;
		}

		bool renderFront = faceDirections.Contains(MeshFaceDirection.FRONT);
		bool renderRight = faceDirections.Contains(MeshFaceDirection.RIGHT);
		bool renderBack = faceDirections.Contains(MeshFaceDirection.BACK);
		bool renderLeft = faceDirections.Contains(MeshFaceDirection.LEFT);
		bool renderTop = faceDirections.Contains(MeshFaceDirection.TOP);
		bool renderBottom = faceDirections.Contains(MeshFaceDirection.BOTTOM);

		Vector3 _000 = new Vector3(0, 0, 0);
		Vector3 _100 = new Vector3(1, 0, 0);
		Vector3 _110 = new Vector3(1, 1, 0);
		Vector3 _010 = new Vector3(0, 1, 0);
		Vector3 _001 = new Vector3(0, 0, 1);
		Vector3 _101 = new Vector3(1, 0, 1);
		Vector3 _111 = new Vector3(1, 1, 1);
		Vector3 _011 = new Vector3(0, 1, 1);

		List<Vector3> vertices = new List<Vector3>();

		if (renderFront)
		{
			// Front
			vertices.Add(_000);
			vertices.Add(_010);
			vertices.Add(_100);

			vertices.Add(_010);
			vertices.Add(_110);
			vertices.Add(_100);
		}

		if (renderRight)
		{
			// Right
			vertices.Add(_100);
			vertices.Add(_110);
			vertices.Add(_101);

			vertices.Add(_110);
			vertices.Add(_111);
			vertices.Add(_101);
		}

		if (renderBack)
		{
			// Back
			vertices.Add(_101);
			vertices.Add(_111);
			vertices.Add(_001);

			vertices.Add(_111);
			vertices.Add(_011);
			vertices.Add(_001);
		}

		if (renderLeft)
		{
			// Left
			vertices.Add(_001);
			vertices.Add(_011);
			vertices.Add(_000);

			vertices.Add(_011);
			vertices.Add(_010);
			vertices.Add(_000);
		}

		if (renderTop)
		{
			// Top
			vertices.Add(_010);
			vertices.Add(_011);
			vertices.Add(_110);

			vertices.Add(_011);
			vertices.Add(_111);
			vertices.Add(_110);
		}

		if (renderBottom)
		{
			// Bottom
			vertices.Add(_100);
			vertices.Add(_101);
			vertices.Add(_000);

			vertices.Add(_000);
			vertices.Add(_101);
			vertices.Add(_001);
		}

		mesh.vertices = vertices.ToArray();

		List<int> triangles = new List<int>();
		for (int i = 0; i < vertices.Count; i++)
		{
			triangles.Add(i);
		}

		mesh.triangles = triangles.ToArray();

		Vector3[] normals = new Vector3[vertices.Count];
		int normalIndex = 0;

		if (renderFront)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.back;
				normalIndex++;
			}
		}

		if (renderRight)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.right;
				normalIndex++;
			}
		}

		if (renderBack)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.forward;
				normalIndex++;
			}
		}

		if (renderLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.left;
				normalIndex++;
			}
		}

		if (renderTop)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.up;
				normalIndex++;
			}
		}

		if (renderBottom)
		{
			for (int i = 0; i < 6; i++)
			{
				normals[normalIndex] = Vector3.down;
				normalIndex++;
			}
		}

		mesh.normals = normals;

		//Vector2[] uv = new Vector2[vertices.Count];

		//int faces = faceDirections.Count;
		//for (int i = 0; i < faceDirections.Count; i++)
		//{
		//    uv[i * faces + 0] = new Vector2(0, 0);
		//    uv[i * faces + 1] = new Vector2(1, 0);
		//    uv[i * faces + 2] = new Vector2(0, 1);
		//    uv[i * faces + 3] = new Vector2(0, 1);
		//    uv[i * faces + 4] = new Vector2(1, 0);
		//    uv[i * faces + 5] = new Vector2(1, 1);
		//}

		//mesh.uv = uv;

		Color32[] colors = new Color32[vertices.Count];
		int colorIndex = 0;

		Color32 dirt = new Color32(75, 49, 49, 255);
		Color32 grass = new Color32(150, 219, 137, 255);

		if (renderFront)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderRight)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderBack)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderLeft)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		if (renderTop)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = grass;
				colorIndex++;
			}
		}

		if (renderBottom)
		{
			for (int i = 0; i < 6; i++)
			{
				colors[colorIndex] = dirt;
				colorIndex++;
			}
		}

		mesh.colors32 = colors;
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
