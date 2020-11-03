
using UnityEngine;

public class ColorSwitcher : MonoBehaviour
{
	private readonly Color _nearest = Color.blue;
	private readonly Color _notNearest = Color.white;

	private Renderer _renderer;
	private MaterialPropertyBlock _propertyBlock;

	private void Awake()
	{
		_propertyBlock = new MaterialPropertyBlock();
		_renderer = GetComponent<Renderer>();
	}

	public void SetIsClosest( bool isClosest )
	{
		_renderer.GetPropertyBlock(_propertyBlock);
		if( isClosest )
			_propertyBlock.SetColor("_Color", _nearest);
		else
			_propertyBlock.SetColor("_Color", _notNearest);

		_renderer.SetPropertyBlock(_propertyBlock);

	}
}
