using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NoticeUI : UI_Base
{
    [SerializeField] private List<TextMeshProUGUI> _texts;
    [SerializeField] private List<Button> _buttons;

    public void SetString(List<string> str)
    {
        _texts.ForEach(x => x.text = string.Empty);
        for (int i = 0; i < str.Count; i++)
        {
            if (_texts.Count <= i)
                break;

            _texts[i].text = str[i];
        }
    }

    public void SetCallback(List<System.Action> callback)
    {
        _buttons.ForEach(x => x.onClick.RemoveAllListeners());
        foreach (var item in _buttons)
        {
            item.gameObject.SetActive(false);
        }

        if (callback == null || callback.Count == 0)
            return;

        int count = Mathf.Min(callback.Count, _buttons.Count);
        for (int i = 0; i < count; i++)
        {
            int index = i;
            _buttons[index].gameObject.SetActive(true);
            _buttons[index].onClick.AddListener(() => 
            {
                callback[index]?.Invoke();
            });
        }
    }

    public void OnClickClose()
    {
        GameManager.UI.CloseUI(this);
    }
}
