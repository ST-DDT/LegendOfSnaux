using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
    static double Scale(double value, double min, double max)
    {
        return min + ((value + 1) / 2) * (max - min);
    }

    public void Awake()
    {
        Dictionary<Vector3Int, bool> chunkNoiseData = new Dictionary<Vector3Int, bool>();

        SimplexNoise simplexNoise = new SimplexNoise();

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                //float noise = Mathf.PerlinNoise((transform.position.x + x) / 16.0f, (transform.position.z + z) / 16.0f);
                //double scaledNoise = Scale(noise, 0, 3);
                double noise = simplexNoise.Eval((transform.position.x + x) / 16.0, (transform.position.z + z) / 16.0);
                double scaledNoise = Scale(noise, 0, 3);
                for (int y = 0; y < scaledNoise; y++)
                {
                    chunkNoiseData.Add(new Vector3Int(x, y, z), scaledNoise > y);
                }
            }
        }

        foreach (var item in chunkNoiseData)
        {
            if (!item.Value)
            {
                continue;
            }

            Vector3Int position = item.Key;
            List<Block.MeshFaceDirection> faceDirections = new List<Block.MeshFaceDirection>();
            bool value;

            // Check Front
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.RoundToInt(Vector3.back), out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.FRONT);
            }

            // Check Right
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.right, out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.RIGHT);
            }

            // Check Back
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.RoundToInt(Vector3.forward), out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.BACK);
            }

            // Check Left
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.left, out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.LEFT);
            }

            // Check Top
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.up, out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.TOP);
            }

            // Check Bottom
            if (!chunkNoiseData.TryGetValue(position + Vector3Int.down, out value) || !value)
            {
                faceDirections.Add(Block.MeshFaceDirection.BOTTOM);
            }

            GameObject goBlock = new GameObject();
            Block block = goBlock.AddComponent<Block>();
            block.Awake();
            block.InitMesh(faceDirections);
            goBlock.transform.parent = gameObject.transform;
            goBlock.transform.localPosition = position;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
