using UnityEngine;
using UnityEngine.EventSystems;

public class UnitClickInput : MonoBehaviour
{
    [SerializeField] private LayerMask _unitLayer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.UI.IsPointerOverUI())
                return;

            Vector2 pos = TileMapReader.Instance.GetCamera().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, Mathf.Infinity, _unitLayer);

            if (hit.collider != null)
            {
                Tower unit = hit.collider.GetComponent<Tower>();
                if (unit != null)
                {
                    UnitPlacementManager.Instance.OnUnitClicked(unit);
                    CameraZoomController.Instance.FocusOnPosition(unit.transform.position);
                }
            }
        }
    }
}