using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block
{
	public string Name { get; internal set; }
	public Chunk Chunk { get; internal set; }
	public Vector3Int BlockID { get; internal set; }
}
