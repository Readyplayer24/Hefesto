using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;

public class Menu : MonoBehaviour
{
    [Header("Options")]
    public Slider volumeFX;
    public Slider volumeMaster;
    public Toggle mute;
    public AudioMixer mixer;
    public AudioSource audioSource;
    public AudioClip clickSound;
    private float lastVolume;

    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject optionsMenu;

    private void Awake()
    {
        volumeFX.onValueChanged.AddListener(ChangeVolumeFX);
        volumeMaster.onValueChanged.AddListener(ChangeVolumeMaster);
    }

    public void PlayLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void StartGame()
    {
        string firstLevelName = "Nivel_01";
        PlayLevel(firstLevelName);
    }
    public void SetMute()
    {
        if (mute.isOn)
        {
            mixer.GetFloat("VolMaster", out lastVolume);
            mixer.SetFloat("VolMaster", -80f);
        }
        else
        {
            mixer.SetFloat("VolMaster", lastVolume);
        }
    }

    public void OpenPanel(GameObject panel)
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        panel.SetActive(true);
        playSoundButton();
    }

    public void ChangeVolumeMaster(float v)
    {
        mixer.SetFloat("VolMaster", v);
    }

    public void ChangeVolumeFX(float v)
    {
        mixer.SetFloat("VolFX", v);
    }

    public void playSoundButton()
    {
        audioSource.PlayOneShot(clickSound);
    }
}


