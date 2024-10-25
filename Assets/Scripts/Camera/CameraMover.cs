using Camera;
using Connection;
using Player.ActionHandlers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Scenes;

[RequireComponent(typeof(CameraHolder))]
public class CameraMover : MonoBehaviour
{
    [SerializeField, Range(0, 1), Tooltip("0 is static, 1 is without smooth")] float _dragSmooth = .2f;
    private ColorConnectionManager colorConnectionManager;

    private bool isDrug;

    private ClickHandler _clickHandler;

    private Vector3 _pointerDownPosition;
    private Vector3 _cameraDefaultPosition;

    private void Awake()
    {
        _clickHandler = ClickHandler.Instance;
        _cameraDefaultPosition = transform.position;

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
        colorConnectionManager = FindAnyObjectByType<ColorConnectionManager>();//TODO: check how better
                                                                               
        //update bounds here
    }

    private void OnPointerDown(Vector3 position)
    {
        if (colorConnectionManager != null && !colorConnectionManager.TryGetColorNodeInPosition(position, out _) && !EventSystem.current.IsPointerOverGameObject())
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
            var newPosition = transform.position + dragVector * _dragSmooth;
            transform.position = new Vector3(newPosition.x, newPosition.y, _cameraDefaultPosition.z) ;
        }
    }

    private void OnPointerUp(Vector3 position)
    {
        if (isDrug)
            isDrug = false;
    }

}
