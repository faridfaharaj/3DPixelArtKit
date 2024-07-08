using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCam : MonoBehaviour
{

    Camera m_Camera;

    //object snapping
    [HideInInspector] public List<ObjectSnap> DynamicObjects;

    //temporary, use renderFeature instead!!!!
    //                      v----lazy 
    [SerializeField] RectTransform screen;

    void LateUpdate()
    {
        Vector2 screenPos = getOffset(m_Camera) *  (Screen.height / (m_Camera.orthographicSize * 2f)) ;
        screen.localPosition = screenPos;
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        m_Camera = gameObject.GetComponent<Camera>();

        //object snapping
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        m_Camera.ResetWorldToCameraMatrix();

        //object snapping
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    Vector3 RoundToPixel(Vector3 position)
    {
        float unitsPerPixel = (1.0f/(m_Camera.scaledPixelHeight/ (m_Camera.orthographicSize*2)));
        if (unitsPerPixel == 0.0f)
            return position;

        Vector3 result;
        result.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
        result.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;
        result.z = Mathf.Round(position.z / unitsPerPixel) * unitsPerPixel;

        return result;
    }

    Vector3 getOffset(Camera Cam)
    {
        Vector3 cameraPosition = -(Cam.transform.InverseTransformPoint(Vector3.zero));
        Vector3 roundedCameraPosition = RoundToPixel(cameraPosition);
        return roundedCameraPosition - cameraPosition;
    }

    void PixelSnap()
    {
        Vector3 offset = getOffset(m_Camera);

        offset = Vector3.Scale(offset, Vector3.one);
        offset.z = -offset.z;
        Matrix4x4 offsetMatrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_Camera.worldToCameraMatrix = offsetMatrix * m_Camera.transform.worldToLocalMatrix;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == m_Camera)
        {
            PixelSnap();

            //object Snapping
            if (DynamicObjects != null)
            {
                for (int i = 0; i < DynamicObjects.Count; i++)
                {
                    DynamicObjects[i].SnapPos(m_Camera);
                }
            }
        }

    }

    //object Snapping
    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == m_Camera)
        {
            if (DynamicObjects != null)
            {
                for (int i = 0; i < DynamicObjects.Count; i++)
                {
                    DynamicObjects[i].undoSnap();
                }
            }
        }
    }
}
