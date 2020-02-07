﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer masterMixer;

    public Slider musicSlider;
    public string musicParamater;

    public Slider UISlider;
    public string UIParamater;

    public Slider DifficultySlider;

    public Slider CameraSlider;

    public touchInputController touchInput;

    void OnEnable()
    {
        float value;
        if (masterMixer.GetFloat(musicParamater, out value)) {
			musicSlider.value = Mathf.Pow(10, value/20);
		}

        if (masterMixer.GetFloat(UIParamater, out value)) {
			UISlider.value = Mathf.Pow(10, value/20);
		}

        if (PlayerPrefs.HasKey("Difficulty")) {
            DifficultySlider.value = PlayerPrefs.GetInt("Difficulty");
		}

        if (PlayerPrefs.HasKey("CameraSensitivity")) {
            CameraSlider.value = PlayerPrefs.GetInt("CameraSensitivity");
		}
    }

    void Awake() {
        if (PlayerPrefs.HasKey("Music")) {
            masterMixer.SetFloat(musicParamater, Mathf.Log10(PlayerPrefs.GetFloat("Music")) * 20);
        }

        if (PlayerPrefs.HasKey("UI")) {
            masterMixer.SetFloat(UIParamater, Mathf.Log10(PlayerPrefs.GetFloat("UI")) * 20);
        }

        if (!PlayerPrefs.HasKey("Difficulty")) {
            PlayerPrefs.SetInt("Difficulty", 1);
        }

        if (!PlayerPrefs.HasKey("CameraSensitivity")) {
            PlayerPrefs.SetInt("CameraSensitivity", 5);
        }

        if (!PlayerPrefs.HasKey("CameraSensitivity")) {
            PlayerPrefs.SetInt("CameraSensitivity", 3);
        }
        
        if (touchInput != null) {
            touchInput.setCameraSensitivity(PlayerPrefs.GetInt("CameraSensitivity"));
        }
    }

    void OnDisable() {
        print("saved settings");
        PlayerPrefs.Save();
    }

    public void SetMusicLevel(float level)
	{
		masterMixer.SetFloat(musicParamater, Mathf.Log10(level) * 20);
        PlayerPrefs.SetFloat("Music", level);
	}

    public void SetUILevel(float level)
	{
		masterMixer.SetFloat(UIParamater, Mathf.Log10(level) * 20);
        PlayerPrefs.SetFloat("UI", level);
	}

    public void SetDifficultyLevel(float level)
	{
        PlayerPrefs.SetInt("Difficulty", (int)level);
	}

    public void SetCameraSensitivityLevel(float level)
    {
        PlayerPrefs.SetInt("CameraSensitivity", (int)level);
        if (touchInput != null) {
            touchInput.setCameraSensitivity(PlayerPrefs.GetInt("CameraSensitivity"));
        }
    }
}
