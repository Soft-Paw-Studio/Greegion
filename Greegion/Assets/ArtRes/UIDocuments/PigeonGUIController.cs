using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PigeonGUIController : MonoBehaviour
{
    private int friesCount;
    private int remainFriesCount;

    private UIDocument visualAsset;

    public static UnityAction EatFriesEvent;

    public Label friesText;
    
    void Start()
    {
        visualAsset = GetComponent<UIDocument>();
        friesText = visualAsset.rootVisualElement.Q<Label>("FriesCount");
        
        friesCount = FindObjectsByType<Food>(FindObjectsSortMode.None).Length;
        remainFriesCount = friesCount;
        friesText.text = remainFriesCount.ToString();
        
        EatFriesEvent += OnEatFriesEvent;
    }

    private void OnEatFriesEvent()
    {
        remainFriesCount -= 1;
        friesText.text = remainFriesCount.ToString();
    }
}
