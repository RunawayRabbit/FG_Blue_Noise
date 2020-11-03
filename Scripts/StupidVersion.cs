using System;
using UnityEngine;

[Serializable]
public class StupidVersion : ISpatialPartition
{
	private GameObject[] _objects;
	private int          _objectCount;

	private GameObject _prefab;
	private Transform _parent;

	public StupidVersion( GameObject prefab, Transform parent, int capacity )
	{
		_prefab = prefab;
		_parent = parent;

		_objects     = new GameObject[capacity];
		_objectCount = 0;
	}


	public void FindNearestPoint( Vector3        queryPoint,
								  out GameObject nearestObject,
								  out float      outSqDist )
	{
		outSqDist     = float.PositiveInfinity;
		nearestObject = null;

		if( _objectCount < 0 ) return;

		for( int i = 0;
			 i < _objectCount;
			 i++ )
		{
			float sqDistance = (_objects[i].transform.position - queryPoint)
			   .sqrMagnitude;

			if( sqDistance < outSqDist )
			{
				outSqDist     = sqDistance;
				nearestObject = _objects[i];
			}
		}
	}

	public void Insert( Vector3 position )
	{
		_objects[_objectCount++] = GameObject.Instantiate( _prefab,
														   position,
														   Quaternion.identity,
														   _parent );
	}

	public void Build() { }
}
