using UnityEngine;

public struct Region
{
	public string Name { get; internal set; }
	public Vector2Int RegionID { get; internal set; }
	public Vector2 RelativeVoronoiPoint { get; internal set; }

	public Vector2 VoronoiWorldPoint
	{
		get
		{
			return (RegionID + RelativeVoronoiPoint) * ChunkGenerator.REGION_SIZE;
		}
	}
}
