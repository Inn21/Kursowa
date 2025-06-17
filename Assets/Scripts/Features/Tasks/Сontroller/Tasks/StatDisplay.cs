using Features.Tasks.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class StatDisplay
{
    public GameObject Root;
    public Image Icon;
    public TextMeshProUGUI ValueText;

    public void SetData(Sprite iconSprite, int value)
    {
        Root.SetActive(true);
        Icon.sprite = iconSprite;
        ValueText.text = value > 0 ? $"+{value}" : value.ToString();
    }

    public void Hide()
    {
        Root.SetActive(false);
    }
}

[System.Serializable]
public class StatIconMapping
{
    public RewardType Type;
    public Sprite Icon;
}