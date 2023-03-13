using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void LateUpdate() // late update in order to follow the movement from the player.
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 1);
    }
}
