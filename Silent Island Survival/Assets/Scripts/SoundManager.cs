using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager SoundManagerInstance;
    public AudioSource SFXSource;

    private void Awake()
    {
        if (SoundManagerInstance != null && SoundManagerInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        SoundManagerInstance = this;
        DontDestroyOnLoad(this);
    }
}
