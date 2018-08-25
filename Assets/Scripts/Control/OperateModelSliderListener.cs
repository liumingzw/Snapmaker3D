using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OperateModelSliderListener : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
	public void OnPointerUp (PointerEventData eventData)
	{
		ModelManager.GetInstance ().OnOperateEnd();
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		ModelManager.GetInstance ().OnOperateStart();
	}
}
