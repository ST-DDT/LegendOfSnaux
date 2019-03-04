using System;
using System.Collections.Generic;
using UnityEngine;

readonly public struct Region : IEquatable<Region>
{
	public readonly string Name;
	public readonly Vector2Int RegionID;
	public readonly Vector2 RelativeVoronoiPoint;
	public readonly float Temperature;
	public readonly float Humidity;

	public Vector2 VoronoiWorldPoint
	{
		get => (RegionID + RelativeVoronoiPoint) * WorldGenerator.REGION_SIZE;
	}

	public Region(string name, Vector2Int regionID, Vector2 relativeVoronoiPoint, float temperature, float humidity)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		RegionID = regionID;
		RelativeVoronoiPoint = relativeVoronoiPoint;
		Temperature = temperature;
		Humidity = humidity;
	}

	public override bool Equals(object obj) => obj is Region && Equals((Region)obj);

	public bool Equals(Region other) => RegionID.Equals(other.RegionID);


	public override int GetHashCode() =>
	   1146541480 + EqualityComparer<Vector2Int>.Default.GetHashCode(RegionID);

	public static bool operator ==(Region left, Region right) => Equals(left, right);

	public static bool operator !=(Region left, Region right) => !Equals(left, right);
}
