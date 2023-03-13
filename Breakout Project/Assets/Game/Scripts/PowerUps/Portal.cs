using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private const float MIN_X = -2f;
    private const float MAX_X = 12f;
    private const float MIN_X_PRINCESS = 1f;
    private const float MAX_X_PRINCESS = 8f;
    private const float MIN_TOP_Y = 0;
    private const float MAX_TOP_Y = 1;
    private const float MIN_BOT_Y = -6;
    private const float MAX_BOT_Y = -2;
    private const string PORTAL_ONE_NAME = "Portal One";
    private const string PORTAL_TWO_NAME = "Portal Two";
    private const int PRINCESS_STAGE = 3;
    [SerializeField] private float portalDistanceToBall;
    [SerializeField] private GameObject portalOne;
    [SerializeField] private GameObject portalTwo;

    private void OnEnable()
    {
        if (GameManager.Shared.GetStage() == PRINCESS_STAGE)
        {
            portalOne.transform.localPosition = new Vector3(Random.Range(MIN_X_PRINCESS, MAX_X_PRINCESS),
                Random.Range(MIN_BOT_Y, MAX_BOT_Y), 0);
            portalTwo.transform.localPosition = new Vector3(Random.Range(MIN_X_PRINCESS, MAX_X_PRINCESS),
                Random.Range(MIN_TOP_Y, MAX_TOP_Y), 0);
        }
        else
        {
            portalOne.transform.localPosition =
                new Vector3(Random.Range(MIN_X, MAX_X), Random.Range(MIN_BOT_Y, MAX_BOT_Y), 0);
            portalTwo.transform.localPosition =
                new Vector3(Random.Range(MIN_X, MAX_X), Random.Range(MIN_TOP_Y, MAX_TOP_Y), 0);
        }

        while (Vector3.Distance(GameManager.Shared.GetBall().transform.position, portalOne.transform.position) <
               portalDistanceToBall)
        {
            portalOne.transform.localPosition =
                new Vector3(Random.Range(MIN_X, MAX_X), Random.Range(MIN_BOT_Y, MAX_BOT_Y), 0);
            return;
        }

        while (Vector3.Distance(GameManager.Shared.GetBall().transform.position, portalTwo.transform.position) <
               portalDistanceToBall)
        {
            portalTwo.transform.localPosition =
                new Vector3(Random.Range(MIN_X, MAX_X), Random.Range(MIN_TOP_Y, MAX_TOP_Y), 0);
            return;
        }

        StartCoroutine(ActiveTime(PowerUpManager.Shared.portalActiveTime));
    }

    private IEnumerator ActiveTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    public void TeleportToOther(string thisPortalName)
    {
        GameObject otherPortal;
        switch (thisPortalName)
        {
            case PORTAL_ONE_NAME:
                otherPortal = portalTwo;
                break;
            case PORTAL_TWO_NAME:
                otherPortal = portalOne;
                break;
            default:
                return;
        }

        gameObject.SetActive(false);
        GameManager.Shared.GetBall().transform.position = otherPortal.transform.position;
    }
}