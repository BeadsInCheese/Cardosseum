using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;
    [SerializeField] private TMPro.TextMeshProUGUI masterSliderText;
    [SerializeField] private TMPro.TextMeshProUGUI musicSliderText;
    [SerializeField] private TMPro.TextMeshProUGUI sfxSliderText;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Toggle fullScreenToggle;

    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            AudioManager.Instance.PlayOneShot(AudioManager.AudioEffects.flipCard);
        }
    }

    void Awake()
    {
        // DontDestroyOnLoad(this.gameObject);
        SetDefaultFullScreenOption();    
        SetDefaultVolumeValue();
    }

    void Start()
    {
    }

    public void HideOptionsPanel()
    {
        this.gameObject.SetActive(false);
    }

    public void OnMasterSliderValueChange()
    {
        masterSliderText.text = Mathf.RoundToInt(masterSlider.value).ToString();
        var mixerVolValue = masterSlider.value - 80;
        mainMixer.SetFloat("MasterVol", mixerVolValue);
        PlayerPrefs.SetFloat("MasterVol", mixerVolValue);
    }

    public void OnMusicSliderValueChange()
    {
        musicSliderText.text = Mathf.RoundToInt(musicSlider.value).ToString();
        var mixerVolValue = musicSlider.value - 80;
        mainMixer.SetFloat("MusicVol", mixerVolValue);
        PlayerPrefs.SetFloat("MusicVol", mixerVolValue);
    }

    public void OnSFXSliderValueChange()
    {
        sfxSliderText.text = Mathf.RoundToInt(sfxSlider.value).ToString();
        var mixerVolValue = sfxSlider.value - 80;
        mainMixer.SetFloat("SFXVol", mixerVolValue);
        PlayerPrefs.SetFloat("SFXVol", mixerVolValue);
    }

    public void OnFullScreenToggleValueChange()
    {
        # if UNITY_EDITOR
            var windows = (UnityEditor.EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(UnityEditor.EditorWindow));
            foreach(var window in windows)
            {
                if(window != null && window.GetType().FullName == "UnityEditor.GameView")
                {
                    window.maximized = fullScreenToggle.isOn;
                    break;
                }
            }
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        #else
            Screen.fullScreen = fullScreenToggle.isOn;
        # endif
        PlayerPrefs.SetInt("FullScreen", (fullScreenToggle.isOn)? 1: 0);
    }

    private void SetDefaultVolumeValue()
    {
        if(PlayerPrefs.HasKey("MasterVol"))
        {
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol") + 80;
        }
        if(PlayerPrefs.HasKey("MusicVol"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol") + 80;
        }
        if(PlayerPrefs.HasKey("SFXVol"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol") + 80;
        }
    }

    public void SetDefaultFullScreenOption()
    {
        if(PlayerPrefs.HasKey("FullScreen"))
        {
            bool isOn = (PlayerPrefs.GetInt("FullScreen") == 1)? true: false;
            fullScreenToggle.isOn = isOn;
        }
        OnFullScreenToggleValueChange();       
    }
}
