using UnityEngine;
using UnityEngine.UI;

namespace Flocking
{
    public class HealthBar : MonoBehaviour
    {
        public Slider slider;

        private void Update()
        {
            slider.value = GameManager.Shared.GetPlayerHealth();
        }
    }
}