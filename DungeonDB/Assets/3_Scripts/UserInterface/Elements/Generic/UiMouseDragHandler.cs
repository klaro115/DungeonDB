using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UiMouseDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	#region Fields

	private bool isDragging = false;

	private Vector2 currentMousePos = Vector2.zero;
	private Vector2 beginDragMousePos = Vector2.zero;

	public UnityEvent beginDragEvent = new UnityEvent();
	public UnityEvent endDragEvent = new UnityEvent();
	public UnityEvent isDraggingEvent = new UnityEvent();

	public float dragEventRepetitionRate = 0.05f;

	private Coroutine dragRoutine = null;

	#endregion
	#region Properties

	public bool IsDragging => isDragging;

	public Vector2 CurrentMousePosition => currentMousePos;
	public Vector2 CurrentDragOffset => isDragging ? currentMousePos - beginDragMousePos : Vector2.zero;

	#endregion
	#region Methods

	public void OnBeginDrag(PointerEventData eventData)
	{
		currentMousePos = eventData.position;
		beginDragMousePos = currentMousePos;

		if (beginDragEvent != null) beginDragEvent.Invoke();

		isDragging = true;

		if (dragRoutine == null) dragRoutine = StartCoroutine("DragUpdateRoutine", dragEventRepetitionRate);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		currentMousePos = eventData.position;

		if (endDragEvent != null) endDragEvent.Invoke();

		if (dragRoutine != null) StopCoroutine(dragRoutine);
		dragRoutine = null;

		isDragging = false;
	}

	private IEnumerator DragUpdateRoutine(float updateInterval)
	{
		while (isDragging)
		{
			yield return new WaitForSecondsRealtime(updateInterval);

			currentMousePos = Input.mousePosition;
			if (isDraggingEvent != null) isDraggingEvent.Invoke();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		//...
	}

	#endregion
}
