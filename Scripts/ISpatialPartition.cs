using UnityEngine;

public interface ISpatialPartition
{
	void FindNearestPoint( Vector3        queryPoint,
						   out GameObject nearestObject,
						   out float      nearestSqDist );

	void Insert( Vector3 position );

	void Build();
}
