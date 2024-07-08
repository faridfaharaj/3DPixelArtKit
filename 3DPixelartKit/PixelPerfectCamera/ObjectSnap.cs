using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectSnap : MonoBehaviour
{
    Vector3 realPos;

    void OnEnable()
    {
        if (GetComponent<Renderer>().isVisible)
        {
            Camera.main.gameObject.GetComponent<PixelPerfectCam>().DynamicObjects.Add(this);
        }
    }

    void OnDisable()
    {
        Camera.main.gameObject.GetComponent<PixelPerfectCam>().DynamicObjects.Remove(this);
    }

    void OnBecameVisible()
    {
        if (!Camera.main.gameObject.GetComponent<PixelPerfectCam>().DynamicObjects.Contains(this))
        {
            Camera.main.gameObject.GetComponent<PixelPerfectCam>().DynamicObjects.Add(this);
        }
    }

    void OnBecameInvisible()
    {
        Camera.main.gameObject.GetComponent<PixelPerfectCam>().DynamicObjects.Remove(this);
    }

    Vector3 RoundToPixel(Vector3 position)
    {
        float unitsPerPixel = (1.0f / (Camera.main.scaledPixelHeight / (Camera.main.orthographicSize * 2)));
        if (unitsPerPixel == 0.0f)
            return position;

        Vector3 result;
        result.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
        result.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;
        result.z = Mathf.Round(position.z / unitsPerPixel) * unitsPerPixel;

        return result;
    }

    public void SnapPos(Camera Cam)
    {
        if (transform.position != realPos)
        {
            realPos = transform.position;

            Matrix4x4 CameraMatrix = Cam.cameraToWorldMatrix;
            Vector3 CameraPos = CameraMatrix.GetPosition();
            Vector3 diference = (transform.position - CameraPos);

            Vector3 Position = Quaternion.Inverse(Cam.transform.rotation) * new Vector3(diference.x, diference.y, diference.z);
            Vector3 roundedPosition = RoundToPixel(Position);
            Vector3 result = Cam.transform.rotation * Vector3.Scale(roundedPosition, Vector3.one) + CameraPos;

            transform.position = result;
            Debug.Log(CameraMatrix.GetPosition());
        }
    }

    public void undoSnap()
    {
        transform.position = realPos;
    }
}
