using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySlider : MonoBehaviour
{
    public Slider slider;
    public TMPro.TextMeshProUGUI Info;
    private void Start()
    {
        Info.text = "Enemy HP + " + 10 * Deck.difficulty + " % ";
        slider.value=Deck.difficulty ;
    }
    public void SetDifficulty()
    {
        Deck.difficulty = (int)slider.value;
        Info.text = "Enemy HP + " + 10 * Deck.difficulty + " % ";
    }
}
