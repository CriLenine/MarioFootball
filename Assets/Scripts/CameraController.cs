using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _vCam;
    private Transform _follow = null;

    public Transform ToFollow => _follow;

    private void Awake()
    {
        _vCam = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        //The field _follow is set only one time with the first Follow transform linked to the virtual camera
        _follow ??= _vCam.Follow;

        //if this is the active camera
        if (_vCam == CameraManager.ActiveCam?.GetComponent<CinemachineVirtualCamera>())
            _vCam.Follow = CameraManager.LockerDynamic;
    }
}
