using UnityEngine;

namespace Flocking
{
    public class ConstantRotation : MonoBehaviour
    {
        // This script constantly rotates the game object it is on. Used for pickups in game.
        [SerializeField] private float rotationSpeed = 100f;
        private int rotationSide;
        private void OnEnable()
        {
            rotationSide = Random.Range(0, 2);
            if (rotationSide == 0) rotationSide = -1;
        }

        private void Update()
        {
            transform.Rotate(0, 0, rotationSide * Time.deltaTime * rotationSpeed, Space.Self);
        }
    }
}
