/* Класс персонажа, который котролирует собранную одежду и некоторые взаимодействия с внешней игрой*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private int weight = 0;

    [SerializeField] private int maxWeight = 10;

    [Header("Конвертирует 1 вес на указанный множетель")]
    [SerializeField] private int convectorMultiple = 10;

    [SerializeField] private MoveController moveClass;

    [HideInInspector] public enum DressType { shlapa, boti, botySpilka, losini, yubka, bottomDress, topDress, other };

    private List<DressID> _topDress = new List<DressID>();
    private List<DressID> _bottomDress = new List<DressID>();

    [SerializeField] private RectTransform weightBarVisual;
    private float startVisualHeight;

    private GameObject currentYubka;
    private GameObject currentBoti;
    private GameObject currenShlapa;
    private GameObject currentBottomDress;
    [SerializeField] private GameObject defaultBoti;
    [SerializeField] private GameObject defaultSlapa;

    [SerializeField] private Text maximumText;

    [SerializeField] private GameObject Arrow;
    private Transform _ExitZone;

    [SerializeField] private Animator weightAnim;
    
    [SerializeField] private ParticleSystem cointSpawnSystem;
    [Header("Партикл при подборе одежды")]
    [SerializeField] private ParticleSystem whenDressUseParticalSystem;

    [Header("HaveDress")]
    [SerializeField] private DressID[] haveDress = new DressID[1];

    private void Start()
    {
        _ExitZone = FindObjectOfType<ExitZone>().transform;

        maximumText.gameObject.SetActive(true);
        maximumText.text = "";
        Arrow.SetActive(false);
        if (moveClass == null) moveClass = GetComponent<MoveController>();
        startVisualHeight = weightBarVisual.rect.width;
        weightBarVisual.sizeDelta = new Vector2(0, weightBarVisual.rect.height);
        currentBoti = defaultBoti;
    }

    private void FixedUpdate()
    {
        if (Arrow.activeInHierarchy)
            Arrow.transform.LookAt(_ExitZone);
    }

    public void GameStarted()
    {
        weight = 0;
    }

    public void GameEnded()
    {

    }

    [Header("Для Разработчиков: вкл. ограничение подбора активных вещей")]
    [SerializeField] private bool cantUse = false;

    private void OnTriggerStay(Collider other)
    {
        if (!GameController.Instance.gameIsPlayed || weight >= maxWeight) return;

        if (other.GetComponent<WardrobeController>())
        {
            DressAsset dress;
            if (other.GetComponent<WardrobeController>().CanBeUse())
            {
                dress = other.GetComponent<WardrobeController>().DressWasUse(gameObject.transform);
                if (dress != null)
                {
                    //Дикий костыль, если надо что бы одетая одежда не одевалась повторно
                    if (cantUse)
                    {
                        for (int i = 0; i < haveDress.Length; i++)
                            if (haveDress[i].ID == dress.dressID)
                                if (haveDress[i]._dress != null)
                                {
                                    if (haveDress[i]._dress.gameObject.activeInHierarchy)
                                        return;
                                }
                                else
                                if (haveDress[i]._dressMeshRender.Length > 0)
                                    if (haveDress[i]._dressMeshRender[0].gameObject.activeInHierarchy)
                                        return;
                    } 
                        
                            weight += dress.weight;
                    GameController.Instance.UpdatePoint(dress.weight * convectorMultiple);

                    float percent = (weight * 100) / maxWeight;
                    weightBarVisual.sizeDelta = new Vector2(((startVisualHeight * percent) / 100), weightBarVisual.rect.height);
                    //Light, Medium, Heavy, Full

                    if (weight >= 0 && weight < (maxWeight * 40 / 100))
                        maximumText.text = "LIGHT";
                    else
                    {
                        if (weight >= (maxWeight * 40 / 100) && weight < (maxWeight * 70 / 100))
                            maximumText.text = "MEDIUM";
                        else
                        {
                            if (weight >= (maxWeight * 70 / 100) && weight < maxWeight)
                            {
                                moveClass.ActiveHardRun(1.1f);
                                maximumText.text = "HEAVY";
                            }
                            else
                                if (weight >= maxWeight)
                            {
                                moveClass.ActiveHardRun(1.5f);
                                maximumText.text = "FULL";
                                weightAnim.SetTrigger("Full");
                                Arrow.SetActive(true);
                            }
                        }
                    }

                    for (int i = 0; i < haveDress.Length; i++)
                        if (haveDress[i].ID == dress.dressID)
                        {
                            if (whenDressUseParticalSystem != null) whenDressUseParticalSystem.Play();

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
                                case DressType.other:
                                    //if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                    //    haveDress[i]._dress.SetBlendShapeWeight(0, weight * 15);
                                    break;
                                case DressType.topDress:
                                    _topDress.Add(haveDress[i]);
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, _topDress.Count * 25);
                                    break;
                                case DressType.bottomDress:
                                    _bottomDress.Add(haveDress[i]);
                                    if (haveDress[i]._dress.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                                        haveDress[i]._dress.SetBlendShapeWeight(0, _bottomDress.Count * 25);
                                    break;
                                case DressType.losini:
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
                                case DressType.yubka:
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
                                case DressType.boti:
                                    gameObject.GetComponent<CapsuleCollider>().height = 1.7f;

                                    if (currentBoti != null) 
                                    {
                                        if (currentBoti != haveDress[i]._dress.gameObject) currentBoti.SetActive(false);
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    break;
                                case DressType.botySpilka:
                                    gameObject.GetComponent<CapsuleCollider>().height = 2f;

                                    if (currentBoti != null)
                                    {
                                        if (currentBoti != haveDress[i]._dress.gameObject) currentBoti.SetActive(false);
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    }
                                    else
                                        currentBoti = haveDress[i]._dress.gameObject;
                                    break;
                                case DressType.shlapa:
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
        if (!GameController.Instance.gameIsPlayed) return;
            
        if (weight == 0) return;

        if (other.GetComponent<ExitZone>())
        {
            weightAnim.SetTrigger("Idle");
            gameObject.GetComponent<CapsuleCollider>().height = 1.70f;

            //GameController.Instance.UpdatePoint(weight * convectorMultiple);
            StartCoroutine(DressOut(other.GetComponent<ExitZone>()));
            moveClass.ActiveEaseRun();
            weight = 0;
            weightBarVisual.sizeDelta = new Vector2(0, weightBarVisual.rect.height);

            if (maximumText != null) maximumText.text = "";
            currenShlapa = null;
            currentYubka = null;
            defaultBoti.SetActive(true);
            currentBoti = defaultBoti;
            defaultSlapa.SetActive(true);
            Arrow.SetActive(false);
        }
    }

    private IEnumerator DressOut(ExitZone exitZone)
    {
        _topDress.Clear();
        _bottomDress.Clear();

        for (int i = 0; i < haveDress.Length; i++)
        {
            int j = haveDress.Length - 1 - i;
            if (haveDress[j]._dress != null)
            {
                if (haveDress[j]._dress.gameObject.activeInHierarchy)
                {
                    exitZone.BoxFlyToCar(gameObject.transform);
                    haveDress[j]._dress.gameObject.SetActive(false);
                }
            }
            else
                if (haveDress[j]._dressMeshRender.Length > 0)
                for (int x = 0; x < haveDress[j]._dressMeshRender.Length; x++)
                    if (haveDress[j]._dressMeshRender[x].gameObject.activeInHierarchy)
                    {
                        exitZone.BoxFlyToCar(gameObject.transform);
                        haveDress[j]._dressMeshRender[x].gameObject.SetActive(false);
                    }

            if (cointSpawnSystem != null) cointSpawnSystem.Play();
            yield return new WaitForSeconds(0.005f);
        }
    }
}

[System.Serializable]
public class DressID
{
    public SkinnedMeshRenderer _dress;
    public MeshRenderer[] _dressMeshRender;
    public int ID;
    public PlayerController.DressType dressType;
}