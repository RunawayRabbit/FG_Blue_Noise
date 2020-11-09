using System;
using UnityEngine;

public class StupidVersion : ISpatialPartition
{
	private GameObject[] _objects;
	private int          _objectCount;

	private GameObject                    _prefab;
	private Transform                     _parent;
	private Func<Vector3, Vector3, float> _measure;

	public StupidVersion( GameObject                    prefab,
						  Transform                     parent,
						  Func<Vector3, Vector3, float> measure,
						  int                           capacity )
	{
		_prefab  = prefab;
		_parent  = parent;
		_measure = measure;

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
			float sqDistance =
				_measure( _objects[i].transform.position, queryPoint );

			if( sqDistance < outSqDist )
			{
				outSqDist     = sqDistance;
				nearestObject = _objects[i];
			}
		}
	}

	public void DebugFindNearestPoint( Vector3        queryPoint,
									   out GameObject nearestObject,
									   out float      outSqDist )
	{
		using( new KristerTimer( "FindNearestPoint call (Stupid Version)", 1 ) )
		{
			FindNearestPoint( queryPoint, out nearestObject, out outSqDist );
		}
	}

	public void Insert( Vector3 position )
	{
		_objects[_objectCount++] = GameObject.Instantiate( _prefab,
														   position,
														   Quaternion.identity,
														   _parent );
	}
}
