using UnityEngine;

public struct Block
{
	public string Name { get; internal set; }
	public Region Region { get; internal set; }
	public Chunk Chunk { get; internal set; }
	public Vector3Int BlockID { get; internal set; }
}
