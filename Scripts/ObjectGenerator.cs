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

	private void Awake()
	{
		Debug.Assert( numObjects > 1 );

		_halfRegionHeight = regionHeight * 0.5f;
	}

	private void Start()
	{
		using( new KristerTimer( $"Blue-Noise Generator (Shitty Version, {numObjects} objects)", 1 ) )
		{
			spatialPartition = new StupidVersion( prefab, transform, numObjects );
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

					spatialPartition.FindNearestPoint( candidate,
													   out GameObject ignored,
													   out float sqDistance );

					if( sqDistance > bestSqDistance )
					{
						bestCandidate  = candidate;
						bestSqDistance = sqDistance;
					}
				}

				spatialPartition.Insert( bestCandidate );
			}

			spatialPartition.Build();
		}
	}

	private Vector3 GenerateRandomPoint()
	{
		float   randomDistance  = Random.Range( 0, regionRadius );
		Vector2 randomDirection = Random.onUnitSphere * randomDistance;

		return new Vector3( randomDirection.x,
							Random.Range( -_halfRegionHeight, _halfRegionHeight ),
							randomDirection.y );
	}
}
