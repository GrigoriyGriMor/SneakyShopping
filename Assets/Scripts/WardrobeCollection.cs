/*  ласс контролирующий количество шкафов и то к каким шкафам боты смогут добегать */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeCollection : MonoBehaviour
{
    private static WardrobeCollection instance;
    public static WardrobeCollection Instance => instance;

    [SerializeField] private WardrobeController[] wardrobes = new WardrobeController[5];
    [SerializeField] private PlayerController player;

    [Header("”казать радиус вокруг игрока, где будут бегать боты")]
    [SerializeField] private float visibleDistance = 15;

    public List<WardrobeController> activeWardrobes = new List<WardrobeController>();

    private void Awake()
    {
        instance = this;
    }

    public void GameStarted()
    {
        StartCoroutine(CountControl());
    }

    private IEnumerator CountControl()
    {
        if (GameController.Instance.gameIsPlayed)
        {
            yield return new WaitForSeconds(0.05f);

            for (int i = 0; i < wardrobes.Length; i++)
            {
                if (Vector3.Distance(player.transform.position, wardrobes[i].gameObject.transform.position) <= visibleDistance)
                {
                    if (!activeWardrobes.Contains(wardrobes[i]))
                        activeWardrobes.Add(wardrobes[i]);
                }
                else
                    activeWardrobes.Remove(wardrobes[i]);
            }

            StartCoroutine(CountControl());
        }
        else
        {
            yield return new WaitForEndOfFrame();
            activeWardrobes.RemoveRange(0, activeWardrobes.Count - 1);
        }
    }

    public Transform ChekNewTarget()
    {
        if (activeWardrobes.Count == 0)
            return wardrobes[Random.Range(0, wardrobes.Length - 1)].gameObject.transform;

        Transform newTarget = activeWardrobes[Random.Range(0, activeWardrobes.Count - 1)].gameObject.transform;
        return newTarget;
    }

}
