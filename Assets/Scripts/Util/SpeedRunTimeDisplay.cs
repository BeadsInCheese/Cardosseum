using UnityEngine;

public class SpeedRunTimeDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        int hours = (int)(speedrunTimer.timer / 3600);
        int minutes = (int)(speedrunTimer.timer / 60) % 60;
        int seconds = (int)(speedrunTimer.timer % 60);
        int milliseconds = (int)((speedrunTimer.timer % 1) * 1000);
        text.text = $"Completion time: {hours:D2} : {minutes:D2} : {seconds:D2} : {milliseconds:D3}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
