using Camera;
using Connection;
using Player.ActionHandlers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Scenes;

[RequireComponent(typeof(CameraHolder))]
public class CameraMover : MonoBehaviour
{
    [SerializeField, Range(0, 1), Tooltip("0 is static, 1 is without smooth")] float dragSmooth = .2f;
    [SerializeField, Range(0, 1), Tooltip("in percent of screenSize")] private float mapBorder = .5f;

    private ColorConnectionManager _colorConnectionManager;
    private ClickHandler _clickHandler;

    private bool isDrug;

    private float4 _bounds;
    private float2 _cameraSize;

    private Vector3 _pointerDownPosition;
    private Vector3 _cameraDefaultPosition;

    private void Awake()
    {
        _clickHandler = ClickHandler.Instance;
        _cameraDefaultPosition = transform.position;

        var tempCamera = GetComponent<CameraHolder>().MainCamera;
        _cameraSize = new float2(tempCamera.orthographicSize * tempCamera.aspect, tempCamera.orthographicSize);


        ScenesChanger.SceneLoadedEvent += OnSceneLoadedEvent;
        _clickHandler.PointerDownEvent += OnPointerDown;
        _clickHandler.PointerUpEvent += OnPointerUp;
    }

    private void OnDestroy()
    {
        ScenesChanger.SceneLoadedEvent -= OnSceneLoadedEvent;
        _clickHandler.PointerDownEvent -= OnPointerDown;
        _clickHandler.PointerUpEvent -= OnPointerUp;
    }

    private void OnSceneLoadedEvent()
    {
        transform.position = _cameraDefaultPosition;
        _colorConnectionManager = FindAnyObjectByType<ColorConnectionManager>();//TODO: check how better

        SetBounds(_colorConnectionManager.GetNodesBounds());
    }

    private void OnPointerDown(Vector3 position)
    {
        if (_colorConnectionManager != null && !_colorConnectionManager.TryGetColorNodeInPosition(position, out _) && !EventSystem.current.IsPointerOverGameObject())
        {
            isDrug = true;
            _pointerDownPosition = position;
        }
    }

    private void FixedUpdate()
    {
        if (isDrug)
        {
            var targetPoint = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            var dragVector = _pointerDownPosition - targetPoint;
            var newPosition = transform.position + dragVector * dragSmooth;
            MoveTo(newPosition);
        }
    }

    private void OnPointerUp(Vector3 position)
    {
        if (isDrug)
            isDrug = false;
    }

    private void MoveTo(Vector3 targetPosition)
    {
        var tmpCameraPosition = transform.position;

        tmpCameraPosition.x = Mathf.Clamp(targetPosition.x, _bounds.x, _bounds.y);
        tmpCameraPosition.y = Mathf.Clamp(targetPosition.y, _bounds.z, _bounds.w);

        transform.position = tmpCameraPosition;
    }


    private void SetBounds(float4 mapBounds)
    {
        var tempBorderValue = _cameraSize * mapBorder;
        _bounds = new float4(
            Mathf.Min(mapBounds.x + tempBorderValue.x, - _cameraSize.x + tempBorderValue.x),
            Mathf.Max(mapBounds.y - tempBorderValue.x, _cameraSize.x - tempBorderValue.x),
            Mathf.Min(mapBounds.z + tempBorderValue.y, - _cameraSize.y + tempBorderValue.y),
            Mathf.Max(mapBounds.w - tempBorderValue.y, _cameraSize.y - tempBorderValue.y)
        );
    }
}
