using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager
{
    private Stack<UI_Base> _uiStack = new Stack<UI_Base>();
    private int _sortOrder = 1000;

    public UI_Base ShowUI(UI_Base popupPrefab, Transform parent = null)
    {
        if(popupPrefab.Type == EUIType.Popup)
        {
            GameObject popup = GameObject.Instantiate(popupPrefab.gameObject, parent != null ? parent : GameManager.Instance.GetPopupParent().transform);
            UI_Base ui = popup.GetComponent<UI_Base>();
            if (ui == null)
                return null;

            var canvas = popup.GetComponent<Canvas>();
            if (canvas != null)
            {
                _sortOrder++;
                canvas.sortingOrder = _sortOrder;
            }
            SetUI(ui);
            return ui;
        }
        else
        {
            popupPrefab.gameObject.SetActive(true);
            SetUI(popupPrefab);
            return popupPrefab;
        } 
    }

    public UI_Base SetUI(UI_Base popupUI)
    {
        _uiStack.Push(popupUI);
        popupUI.gameObject.SetActive(true);
        return popupUI;
    }

    public void CloseTopUI()
    {
        if (_uiStack.Count > 0)
        {
            UI_Base topPopup = _uiStack.Pop();

            if(topPopup.Type == EUIType.Popup)
                GameObject.Destroy(topPopup.gameObject);
            else
                topPopup.gameObject.SetActive(false);            
        }
    }

    public void CloseUI(UI_Base popup)
    {
        if (_uiStack.Contains(popup))
        {
            Stack<UI_Base> tempStack = new Stack<UI_Base>();
            while (_uiStack.Count > 0)
            {
                UI_Base currentPopup = _uiStack.Pop();
                if (currentPopup == popup)
                {
                    if(currentPopup.Type == EUIType.Popup)
                        GameObject.Destroy(currentPopup.gameObject);
                    else
                        currentPopup.gameObject.SetActive(false);

                    break;
                }
                else
                {
                    tempStack.Push(currentPopup);
                }
            }
            while (tempStack.Count > 0)
            {
                _uiStack.Push(tempStack.Pop());
            }
        }
    }

    public void CloseAllUIs()
    {
        while (_uiStack.Count > 0)
        {
            UI_Base popup = _uiStack.Pop();
            if (popup == null || popup.gameObject == null)
                continue;

            if (popup.Type == EUIType.Popup)
                GameObject.Destroy(popup.gameObject);
            else
                popup.gameObject.SetActive(false);
        }
    }

    public bool HasUI()
    {
        return _uiStack.Count > 0;
    }

    public bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();

#elif UNITY_IOS || UNITY_ANDROID
    if (Input.touchCount > 0)
        return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    else
        return false;

#else
    return false;
#endif
    }
}
