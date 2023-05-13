using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSetter : MonoBehaviour
{
    [SerializeField] private TMP_Text width;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private TMP_Text height;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Text mines;
    [SerializeField] private Slider minesSlider;

    
    void Update()
    {
        width.text = widthSlider.value.ToString();
        height.text = heightSlider.value.ToString();
        minesSlider.maxValue = Mathf.Floor(widthSlider.value * heightSlider.value / 5.7f); 
        mines.text = Mathf.Floor(minesSlider.value).ToString();
    }

    public void StartCustomLevel() 
    {
        DataHolder.width = (int)widthSlider.value;
        DataHolder.height = (int)heightSlider.value;
        DataHolder.mines = (int)minesSlider.value;
    }
}
