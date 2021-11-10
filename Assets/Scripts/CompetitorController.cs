/* Класс управления ботами, можно настроить скорость, собираемый вес и минимальную дистанцию */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CompetitorController : MonoBehaviour
{
    private NavMeshAgent AI;

    [Header("AI Setting")]
    [SerializeField] private float speed = 5;
    [SerializeField] private int maxWeight = 5;
    [SerializeField] private Transform exitZone;
    [SerializeField] private float minDistance = 2;

    [SerializeField] private Transform target;
    private float weight = 0;

    [SerializeField] private Animator anim;

    [SerializeField] private GameObject defaultBoti;
    [SerializeField] private GameObject defaultSlapa;

    [Header("HaveDress")]
    [SerializeField] private DressID[] haveDress = new DressID[1];

    private GameObject currentYubka;
    private GameObject currentBoti;
    private GameObject currenShlapa;
    private GameObject currentBottomDress;

    private List<DressID> _topDress = new List<DressID>();
    private List<DressID> _bottomDress = new List<DressID>();

    private void Start()
    {
        AI = GetComponent<NavMeshAgent>();
        weight = 0;
        AI.speed = 0;

        currentBoti = defaultBoti;
    }

    public void GameStart()
    {
        weight = 0;

        target = WardrobeCollection.Instance.ChekNewTarget();
        AI.destination = target.position;
        AI.speed = speed;
        anim.SetBool("Run", true);
    }

    public void EndGame(bool win)
    {
        AI.speed = speed;

        if (win)
            anim.SetBool("LoseGame", true);
        else
            anim.SetBool("WinGame", true);

        anim.SetBool("Run", false);
    }

    private float time = 0;
    private void FixedUpdate()
    {
        if (!GameController.Instance.gameIsPlayed)
        {
            AI.speed = 0;
            return;
        }

        if (target != null && Vector3.Distance(gameObject.transform.position, target.transform.position) < minDistance)
        {
            target = WardrobeCollection.Instance.ChekNewTarget();
            AI.destination = target.position;
            time = 0;
        }
        else
        {
            if (target != null && time >= 5f)
            {
                target = WardrobeCollection.Instance.ChekNewTarget();
                AI.destination = target.position;
                time = 0;
            }
        }

        time += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!GameController.Instance.gameIsPlayed) return;

        if (other.GetComponent<WardrobeController>())
        {
            DressAsset dress;
            if (other.GetComponent<WardrobeController>().CanBeUse())
            {
                dress = other.GetComponent<WardrobeController>().DressWasUse(gameObject.transform);
                if (dress != null)
                {
                    weight += dress.weight;

                    if (weight >= maxWeight)
                        AI.destination = exitZone.position;

                    for (int i = 0; i < haveDress.Length; i++)
                        if (haveDress[i].ID == dress.dressID)
                        {
                           // haveDress[i]._dress.gameObject.SetActive(true);
                            //haveDress[i]._dress.material = dress.dress.material;

                            if (haveDress[i]._dress != null)
                            {
                                haveDress[i]._dress.gameObject.SetActive(true);
                                haveDress[i]._dress.material = dress.dress.material;
                            }
                            else
                            {
                                if (haveDress[i]._dressMeshRender.Length > 1)
                                {
                                    for (int j = 0; j < haveDress[i]._dressMeshRender.Length; j++)
                                        if (!haveDress[i]._dressMeshRender[j].gameObject.activeInHierarchy)
                                        {
                                            haveDress[i]._dressMeshRender[j].gameObject.SetActive(true);
                                            haveDress[i]._dressMeshRender[j].material = dress.dress.material;
                                            break;
                                        }
                                }
                                else
                                {
                                    haveDress[i]._dressMeshRender[0].gameObject.SetActive(true);
                                    haveDress[i]._dressMeshRender[0].material = dress.dress.material;
                                }
                            }


                            switch (haveDress[i].dressType)
                            {
                                case PlayerController.DressType.other:
                                    //if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                    //    haveDress[i]._dress.SetBlendShapeWeight(0, weight * 15);
                                    break;
                                case PlayerController.DressType.topDress:
                                    _topDress.Add(haveDress[i]);
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, _topDress.Count * 25);
                                    break;
                                case PlayerController.DressType.bottomDress:
                                    _bottomDress.Add(haveDress[i]);
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, _bottomDress.Count * 25);
                                    break;
                                case PlayerController.DressType.losini:
                                    //if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                    //    haveDress[i]._dress.SetBlendShapeWeight(0, weight * 20);

                                    if (currentBottomDress != null && currentBottomDress != haveDress[i]._dress.gameObject)
                                    {
                                        currentBottomDress.SetActive(false);
                                        currentBottomDress = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentBottomDress = haveDress[i]._dress.gameObject;
                                    break;
                                case PlayerController.DressType.yubka:
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, weight * 15);

                                    if (currentYubka != null && currentYubka != haveDress[i]._dress.gameObject)
                                    {
                                        currentYubka.SetActive(false);
                                        currentYubka = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentYubka = haveDress[i]._dress.gameObject;
                                    break;
                                case PlayerController.DressType.boti:
                                    gameObject.GetComponent<CapsuleCollider>().height = 1.7f;

                                    if (currentBoti != null)
                                    {
                                        currentBoti.SetActive(false);
                                        if (currentBoti != haveDress[i]._dress.gameObject) currentBoti = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    break;
                                case PlayerController.DressType.botySpilka:
                                    gameObject.GetComponent<CapsuleCollider>().height = 2f;

                                    if (currentBoti != null)
                                    {
                                        if (currentBoti != haveDress[i]._dress.gameObject) currentBoti.SetActive(false);
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    break;
                                case PlayerController.DressType.shlapa:
                                    defaultSlapa.SetActive(false);

                                    if (currenShlapa != null && currenShlapa != haveDress[i]._dressMeshRender[0].gameObject)
                                    {
                                        currenShlapa.SetActive(false);
                                        currenShlapa = haveDress[i]._dressMeshRender[0].gameObject;
                                    }
                                    else
                                        currenShlapa = haveDress[i]._dressMeshRender[0].gameObject;
                                    break;
                                default:
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, weight * 15);
                                    break;
                            }
                        }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (weight == 0) return;

        if (other.GetComponent<ExitZone>())
        {
            weight = 0;
            gameObject.GetComponent<CapsuleCollider>().height = 1.70f;
            target = WardrobeCollection.Instance.ChekNewTarget();
            AI.destination = target.position;
            for (int i = 0; i < haveDress.Length; i++)
            {
                if (haveDress[i]._dress != null)
                        haveDress[i]._dress.gameObject.SetActive(false);
                else
                    if (haveDress[i]._dressMeshRender.Length > 0)
                    for (int x = 0; x < haveDress[i]._dressMeshRender.Length; x++)
                            haveDress[i]._dressMeshRender[x].gameObject.SetActive(false);
            }

            _topDress.Clear();
            _bottomDress.Clear();
            currenShlapa = null;
            currentYubka = null;
            defaultBoti.SetActive(true);
            currentBoti = defaultBoti;
            defaultSlapa.SetActive(true);
        }
    }
}
