using UnityEngine;

public class NearestObjectHighlighter : MonoBehaviour
{
	[SerializeField] private GameObject generator = default;

	private ISpatialPartition _spatialPartition;
	private GameObject        _curNearest;

	private void Start()
	{
		var objectGenerator = generator.GetComponent<ObjectGenerator>();
		_spatialPartition = objectGenerator.spatialPartition;
	}

	private void Update()
	{
		if( _spatialPartition == null ) Start();

		_spatialPartition.FindNearestPoint( transform.position,
											out GameObject nearestObject,
											out float nearestSqDist );

		UpdateNearest( nearestObject );
	}

	private void UpdateNearest( GameObject nearestObject )
	{
		if( nearestObject == _curNearest
			|| nearestObject == null )
			return;

		if( _curNearest != null )
			_curNearest.GetComponent<ColorSwitcher>().SetIsClosest( false );

		_curNearest = nearestObject;
		_curNearest.GetComponent<ColorSwitcher>().SetIsClosest( true );
	}
}
