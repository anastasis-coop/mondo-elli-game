using UnityEngine;

[ExecuteInEditMode]
public class Updowner : MonoBehaviour
{
    [SerializeField, Min(0)]
    private float _amplitude;

    [SerializeField, Min(0)]
    private float _period;

    private Vector3? _start;

    public Vector3 Start
    {
        get => transform.parent.TransformPoint(_start ?? transform.localPosition);
        set => _start = transform.parent.InverseTransformPoint(value);
    }

    private void Awake()
    {
        _start ??= transform.localPosition;
    }
    
    private void Update()
    {
        if (_period == 0 || _amplitude == 0) return;
        transform.localPosition = _start.Value + Mathf.Sin(2 * Mathf.PI / _period * Time.time) * _amplitude * Vector3.up;
    }
}
