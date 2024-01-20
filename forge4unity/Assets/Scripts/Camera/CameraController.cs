using UnityEngine;


public class CameraController : MonoBehaviour
{
    [Header("NAVIGATION")]
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float rotationSpeed = 10.0f;
    [SerializeField]
    private float zoomSpeed = 10.0f;

    [Header("MOVEMENT")]
    [SerializeField]
    private KeyCode forwardKey = KeyCode.W;
    [SerializeField]
    private KeyCode backKey = KeyCode.S;
    [SerializeField]
    private KeyCode leftKey = KeyCode.A;
    [SerializeField]
    private KeyCode rightKey = KeyCode.D;

    [Header("HORIZONTAL-ONLY MOVEMENT")]
    [SerializeField, Tooltip("For movement only in horizontal axis")]
    private KeyCode flatMoveKey = KeyCode.LeftShift;

    [Header("AXES")]
    [SerializeField, Tooltip("Vertical")]
    private string mouseY = "Mouse Y";
    [SerializeField, Tooltip("Horizontal")]
    private string mouseX = "Mouse X";
    [SerializeField, Tooltip("Zoom")]
    private string zoomAxis = "Mouse ScrollWheel";

    [Header("ANCHORED NAVIGATION")]
    [SerializeField, Tooltip("Movement")]
    private KeyCode anchoredMoveKey = KeyCode.Mouse2;
    [SerializeField, Tooltip("Rotation")]
    private KeyCode anchoredRotateKey = KeyCode.Mouse1;

    [Header("TEXTURES")]
    [SerializeField]
    private Texture2D anchoredRotationTexture;
    [SerializeField]
    private Texture2D anchoredMovementTexture;

    private void LateUpdate()
    {
        UpdateMouseIcon();

        Vector3 move = Vector3.zero;

        //Move and rotate the camera

        if (Input.GetKey(forwardKey))
            move += Vector3.forward * moveSpeed;
        if (Input.GetKey(backKey))
            move += Vector3.back * moveSpeed;
        if (Input.GetKey(leftKey))
            move += Vector3.left * moveSpeed;
        if (Input.GetKey(rightKey))
            move += Vector3.right * moveSpeed;

        //By far the simplest solution I could come up with for moving only on the Horizontal plane - no rotation, just cache y
        if (Input.GetKey(flatMoveKey))
        {
            float origY = transform.position.y;

            transform.Translate(move);
            transform.position = new Vector3(transform.position.x, origY, transform.position.z);

            return;
        }

        float mouseMoveY = Input.GetAxis(mouseY);
        float mouseMoveX = Input.GetAxis(mouseX);

        //Move the camera when anchored
        if (Input.GetKey(anchoredMoveKey))
        {
            move += mouseMoveY * -moveSpeed * Vector3.up;
            move += mouseMoveX * -moveSpeed * Vector3.right;
        }

        //Rotate the camera when anchored
        if (Input.GetKey(anchoredRotateKey))
        {
            transform.RotateAround(transform.position, transform.right, mouseMoveY * -rotationSpeed);
            transform.RotateAround(transform.position, Vector3.up, mouseMoveX * rotationSpeed);
        }

        transform.Translate(move);

        //Scroll to zoom
        float mouseScroll = Input.GetAxis(zoomAxis);
        transform.Translate(mouseScroll * zoomSpeed * Vector3.forward);
    }
    public void UpdateMouseIcon()
    {
        Texture2D icon = null;
        if (Input.GetKey(anchoredMoveKey))
            icon = anchoredMovementTexture;
        if (Input.GetKey(anchoredRotateKey))
            icon = anchoredRotationTexture;

        Cursor.SetCursor(icon, Vector2.zero, CursorMode.Auto);
    }

}