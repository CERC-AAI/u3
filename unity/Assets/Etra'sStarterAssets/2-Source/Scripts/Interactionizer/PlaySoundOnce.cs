using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnce : MonoBehaviour
{
    public AudioClip sound;

    public void PlaySound()
    {
        AudioSource.PlayClipAtPoint(sound, transform.position);
    }
}
