using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpeningScene : MonoBehaviour
{
    //[0] == island 1 etc..
    [SerializeField] private List<Image> ImagesOfLocks;
    [SerializeField] private List<Image> ImagesOfDarken;
    [SerializeField] private List<Image> starsOfIsland1;
    [SerializeField] private List<Image> starsOfIsland2;
    [SerializeField] private List<Image> starsOfIsland3;
    [SerializeField] private List<Image> starsOfIsland4;
    [SerializeField] private List<Image> starsOfIsland5;
    [SerializeField] private GameObject firstButton;

    private void Awake()
    {
        SetPlayerPrefs();
        Time.timeScale = 1f;
    }

    void Start()
    {
        SetImagesOfIslands();
        SetStarsOfIslands();
        EventSystem.current.SetSelectedGameObject(firstButton);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("Island_1_Unlocked"))
        {
            PlayerPrefs.SetInt("Island_1_Unlocked", 0);
        }
        if (!PlayerPrefs.HasKey("Island_2_Unlocked"))
        {
            PlayerPrefs.SetInt("Island_2_Unlocked", 0);
        }
        if (!PlayerPrefs.HasKey("Island_3_Unlocked"))
        {
            PlayerPrefs.SetInt("Island_3_Unlocked", 0);
        }
        if (!PlayerPrefs.HasKey("Island_4_Unlocked"))
        {
            PlayerPrefs.SetInt("Island_4_Unlocked", 0);
        }
        if (!PlayerPrefs.HasKey("Island_5_Unlocked"))
        {
            PlayerPrefs.SetInt("Island_5_Unlocked", 0);
        }
        if (!PlayerPrefs.HasKey("Island_1_stars"))
        {
            PlayerPrefs.SetInt("Island_1_stars", 0);
        }
        if (!PlayerPrefs.HasKey("Island_2_stars"))
        {
            PlayerPrefs.SetInt("Island_2_stars", 0);
        }
        if (!PlayerPrefs.HasKey("Island_3_stars"))
        {
            PlayerPrefs.SetInt("Island_3_stars", 0);
        }
        if (!PlayerPrefs.HasKey("Island_4_stars"))
        {
            PlayerPrefs.SetInt("Island_4_stars", 0);
        } 
        if (!PlayerPrefs.HasKey("Island_5_stars"))
        {
            PlayerPrefs.SetInt("Island_5_stars", 0);
        }
    }

    private void SetImagesOfIslands()
    {
        if (PlayerPrefs.GetInt("Island_1_Unlocked") == 1)
        {
            ImagesOfDarken[0].gameObject.SetActive(false);
            ImagesOfLocks[0].gameObject.SetActive(false);
        }
        else
        {
            ImagesOfDarken[0].gameObject.SetActive(true);
            ImagesOfLocks[0].gameObject.SetActive(true);
        }
        if (PlayerPrefs.GetInt("Island_2_Unlocked") == 1)
        {
            ImagesOfDarken[1].gameObject.SetActive(false);
            ImagesOfLocks[1].gameObject.SetActive(false);
        }
        else
        {
            ImagesOfDarken[1].gameObject.SetActive(true);
            ImagesOfLocks[1].gameObject.SetActive(true);
        }
        if (PlayerPrefs.GetInt("Island_3_Unlocked") == 1)
        {
            ImagesOfDarken[2].gameObject.SetActive(false);
            ImagesOfLocks[2].gameObject.SetActive(false);
        }
        else
        {
            ImagesOfDarken[2].gameObject.SetActive(true);
            ImagesOfLocks[2].gameObject.SetActive(true);
        }
        if (PlayerPrefs.GetInt("Island_4_Unlocked") == 1)
        {
            ImagesOfDarken[3].gameObject.SetActive(false);
            ImagesOfLocks[3].gameObject.SetActive(false);
        }
        else
        {
            ImagesOfDarken[3].gameObject.SetActive(true);
            ImagesOfLocks[3].gameObject.SetActive(true);
        }
        if (PlayerPrefs.GetInt("Island_5_Unlocked") == 1)
        {
            ImagesOfDarken[4].gameObject.SetActive(false);
            ImagesOfLocks[4].gameObject.SetActive(false);
        }
        else
        {
            ImagesOfDarken[4].gameObject.SetActive(true);
            ImagesOfLocks[4].gameObject.SetActive(true);
        }
    }

    private void SetStarsOfIslands()
    {
        int stars;
        stars = PlayerPrefs.GetInt("Island_1_stars");
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                starsOfIsland1[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                starsOfIsland1[i].gameObject.SetActive(false);
            }
        }
        stars = PlayerPrefs.GetInt("Island_2_stars");
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                starsOfIsland2[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                starsOfIsland2[i].gameObject.SetActive(false);
            }
        }
        stars = PlayerPrefs.GetInt("Island_3_stars");
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                starsOfIsland3[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                starsOfIsland3[i].gameObject.SetActive(false);
            }
        }
        stars = PlayerPrefs.GetInt("Island_4_stars");
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                starsOfIsland4[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                starsOfIsland4[i].gameObject.SetActive(false);
            }
        }
        stars = PlayerPrefs.GetInt("Island_5_stars");
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                starsOfIsland5[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                starsOfIsland5[i].gameObject.SetActive(false);
            }
        }
    }
}
