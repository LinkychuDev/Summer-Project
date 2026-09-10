using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoorCanvasScript : MonoBehaviour
{
    public GameObject displayBackground;
    public TextMeshProUGUI levelName, collectedOrbsText;
    public Image[] berryImages;
    public Color berryNotCollectedColor;
    public Color berryCollectedColor;
    public Color completedColor;
    public Image disabledImage;
    public Image orbImage;
    public Color orbCollectedColor;
    private Color levelNameColor => levelName.color;
    private Color _uc;
    private Color orbImageColor => orbImage.color;
    public Color unlockedColor = new Color(1, 1, 1);
    public Color notUnlockedColor;
    
    public float dotweenShakeScale = 0.5f;
    public float dotweenTime = 0.2f;
    void Start()
    {
        Clear();
    }
    public void Display(LevelInfoSO levelInfo, bool isUnlockable)
    {
        if (isUnlockable)
        {
           // _uc = unlockedColor;
        }

        else
        {
           // _uc = notUnlockedColor;
        }
            
        levelName.text = levelInfo.levelName;
        //levelName.color = levelNameColor;
        if (GameManager.instance.CollectedOrbs.TryGetValue(levelInfo.SceneName, out var collectedOrbs))
        {
            collectedOrbsText.text = collectedOrbs.ToString();
        }

        else
        {
            collectedOrbsText.text = 0.ToString();
           // orbImage.color = orbImageColor * _uc;
        }


        if (GameManager.instance.GiantBerriesDict.TryGetValue(levelInfo.SceneName, out var list))
        {
            for (int i = 0; i < berryImages.Length; i++)
            {
                berryImages[i].color = list[i].isCollected ? berryCollectedColor : berryNotCollectedColor;
            }
        }

        else
        {
            for (int i = 0; i < berryImages.Length; i++)
            {
                berryImages[i].color = berryNotCollectedColor;
            }
        }
        if (levelInfo.isCompleted)
        {
            levelName.color = completedColor;
        }


        disabledImage.gameObject.SetActive(!isUnlockable);
        displayBackground.SetActive(true);
        
        displayBackground.transform.DOPunchScale(Vector3.one * dotweenShakeScale, dotweenTime);
    }   

    public void Clear()
    {
        levelName.text = "";
        collectedOrbsText.text = "";
        displayBackground.SetActive(false);
        disabledImage.gameObject.SetActive(false);
        
    }
}
