using System;
using System.Collections.Generic;
using UnityEngine;

readonly public struct Block : IEquatable<Block>
{
	public readonly string Name;
	public readonly Region Region;
	public readonly Chunk Chunk;
	public readonly Vector3Int BlockID;

	public Block(string name, Region region, Chunk chunk, Vector3Int blockID) : this()
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Region = region;
		Chunk = chunk ?? throw new ArgumentNullException(nameof(chunk));
		BlockID = blockID;
	}

	public override bool Equals(object obj) => obj is Block && Equals((Block)obj);

	public bool Equals(Block other) =>
		EqualityComparer<Chunk>.Default.Equals(Chunk, other.Chunk) && BlockID.Equals(other.BlockID);

	public override int GetHashCode()
	{
		int hashCode = 370604417;
		hashCode = hashCode * -1521134295 + EqualityComparer<Chunk>.Default.GetHashCode(Chunk);
		hashCode = hashCode * -1521134295 + EqualityComparer<Vector3Int>.Default.GetHashCode(BlockID);
		return hashCode;
	}

	public static bool operator ==(Block left, Block right) => Equals(left, right);


	public static bool operator !=(Block left, Block right) => !Equals(left, right);
}
