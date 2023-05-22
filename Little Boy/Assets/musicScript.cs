using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class musicScript : MonoBehaviour
{
    [SerializeField] private AudioClip openScreenMusic;
    [SerializeField] private AudioClip GameMusic;
    private AudioSource _audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "OpenScreen" && _audioSource.clip != openScreenMusic)
        {
            _audioSource.clip = openScreenMusic;
            _audioSource.Play();
        }
        if ( SceneManager.GetActiveScene().name != "OpenScreen" &&_audioSource.clip != GameMusic )
        {
            _audioSource.clip =GameMusic;
            _audioSource.Play();

        }
    }
}
