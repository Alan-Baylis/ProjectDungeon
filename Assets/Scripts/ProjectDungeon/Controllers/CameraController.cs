using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

  private enum MouseButtons : int
  {
    Left = 0,
    Right = 1,
    Middle = 2,
  }

  public GameObject CameraTarget;
  int scrollRate = 16;
  float targetRotation = 45.0f;
  // Update is called once per frame
  void Update()
  {
    HandleKeyboardPan(Time.deltaTime);
    HandleKeyboardRotate(Time.deltaTime);
  }

  //void HandleMouseZoom()
  //{
  //  Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * 2;
  //  Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 2f, 10f);
  //}

  void HandleKeyboardPan(float deltaTime)
  {
    var one = 1 * deltaTime * scrollRate;
    var moveVector = new Vector3(0, 0, 0);
    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
    {
      moveVector.x += one;
      moveVector.z += -one;
    }
    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
    {
      moveVector.x += -one;
      moveVector.z += one;
    }
    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
    {
      moveVector.x += one;
      moveVector.z += one;
    }
    if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
    {
      moveVector.x += -one;
      moveVector.z += -one;
    }
    CameraTarget.transform.Translate(moveVector);
  }

  void HandleKeyboardRotate(float deltaTime)
  {
    if (Input.GetKeyDown(KeyCode.Q))
      targetRotation += 90;

    if (Input.GetKeyDown(KeyCode.E))
      targetRotation -= 90;

    Quaternion target = Quaternion.Euler(0, targetRotation, 0);
    CameraTarget.transform.localRotation = Quaternion.Slerp(CameraTarget.transform.localRotation, target, 0.03f);
  }
}
