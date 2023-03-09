using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slideshow : MonoBehaviour
{
    // Slideshow has an array of GameObjects.
    // We have an index that corresponds to the current GameObject.
    // All other GameObjects are disabled.
    // Navigation can be automatic or manual.

    public GameObject[] slides;
    public float slideDuration = 5.0f;
    public bool automatic = true;
    public bool loop = true;

    public int currentSlide = 0; // Public so we can pick starting slide in inspector

    private float timer = 0.0f;

    public void NextSlide()
    {
        slides[currentSlide].SetActive(false);
        currentSlide = (currentSlide + 1) % slides.Length;
        slides[currentSlide].SetActive(true);
    }

    public void PreviousSlide()
    {
        slides[currentSlide].SetActive(false);
        currentSlide = (currentSlide - 1 + slides.Length) % slides.Length;
        slides[currentSlide].SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Disable all slides except the first one
        for (int i = 0; i < slides.Length; i++)
        {
            if (i != currentSlide)
            {
                slides[i].SetActive(false);
            }
        }

        // Set the first slide to active
        slides[currentSlide].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (automatic)
        {
            timer += Time.deltaTime;
            if (timer >= slideDuration)
            {
                NextSlide();
                timer = 0.0f;
            }
        }
    }
}
