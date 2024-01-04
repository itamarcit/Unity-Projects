using ItamarRamon;
using UnityEngine;
using WalkingSimulator;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject flockSpawner;
    [SerializeField] private GameObject pickupSpawner;
    [SerializeField] private MovementController playerMovementController;
    [SerializeField] private WiggleTail playerTail;

    public void PauseAll()
    {
        flockSpawner.SetActive(false);
        pickupSpawner.SetActive(false);
        playerMovementController.enabled = false;
        playerTail.PauseTailAttack(true);
    }

    public void UnpauseAll()
    {
        flockSpawner.SetActive(true);
        pickupSpawner.SetActive(true);
        playerMovementController.enabled = true;
        playerTail.PauseTailAttack(false);
    }
}
