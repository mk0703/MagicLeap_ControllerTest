using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class NewBehaviourScript : MonoBehaviour
{
    private MLInputController mLInputController;
    // Start is called before the first frame update
    void Start()
    {
        MLInput.Start();
        mLInputController = MLInput.GetController(MLInput.Hand.Left);
    }

    void OnDestory()
    {
        MLInput.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mLInputController.Position;
        transform.rotation = mLInputController.Orientation;
    }
}
