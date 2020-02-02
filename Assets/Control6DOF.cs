using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class Control6DOF : MonoBehaviour
{
    private MLInputController _controller;

    // Start is called before the first frame update
    void Start()
    {
        MLInput.Start();
        _controller = MLInput.GetController(MLInput.Hand.Left);
    }

    private void OnDestroy()
    {
        MLInput.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _controller.Position;
        transform.rotation = _controller.Orientation;
    }
}
