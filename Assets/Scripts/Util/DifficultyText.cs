using UnityEngine;

public class DifficultyText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TMPro.TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "modifier HP "+ 10 * Deck.difficulty + " % ";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
