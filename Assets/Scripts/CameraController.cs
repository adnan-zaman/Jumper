using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{

    public InputActionAsset playerControls;
    public float rotationSpeed;

    private CinemachineVirtualCamera virtualCamera;
    private InputAction look;

    void Awake()
    {
        InputActionMap actionMap = playerControls.FindActionMap("Player");
        actionMap.Enable();
        look = actionMap.FindAction("Look");
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        Vector2 lookDir = look.ReadValue<Vector2>().normalized;
        virtualCamera.transform.Rotate(new Vector3(0, lookDir.x * rotationSpeed * Time.deltaTime, 0), Space.World);

    }
}
