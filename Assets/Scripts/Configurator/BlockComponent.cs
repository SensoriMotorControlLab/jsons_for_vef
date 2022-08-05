using UnityEngine;
using UnityEngine.EventSystems;

public class BlockComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /*
     * This script is attached to each individual block/notch pair in BlockView.
     * 
     * This handles the detection of beginning a drag, dragging, and ending a drag.
     * 
     */


    public ConfigurationBlockManager BlockController;
    public int BlockID;
    public GameObject Notch;
    public GameObject Block;

    public void OnBeginDrag(PointerEventData eventData)
    {
        BlockController.Dragged = true;
        BlockController.OnBlockBeginDrag(gameObject);
        //parent = transform.parent;
        //transform.SetParent(transform.parent.parent.parent.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = eventData.position;
        BlockController.OnBlockDrag(gameObject, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //transform.SetParent(parent);
        BlockController.Dragged = false;
        BlockController.OnEndDrag(gameObject);
    }
}