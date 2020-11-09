#define KDTREE

using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
	[SerializeField] private GameObject prefab           = default;
	[SerializeField] private int        numObjects       = 512;
	[SerializeField] private int        sampleMultiplier = 1;
	[SerializeField] private float      regionRadius     = 50.0f;
	[SerializeField] private float      regionHeight     = 5.0f;

	public ISpatialPartition spatialPartition;

	private float _halfRegionHeight;

	static float SqEuclidean( Vector3 a, Vector3 b ) => (a - b).sqrMagnitude;

	static float Euclidean( Vector3 a, Vector3 b ) => (a - b).magnitude;

	static float SqRectilinear( Vector3 a, Vector3 b )
	{
		return (a.x - b.x).Square()
			   + (a.y - b.y).Square()
			   + (a.z - b.z).Square();
	}

	static float Rectilinear( Vector3 a, Vector3 b )
	{
		return Mathfs.Abs( a.x - b.x )
			   + Mathfs.Abs( a.y - b.y )
			   + Mathfs.Abs( a.z - b.z );
	}

	static float Chebyshev( Vector3 a, Vector3 b )
	{
		var dist = a - b;

		return Mathfs.Max( Mathfs.Abs( dist.x ),
						   Mathfs.Abs( dist.y ),
						   Mathfs.Abs( dist.z ) );
	}

	static float SqChebyshev( Vector3 a, Vector3 b )
	{
		var dist = a - b;

		return Mathfs.Max( dist.x.Square(),
						   dist.y.Square(),
						   dist.z.Square() );
	}

	private void Awake()
	{
		Debug.Assert( numObjects > 1 );

		_halfRegionHeight = regionHeight * 0.5f;
	}

	private void Start()
	{
#if STUPID
		using( new KristerTimer(
			$"Blue-Noise Generator (Shitty Version, {numObjects} objects)",
			1 ) )
		{
			spatialPartition =
				new StupidVersion( prefab,
								   transform,
								   SqRectilinear,
								   numObjects );
#elif KDTREE
		using( new KristerTimer( $"Blue-Noise Generator (k-d Tree Version, {numObjects} objects)", 1 ) )
		{
			spatialPartition =
				new KdTree( prefab, transform, SqEuclidean, numObjects );
#elif OCTREE
		using( new KristerTimer( $"Blue-Noise Generator (Octree Version, {numObjects} objects)", 1 ) )
		{
			spatialPartition = new Octree( prefab, transform, numObjects );
#elif OFFSETOCTREE
		using( new KristerTimer( $"Blue-Noise Generator (Offset Octree Version, {numObjects} objects)", 1 ) )
		{
			spatialPartition =
 new OffsetOctree( prefab, transform, numObjects );
#endif

			spatialPartition.Insert( Vector3.zero );

			for( int pointIndex = 1;
				 pointIndex < numObjects;
				 pointIndex++ )
			{
				var     bestSqDistance = float.MinValue;
				Vector3 bestCandidate  = default;

				for( int candidateIndex = 0;
					 candidateIndex < (pointIndex * sampleMultiplier) + 1;
					 candidateIndex++ )
				{
					var candidate = GenerateRandomPoint();

					float sqDistance = float.MinValue;
					spatialPartition.FindNearestPoint( candidate,
													   out GameObject ignored,
													   out sqDistance );

					if( sqDistance > bestSqDistance )
					{
						bestCandidate  = candidate;
						bestSqDistance = sqDistance;
					}
				}

				spatialPartition.Insert( bestCandidate );
			}
		}
	}

	private Vector3 GenerateRandomPoint()
	{
		float   randomDistance  = Random.Range( 0, regionRadius );
		Vector2 randomDirection = Random.onUnitSphere * randomDistance;

		return new Vector3( randomDirection.x,
							Random.Range( -_halfRegionHeight,
										  _halfRegionHeight ),
							randomDirection.y );
	}
}
