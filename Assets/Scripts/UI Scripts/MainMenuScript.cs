using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using NUnit.Framework;

public class MainMenuScript : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource source;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider soundSlider;

    public GameObject optionsMenu;
    public GameObject controlsMenu;
    public GameObject graphicsMenu;
    public GameObject audioMenu;

    Resolution[] resolutions;
    public TMP_Dropdown resolutionDropdown;
    int currentResolutionIndex = 0;
    bool isResButtonPressed = false;

    int minRes;
    int maxRes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("Master Volume", 1);
        musicSlider.value = PlayerPrefs.GetFloat("Music Volume", 1);
        soundSlider.value = PlayerPrefs.GetFloat("Sound Volume", 1);

        
        controlsMenu.SetActive(false);
        optionsMenu.SetActive(false);
        graphicsMenu.SetActive(false);
        audioMenu.SetActive(false);

        Resolution();

        optionsMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        SetMasterVolume();
        SetMusicVolume();
        SetSoundVolume();
    }

    #region Audio Sliders

    public void SetMasterVolume()
    {
        // Get Value From Slide
        float volume = masterSlider.value;
        if (volume <= 0)
        {
            volume = 0.001f;
        }

        //Tell the Mixer to set the Master Volume Parameter to that + Maths
        mixer.SetFloat("Master Volume", Mathf.Log10(volume) * 20);

        //Use PlayerPrefs To Save That Value To The Prefrences
        PlayerPrefs.SetFloat("Master Volume", volume);
    }

    public void SetMusicVolume()
    {
        // Get Value From Slide
        float volume = musicSlider.value;
        if (volume <= 0)
        {
            volume = 0.001f;
        }

        //Tell the Mixer to set the Master Volume Parameter to that + Maths
        mixer.SetFloat("Music Volume", Mathf.Log10(volume) * 20);

        //Use PlayerPrefs To Save That Value To The Prefrences
        PlayerPrefs.SetFloat("Music Volume", volume);
    }

    public void SetSoundVolume()
    {
        // Get Value From Slide
        float volume = soundSlider.value;
        if (volume <= 0)
        {
            volume = 0.001f;
        }

        //Tell the Mixer to set the Master Volume Parameter to that + Maths
        mixer.SetFloat("SFX Volume", Mathf.Log10(volume) * 20);

        //Use PlayerPrefs To Save That Value To The Prefrences
        PlayerPrefs.SetFloat("Sound Volume", volume);
    }

    #endregion

    #region Options Menu
    public void OpenOptionsMenu()
    {
        optionsMenu.SetActive(true);
        audioMenu.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        optionsMenu.SetActive(false);
        audioMenu.SetActive(false);
        controlsMenu.SetActive(false);
        graphicsMenu.SetActive(false);
    }

    public void AudioMenu()
    {
        audioMenu.SetActive(true);
        graphicsMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    public void GraphicsMenu()
    {
        graphicsMenu.SetActive(true);
        audioMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    public void ControlsMenu()
    {
        controlsMenu.SetActive(true);
        audioMenu.SetActive(false);
        graphicsMenu.SetActive(false);
    }
    #endregion

    #region Graphics Menu

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void Resolution()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();


        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && !isResButtonPressed)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        maxRes = resolutions.Length - 1;
        minRes = 0;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void NextResolution()
    {
        if(currentResolutionIndex < maxRes)
        {
            currentResolutionIndex += 1;
            isResButtonPressed = true;
            Resolution();
        }   
    }

    public void PreviousResolution() 
    {
        if(currentResolutionIndex > minRes) 
        {
            currentResolutionIndex -= 1;
            isResButtonPressed = true;
            Resolution();
        }
    }


    public void SetVSync(bool isVsync)
    {
        if(isVsync)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }
    }

    

    #endregion

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    
}
