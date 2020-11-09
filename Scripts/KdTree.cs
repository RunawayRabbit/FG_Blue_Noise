using System;
using System.Collections.Generic;
using UnityEngine;

public class KdTree : ISpatialPartition
{
	private const int K = 3;

	private struct Sphere
	{
		public Vector3 center;
		public float   radius;

		public Sphere( Vector3 center, float radius )
		{
			this.center = center;
			this.radius = radius;
		}
	}

	private abstract class Node { }

	private struct LeafNodeEntry
	{
		public int     ID;
		public Vector3 position;
	}

	private class LeafNode : Node
	{
		public LeafNodeEntry[] entries;

		public int objectCount;

		public LeafNode( int capacity )
		{
			objectCount = 0;
			entries      = new LeafNodeEntry[capacity];

			for( int i = 0;
				 i < capacity;
				 i++ )
			{
				entries[i].ID = int.MaxValue;
				entries[i].position = Vector3.positiveInfinity;
			}
		}

		public LeafNode( List<LeafNodeEntry> entries, int capacity )
		{
			this.entries = new LeafNodeEntry[capacity];
			objectCount = entries.Count;

			for( int i = 0;
				 i < capacity;
				 i++ )
			{
				this.entries[i].ID       = int.MaxValue;
				this.entries[i].position = Vector3.positiveInfinity;
			}

			for( int i = 0;
				 i < entries.Count;
				 i++ )
			{
				this.entries[i] = entries[i];
			}
		}

		public bool TryAdd( Vector3 position, int id )
		{
			if( objectCount == entries.Length ) return false;

			entries[objectCount].ID       = id;
			entries[objectCount].position = position;

			objectCount++;

			return true;
		}
	}

	private class TreeNode : Node
	{
		public          Node  left;
		public          Node  right;
		public readonly int   splitAxis;
		public readonly float splitValue;

		public TreeNode( float splitValue,
						 int   splitAxis )
		{
			this.splitValue = splitValue;
			this.splitAxis  = splitAxis;

			this.right = null;
			this.left  = null;
		}
	}

	private readonly List<GameObject> _objectStore;

	private readonly int _leafCapacity;
	private readonly int _halfSize;

	private GameObject _prefab;
	private Transform  _parent;

	private Node _root;

	private readonly Func<Vector3, Vector3, float> _measure;

	public KdTree( GameObject                    prefab,
				   Transform                     parent,
				   Func<Vector3, Vector3, float> measure,
				   int                           capacity )
	{
		_measure      = measure;
		_objectStore  = new List<GameObject>( capacity );
		_prefab       = prefab;
		_parent       = parent;
		_leafCapacity = Mathfs.Max( 2, Mathfs.FloorToInt( capacity * 0.01f ) );
		_halfSize     = Mathf.CeilToInt( _leafCapacity * 0.5f );
	}

	public void FindNearestPoint( Vector3        queryPoint,
								  out GameObject nearestObject,
								  out float      nearestSqDist )
	{
		var     searchRadius = new Sphere( queryPoint, float.MaxValue );
		Vector3 nearestPoint = searchRadius.center;

		int objectIndex = -1;
		VisitNode( _root, ref searchRadius, ref nearestPoint, ref objectIndex );

		nearestObject = _objectStore[objectIndex];
		nearestSqDist = searchRadius.radius;
	}

	public void Insert( Vector3 position )
	{
		// Handle GameObject Creation
		int id = CreateGameObject( position );


		// If we have no tree, start by making one!
		if( _root == null )
		{
			CreateRootElement( position, id );

			return;
		}

		_root = InsertIntoNode( _root, position, id, 0 );
	}

	private Node InsertIntoNode( Node    node,
								 Vector3 position,
								 int     id,
								 int     iterDepth )
	{

		if( node is LeafNode leafNode )
			return InsertIntoLeafNode( leafNode, position, id, iterDepth );

		if( node is TreeNode treeNode )
			return InsertIntoTreeNode( treeNode, position, id, iterDepth + 1 );


		throw new ArgumentException(
			"InsertIntoNode was passed a node type that it doesn't understand." );
	}

