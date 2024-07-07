using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour {

    public float pressDuration = 0.1f;
    public UnityEvent onPress = new UnityEvent();

    private Vector3 startPos;
    private Vector3 startScale;
    private float elapsed;
    private bool pressing;

    // Start is called before the first frame update
    void Start() {
        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update() {
        if (pressing) {
            float t = Mathf.Clamp01(elapsed / pressDuration);
            float dy = Mathf.Sin(t * Mathf.PI) * (startScale.y / 2);
            Vector3 pos = startPos;
            Vector3 scale = startScale;
            pos.y -= dy;
            scale.y = startScale.y - dy;
            transform.localPosition = pos;
            transform.localScale = scale;
            elapsed = elapsed += Time.deltaTime;
            pressing = (t < 1);
        }
        else {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    if (hit.transform == transform) {
                        pressing = true;
                        elapsed = 0;
                        onPress.Invoke();
                    }
                }
            }
        }
    }
}