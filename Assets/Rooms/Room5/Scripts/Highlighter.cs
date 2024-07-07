using Common;
using UnityEngine;

namespace Room5
{
    public class Highlighter : MonoBehaviour
    {
        [SerializeField]
        private Updowner _arrow;

        [SerializeField]
        private Vector3 _localDefaultArrowPoint;
        
        [SerializeField]
        private Material _highlightOverride;

        private MaterialHighlighter _object;

        private MeshRenderer _arrowRenderer;
        
        public void Highlight(GameObject obj)
        {
            if (_object != null)
            {
                _object.ExternalOverride = null;
                _object.DeEmphasize();
            }

            _object = null;
            if (obj != null && obj.TryGetComponent(out _object))
            {
                transform.position = obj.transform.position;
                _object.ExternalOverride = _highlightOverride;
                _object.Emphasize();
            }

            if (obj != null && obj.TryGetComponent(out Collider collider))
            {
                _arrowRenderer ??= _arrow.GetComponent<MeshRenderer>();
                
                var bounds = collider.bounds;
                var arrowBounds = _arrowRenderer.bounds;
                var verticalOffset = bounds.extents.y + arrowBounds.extents.y - arrowBounds.center.y + _arrow.transform.position.y + .2f;
                var upPoint = bounds.center + verticalOffset * Vector3.up;
                _arrow.Start = upPoint;
            }
            else _arrow.Start = transform.TransformPoint(_localDefaultArrowPoint);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Highlight(null);
            gameObject.SetActive(false);
        }
    
    }
}
