using System.Collections.Generic;
using UnityEngine;

public struct Region
{
	public string Name { get; internal set; }
	public Vector2Int RegionID { get; internal set; }
	public Vector2 RelativeVoronoiPoint { get; internal set; }

	public float Temperature { get; internal set; }
	public float Humidity { get; internal set; }

	public Vector2 VoronoiWorldPoint
	{
		get => (RegionID + RelativeVoronoiPoint) * ChunkGenerator.REGION_SIZE;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Region))
		{
			return false;
		}

		var region = (Region)obj;
		return RegionID.Equals(region.RegionID);
	}

	public override int GetHashCode() =>
		1146541480 + EqualityComparer<Vector2Int>.Default.GetHashCode(RegionID);

	public static bool operator ==(Region left, Region right) => Equals(left, right);

	public static bool operator !=(Region left, Region right) => !Equals(left, right);
}
