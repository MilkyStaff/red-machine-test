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
    [SerializeField, Range(0, 1), Tooltip("in percent of screenSize")] private float mapBorder = .5f;
    [SerializeField, Range(0, 1), Tooltip("0 is static, 1 is without smooth")] float dragSmooth = .2f;
    [SerializeField] bool shouldInertialMotion = true;
    private const float targetFrameRate = 60;

    private ColorConnectionManager _colorConnectionManager;
    private ClickHandler _clickHandler;

    private bool isDrug;
    private bool isSmooth;

    private float4 _bounds;
    private float2 _cameraSize;

    private Vector3 _pointerDownPosition;
    private Vector3 _cameraDefaultPosition;
    private Vector3 _smoothEndPosition;

    private void Awake()
    {
        _clickHandler = ClickHandler.Instance;
        _cameraDefaultPosition = transform.position;

        var mainCamera = CameraHolder.Instance.MainCamera;
        _cameraSize = new float2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize);

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
        if (shouldInertialMotion)
            isSmooth = true;
        _smoothEndPosition = _cameraDefaultPosition;

        _colorConnectionManager = FindAnyObjectByType<ColorConnectionManager>();
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

    private void LateUpdate()
    {
        if (isDrug)
        {
            var targetPoint = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            var dragVector = _pointerDownPosition - targetPoint;
            var newPosition = transform.position + dragVector * dragSmooth * Time.deltaTime * targetFrameRate;
            MoveTo(newPosition);
        }
        else if (isSmooth)
        {
            if (Vector3.Magnitude(transform.position - _smoothEndPosition) > .1f)
                MoveTo(Vector3.Lerp(transform.position, _smoothEndPosition, dragSmooth * Time.deltaTime * targetFrameRate));
            else
                isSmooth = false;
        }
    }

    private void OnPointerUp(Vector3 position)
    {
        if (isDrug)
        {
            isDrug = false;
            if (shouldInertialMotion)
            {
                var temp = transform.position + _pointerDownPosition - position;
                _smoothEndPosition = new Vector3(temp.x, temp.y, _cameraDefaultPosition.z);
                isSmooth = true;
            }
        }
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