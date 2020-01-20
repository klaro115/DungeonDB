using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UiMouseDownHandler : MonoBehaviour, IPointerDownHandler
{
	public UnityEvent pointerDownEvent = new UnityEvent();

	public void OnPointerDown(PointerEventData eventData)
	{
		if (pointerDownEvent != null) pointerDownEvent.Invoke();
	}
}
