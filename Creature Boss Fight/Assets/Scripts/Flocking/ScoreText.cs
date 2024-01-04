using TMPro;
using UnityEngine;

namespace Flocking
{
    public class ScoreText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmp;
        private void Awake()
        {
            //tmp = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (GameManager.Shared.IsGameOver()) return;
            tmp.text = "Score: " + GameManager.Shared.GetPlayerScore() + "\n" + "Health: " + GameManager.Shared.GetPlayerHealth();
        }
    }
}