	private Node InsertIntoLeafNode( LeafNode leafNode,
									 Vector3  position,
									 int      id,
									 int      iterDepth )
	{
		// Attempt to put our position into this leaf node.
		if( leafNode.TryAdd( position, id ) ) return leafNode;


		//@BUG: Sanity check. Am I handling iterDepth correctly here?

		// leafNode is full! Branch!
		TreeNode newTreeNode = BranchLeafNode( leafNode, iterDepth );
		InsertIntoTreeNode( newTreeNode, position, id, iterDepth + 1 );

		return newTreeNode;
	}

	private Node InsertIntoTreeNode( TreeNode node,
									 Vector3  position,
									 int      id,
									 int      iterDepth )
	{
		if( position[node.splitAxis] > node.splitValue )
			node.right = InsertIntoNode( node.right, position, id, iterDepth + 1 );

		else
			node.left = InsertIntoNode( node.left, position, id, iterDepth + 1 );

		return node;
	}

	private TreeNode BranchLeafNode( LeafNode leafNode, int iterDepth )
	{
		int dim = iterDepth % K;

		var temp = new List<LeafNodeEntry>( leafNode.entries );

		temp.Sort( ( a, b ) =>
					   a.position[dim].CompareTo( b.position[dim] ) );

		var pivot = temp[_halfSize];

		var result = new TreeNode( pivot.position[dim], dim );

		var leftHalf = temp.GetRange( 0, _halfSize );

		var rightHalf =
			temp.GetRange( leftHalf.Count, temp.Count - leftHalf.Count );

		result.left  = new LeafNode( leftHalf, _leafCapacity );
		result.right = new LeafNode( rightHalf, _leafCapacity );

		return result;
	}


	private int CreateGameObject( Vector3 position )
	{
		if( _objectStore.Count == _objectStore.Capacity )
		{
			throw new IndexOutOfRangeException( "Tree is \"full!\"" );
		}

		_objectStore.Add( GameObject.Instantiate( _prefab,
												  position,
												  Quaternion.identity,
												  _parent ) );

		return _objectStore.Count - 1;
	}

	private void CreateRootElement( Vector3 position, int id )
	{
		_root = new LeafNode( _leafCapacity );
		if( _root is LeafNode leafAtRoot ) leafAtRoot.TryAdd( position, id );
	}

	private void VisitNode( Node        node,
							ref Sphere  sphere,
							ref Vector3 nearestPoint,
							ref int     objectIndex )
	{
		if( node == null )
		{
			throw new InvalidOperationException(
				"Attempt to query an empty k-d tree!" );
		}

		if( node is LeafNode leafNode )
		{
			VisitLeafNode( leafNode, ref sphere, ref objectIndex );
		}
		else if( node is TreeNode treeNode )
		{
			VisitTreeNode( treeNode,
						   ref sphere,
						   ref nearestPoint,
						   ref objectIndex );
		}
	}

	private void VisitLeafNode( LeafNode   leafNode,
								ref Sphere sphere,
								ref int    objectIndex )
	{
		for( int i = 0;
			 i < leafNode.objectCount;
			 i++ )
		{
			if( leafNode.entries[i].ID < 0 ) continue;

			var distance = _measure( sphere.center,
									 leafNode.entries[i].position );

			if( distance < sphere.radius )
			{
				sphere.radius = distance;
				objectIndex   = leafNode.entries[i].ID;
			}
		}
	}

	private void VisitTreeNode( TreeNode    node,
								ref Sphere  sphere,
								ref Vector3 nearestPoint,
								ref int     objectIndex )
	{
		// Establish which side to recurse into first
		Node ourSide;
		Node oppositeSide;

		if( sphere.center[node.splitAxis] < node.splitValue )
		{
			ourSide      = node.left;
			oppositeSide = node.right;
		}
		else
		{
			ourSide      = node.right;
			oppositeSide = node.left;
		}

		// Down the rabbit hole..
		VisitNode( ourSide, ref sphere, ref nearestPoint, ref objectIndex );

		// newNearestPoint that is accurate for this iteration depth
		var newNearestPoint = nearestPoint;
		newNearestPoint[node.splitAxis] = node.splitValue;


		// Search radius may include the other side of the tree. If it does, we have to visit both.
		if( _measure( newNearestPoint, sphere.center )
			< sphere.radius )
		{
			VisitNode( oppositeSide,
					   ref sphere,
					   ref newNearestPoint,
					   ref objectIndex );
		}
	}
}
