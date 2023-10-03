using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuManager : MonoBehaviour
{
    public RawImage image;
    public Image fade;
    public Texture[] textures;
    public float[] timings;
    public float fadeTime;
    public float sigmoidScale;
    public float blackScreen;
    public String nextScene;
    private float currentTimeOnScreen;
    private int textureI = 0;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        
        if (textures.Length == 0) {
            return;
        }

        image.texture = textures[textureI];

        if (textureI < textures.Length && currentTimeOnScreen >= timings[textureI]) {
            textureI += 1;
            currentTimeOnScreen = 0;
        }

        if (textureI == textures.Length) {
            SceneManager.LoadScene(nextScene);
            return;
        }

        if (currentTimeOnScreen < blackScreen) {
            fade.color = new Color(0, 0, 0, 1);
        } else if (currentTimeOnScreen >= blackScreen 
            && currentTimeOnScreen < blackScreen + fadeTime) {
            fade.color = new Color(0, 0, 0, 1 - (currentTimeOnScreen - blackScreen) / fadeTime);
        } else if (currentTimeOnScreen > timings[textureI] - fadeTime - blackScreen 
            && currentTimeOnScreen <= timings[textureI] - blackScreen) {
            fade.color = new Color(0, 0, 0, 1 - (timings[textureI] - blackScreen - currentTimeOnScreen) / fadeTime);
        } else if (currentTimeOnScreen > timings[textureI] - blackScreen) {
            fade.color = new Color(0, 0, 0, 1);
        } else {
            fade.color = new Color(0, 0, 0, 0);
        }
        
        currentTimeOnScreen += Time.deltaTime;
    }


    public float Sigmoid(float value) 
    {
        float k = Mathf.Exp(value);
        return k / (1.0f + k);
    }
}
