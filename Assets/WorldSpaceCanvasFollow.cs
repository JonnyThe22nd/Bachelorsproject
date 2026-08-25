using UnityEngine;


/// Hält eine World-Space-Canvas an einem Zielobjekt und dreht sie
/// so, dass sie immer zur Kamera zeigt. 

public class WorldSpaceCanvasFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0.3f, 0);
    public bool faceCamera = true;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        if (faceCamera && Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
