using UnityEngine;
using UnityEngine.XR;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using UnityEditor;

namespace Unity.XR.MagicLeap.Tests
{
    public class CameraCheck : TestBaseSetup
    {
        private bool m_RaycastHit = false;
#pragma warning disable 0414
        private bool m_DidSaveScreenCapture = false;
#pragma warning restore 0414
        private string m_FileName;

        private float m_StartingScale;
        private float m_StartingZoomAmount;
        private float m_StartingRenderScale;
        private float kDeviceSetupWait = 1f;

        private Texture2D m_MobileTexture;

        List<XRNodeState> m_XrNodeStates;
        XRNode m_XrNodes;
        XRNode m_Head;
        private Vector3 m_XrHeadNodePos;

        void Start()
        {
            m_StartingScale = XRSettings.eyeTextureResolutionScale;
            m_StartingZoomAmount = XRDevice.fovZoomFactor;
            m_StartingRenderScale = XRSettings.renderViewportScale;
            m_XrNodeStates = new List<XRNodeState>();
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            m_TestSetupHelpers.TestCubeSetup(TestCubesConfig.TestCube);
        }

        [TearDown]
        public override void TearDown()
        {
            m_RaycastHit = false;

            XRSettings.eyeTextureResolutionScale = 1f;
            XRDevice.fovZoomFactor = m_StartingZoomAmount;
            XRSettings.renderViewportScale = 1f;

            base.TearDown();
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator GazeCheck()
        {
            m_XrNodeStates = new List<XRNodeState>();
            yield return new WaitForSeconds(kDeviceSetupWait);

            RaycastHit info = new RaycastHit();

            yield return null;

            InputTracking.GetNodeStates(m_XrNodeStates);

            foreach (XRNodeState node in m_XrNodeStates)
            {
                if (node.nodeType == XRNode.Head)
                {
                    m_Head = node.nodeType;
                    node.TryGetPosition(out m_XrHeadNodePos);
                }
            }

            InputTracking.Recenter();

            yield return null;

            if (m_Cube != null)
            {
                m_Cube.transform.position = new Vector3(m_XrHeadNodePos.x, m_XrHeadNodePos.y, m_XrHeadNodePos.z + 2f);
            }
            else if (m_Cube == null)
            {
                m_TestSetupHelpers.TestCubeSetup(TestCubesConfig.TestCube);
                m_Cube.transform.position = new Vector3(m_XrHeadNodePos.x, m_XrHeadNodePos.y, m_XrHeadNodePos.z + 2f);
            }


            yield return new WaitForSeconds(2f);

            if (Physics.Raycast(m_XrHeadNodePos, m_Camera.GetComponent<Camera>().transform.forward, out info, 10f))
            {
                yield return new WaitForSeconds(0.05f);
                if (info.collider.name == m_Cube.name)
                {
                    m_RaycastHit = true;
                }
            }

            if (m_Cube != null)
            {
                GameObject.Destroy(m_Cube);
            }

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                Assert.IsTrue(m_RaycastHit, "Gaze check failed to hit something!");
            }
        }

#if UNITY_EDITOR
        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator CameraCheckForMultiPass()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            m_TestSetupHelpers.TestStageSetup(TestStageConfig.MultiPass);
            Assert.AreEqual(StereoRenderingPath.MultiPass, PlayerSettings.stereoRenderingPath,
                "Expected StereoRenderingPath to be Multi pass");
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator CameraCheckForInstancing()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            m_TestSetupHelpers.TestStageSetup(TestStageConfig.Instancing);
            Assert.AreEqual(StereoRenderingPath.Instancing, PlayerSettings.stereoRenderingPath,
                "Expected StereoRenderingPath to be Instancing");
        }
#endif

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator RenderViewportScale()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            XRSettings.renderViewportScale = 1f;
            Assert.AreEqual(1f, XRSettings.renderViewportScale, "Render viewport scale is not being respected");

            XRSettings.renderViewportScale = 0.7f;
            Assert.AreEqual(0.7f, XRSettings.renderViewportScale, "Render viewport scale is not being respected");

            XRSettings.renderViewportScale = 0.5f;
            Assert.AreEqual(0.5f, XRSettings.renderViewportScale, "Render viewport scale is not being respected");
        }


        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator EyeTextureResolutionScale()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            float scale = 0.1f;
            float scaleCount = 0.1f;

            for (float i = 0.1f; i < 2; i++)
            {
                scale = scale + 0.1f;
                scaleCount = scaleCount + 0.1f;

                XRSettings.eyeTextureResolutionScale = scale;

                yield return null;

                Debug.Log("EyeTextureResolutionScale = " + scale);
                Assert.AreEqual(scaleCount, XRSettings.eyeTextureResolutionScale,
                    "Eye texture resolution scale is not being respected");
            }
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator DeviceZoom()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            float zoomAmount = 0f;
            float zoomCount = 0f;

            for (int i = 0; i < 2; i++)
            {
                zoomAmount = zoomAmount + 1f;
                zoomCount = zoomCount + 1f;

                XRDevice.fovZoomFactor = zoomAmount;

                yield return null;

                Debug.Log("fovZoomFactor = " + zoomAmount);
                Assert.AreEqual(zoomCount, XRDevice.fovZoomFactor, "Zoom Factor is not being respected");
            }
        }

        [UnityTest]
        [RequireMagicLeapDevice]
        public IEnumerator TakeScreenShot()
        {
            yield return new WaitForSeconds(kDeviceSetupWait);

            try
            {
                m_FileName = Application.temporaryCachePath + "/ScreenShotTest.jpg";
                ScreenCapture.CaptureScreenshot(m_FileName, ScreenCapture.StereoScreenCaptureMode.BothEyes);

                m_DidSaveScreenCapture = true;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to get capture! : " + e);
                m_DidSaveScreenCapture = false;
                Assert.Fail("Failed to get capture! : " + e);
            }

            yield return new WaitForSeconds(5);

            var tex = new Texture2D(2, 2);

            var texData = File.ReadAllBytes(m_FileName);
            Debug.Log("Screen Shot Success!" + Environment.NewLine + "File Name = " + m_FileName);

            tex.LoadImage(texData);

            Assert.IsNotNull(tex, "Texture Data is empty");
        }
    }
}
