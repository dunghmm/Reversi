using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [SerializeField]
    Image MenuBackground;

    [SerializeField]
    GameObject UserSetImageButton;

    [SerializeField]
    GameObject OptionGameplay;

    [SerializeField]
    GameObject OptionBackground;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Button ButtonGameplay;

    [SerializeField]
    Button ButtonBackground;

    [SerializeField]
    Button ButtonMusicOn;

    [SerializeField]
    Button ButtonMusicOff;

    [SerializeField]
    Button ButtonAnimationSlow;

    [SerializeField]
    Button ButtonAnimationMedium;

    [SerializeField]
    Button ButtonAnimationFast;

    [SerializeField]
    Button ButtonSoundOn;

    [SerializeField]
    Button ButtonSoundOff;

    [SerializeField]
    Button ButtonNightOn;

    [SerializeField]
    Button ButtonNightOff;

    [SerializeField]
    AudioSource placeDiscSound;

    Sprite userSprite;

    public void OnUpdateBackground(Sprite sprite)
    {
        MenuBackground.sprite = sprite;
        PlayerPrefs.SetInt("IsUsingUserImage", 0);
        PlayerPrefs.SetString("planePicture", MenuBackground.sprite.name);
    }

    private void OnEnable()
    {
        if (ButtonAnimationSlow.gameObject.activeSelf)
        {
            if (PlayerPrefs.GetFloat("AnimationSpeed") != 0)
            {
                switch (PlayerPrefs.GetFloat("AnimationSpeed"))
                {
                    case 0.8f:
                        ButtonAnimationSlow.interactable = false;
                        ButtonAnimationMedium.interactable = true;
                        ButtonAnimationFast.interactable = true;
                        break;
                    case 1:
                        ButtonAnimationSlow.interactable = true;
                        ButtonAnimationMedium.interactable = false;
                        ButtonAnimationFast.interactable = true;
                        break;
                    case 1.5f:
                        ButtonAnimationSlow.interactable = true;
                        ButtonAnimationMedium.interactable = true;
                        ButtonAnimationFast.interactable = false;
                        break;
                }
            }

            if (PlayerPrefs.GetInt("Night") == 1)
            {
                ButtonNightOn.interactable = false;
                ButtonNightOff.interactable = true;
            }

            if (PlayerPrefs.GetInt("IsUsingUserImage") != 1)
            {
                if (PlayerPrefs.GetString("userDirectory").Length > 0)
                {
                    StartCoroutine(LoadUserImage(PlayerPrefs.GetString("userDirectory")));
                }
                string planePictureName = PlayerPrefs.GetString("planePicture");
                if (planePictureName.Length > 0)
                {
                    OnUpdateBackground(Resources.Load<Sprite>("PlaneTemplates/" + planePictureName));
                }
            }
            else
            {
                StartCoroutine(LoadUserImageOnStart());
            }
        }

        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            ButtonSoundOn.interactable = false;
            ButtonSoundOff.interactable = true;
        }

        if (PlayerPrefs.GetInt("Music") == 1)
        {
            ButtonMusicOn.interactable = false;
            ButtonMusicOff.interactable = true;
            audioSource.Play();
        }
    }

    private IEnumerator LoadUserImageOnStart()
    {
        yield return LoadUserImage(PlayerPrefs.GetString("userDirectory"));
        OnUserSetImageClicked();
    }

    public void OnUserImagePickClicked()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("", ".jpg", ".png"));
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Load", "Load");

        if (FileBrowser.Success)
        {
            PlayerPrefs.SetString("userDirectory", FileBrowser.Result[0]);
            yield return LoadUserImage(FileBrowser.Result[0]);
        }
    }

    private IEnumerator LoadUserImage(string directory)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(directory);
        yield return www.SendWebRequest();
        if (!(www.result == UnityWebRequest.Result.ConnectionError) && !(www.result == UnityWebRequest.Result.ProtocolError))
        {
            var texture = DownloadHandlerTexture.GetContent(www);
            userSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            UserSetImageButton.GetComponent<Image>().sprite = userSprite;
        }
    }

    public void OnUserSetImageClicked()
    {
        if (PlayerPrefs.GetString("userDirectory").Length > 0)
        {
            PlayerPrefs.SetInt("IsUsingUserImage", 1);
            MenuBackground.sprite = userSprite;
        }
    }

    public void OnGameplayClicked()
    {
        
        OptionGameplay.SetActive(true);
        OptionBackground.SetActive(false);
        ButtonGameplay.interactable = false;
        ButtonBackground.interactable = true;
    }

    public void OnBackgroundClicked()
    {
        OptionGameplay.SetActive(false);
        OptionBackground.SetActive(true);
        ButtonBackground.interactable = false;
        ButtonGameplay.interactable = true;
    }

    public void OnMusicOnClicked()
    {
        PlayerPrefs.SetInt("Music", 1);
        audioSource.Play();
        ButtonMusicOn.interactable = false;
        ButtonMusicOff.interactable = true;
    }

    public void OnMusicOffClicked()
    {
        PlayerPrefs.SetInt("Music", 0);
        audioSource.Stop();
        ButtonMusicOn.interactable = true;
        ButtonMusicOff.interactable = false;
    }

    public void OnAnimationSlowClicked()
    {
        PlayerPrefs.SetFloat("AnimationSpeed", 0.8f);
        ButtonAnimationSlow.interactable = false;
        ButtonAnimationMedium.interactable = true;
        ButtonAnimationFast.interactable = true;
    }
    public void OnAnimationMediumClicked()
    {
        PlayerPrefs.SetFloat("AnimationSpeed", 1);
        ButtonAnimationSlow.interactable = true;
        ButtonAnimationMedium.interactable = false;
        ButtonAnimationFast.interactable = true;
    }
    public void OnAnimationFastClicked()
    {
        PlayerPrefs.SetFloat("AnimationSpeed", 1.5f);
        ButtonAnimationSlow.interactable = true;
        ButtonAnimationMedium.interactable = true;
        ButtonAnimationFast.interactable = false;
    }

    public void OnSoundOnClicked()
    {
        PlayerPrefs.SetInt("Sound", 1);
        ButtonSoundOn.interactable = false;
        ButtonSoundOff.interactable = true;
        placeDiscSound.Play();
    }
    public void OnSoundOffClicked()
    {
        PlayerPrefs.SetInt("Sound", 0);
        ButtonSoundOn.interactable = true;
        ButtonSoundOff.interactable = false;
    }

    public void OnNightOnClicked()
    {
        PlayerPrefs.SetInt("Night", 1);
        ButtonNightOn.interactable = false;
        ButtonNightOff.interactable = true;
    }
    public void OnNightOffClicked()
    {
        PlayerPrefs.SetInt("Night", 0);
        ButtonNightOn.interactable = true;
        ButtonNightOff.interactable = false;
    }
}
