using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    [SerializeField] private GameObject box;
    [SerializeField] private List<GameObject> boxPool = new List<GameObject>();

    [SerializeField] private GameObject[] boxInCar = new GameObject[15];
    [SerializeField] private Transform car;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject _go = Instantiate(box, Vector3.zero, Quaternion.identity);
            boxPool.Add(_go);
            _go.SetActive(false);
        }
    }

    public void BoxFlyToCar(Transform pos)
    {
        for (int i = 0; i < boxPool.Count; i++)
        {
            if (!boxPool[i].activeInHierarchy)
            {
                boxPool[i].SetActive(true);
                boxPool[i].transform.position = pos.position;
                boxPool[i].GetComponentInChildren<Animator>().SetTrigger("FlyToCar");
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < boxPool.Count; i++)
            if (boxPool[i].activeInHierarchy)
            {
                if (Vector3.Distance(boxPool[i].transform.position, car.position) < 2)
                {
                    boxPool[i].SetActive(false);
                    for (int j = 0; j < boxInCar.Length; j++)
                        if (!boxInCar[j].activeInHierarchy)
                        {
                            boxInCar[j].SetActive(true);
                            break;
                        }
                }
                else
                    boxPool[i].transform.position = Vector3.Lerp(boxPool[i].transform.position, car.position, 2f * Time.deltaTime);
            }
    }
}
