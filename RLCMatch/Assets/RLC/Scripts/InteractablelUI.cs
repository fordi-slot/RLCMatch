using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractablelUI : MonoBehaviour
{
    public static bool PointerOnUI { get; protected set; }
    private RectTransform m_rectTransform;

    private void Update()
    {
        if (m_rectTransform == null)
            m_rectTransform = (RectTransform)transform;

        PointerOnUI = RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition);

        //PointerOnUI = m_rectTransform.rect.Contains(Input.mousePosition);
    }
}
