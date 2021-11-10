/* =======================================WardrobeController===================================
 * Класс контролирует одежду на вешалке, сколько ее может быть, какая может быть использованна
 * и через сколько она может появиться на вешалке снова, после того как ее схватили с нее
 DressAsset - это набор данных по каждой отдельной одежде на вешалке, хранит в себе сам объект, 
 его позицию, может ли он быть использован и вес
 * ============================================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeController : MonoBehaviour
{
   [SerializeField] private DressAsset[] dress = new DressAsset[1];

   [SerializeField] private float minLocalDist = 2;

   [Header("SpawnSetting")]
   [SerializeField] private float spawnMultiple = 2;

   private int deactiveDressCount = 0;

   private void Start()
   {
       deactiveDressCount = 0;

       for (int i = 0; i < dress.Length; i++)
       {
           if (dress[i].dress != null)
           dress[i].canBeUse = true;
       }
   }

    public bool CanBeUse()
    {
        if (deactiveDressCount >= dress.Length)
            return false;
        else
        return true;
    }

    public DressAsset DressWasUse(Transform _user)
    {
        int minDistanceObjID = -1;
        float minDist = 150;

        for (int i = 0; i < dress.Length; i++)
        {
            if (dress[i].canBeUse)
            {
                float newDist = Vector3.Distance(_user.position, dress[i].dressTransform.position);
                if (newDist <= minLocalDist && newDist < minDist)
                {
                    minDist = newDist;
                    minDistanceObjID = i;
                }
            }
        }

        if (minDistanceObjID == -1) return null;


        deactiveDressCount += 1;
        dress[minDistanceObjID].canBeUse = false;
        StartCoroutine(ResetDress(dress[minDistanceObjID]));
        return dress[minDistanceObjID];
    }

   private IEnumerator ResetDress(DressAsset obj)
   {
       obj.dress.gameObject.SetActive(false);
       yield return new WaitForSeconds(obj.weight * spawnMultiple);

       obj.dress.gameObject.SetActive(true);
       if (MaterialAsset.Instance) obj.dress.material = MaterialAsset.Instance.GetRandomMaterials();
       deactiveDressCount -= 1;
       obj.canBeUse = true;
   }
}

[System.Serializable]
public class DressAsset
{
    public MeshRenderer dress;

    public Transform dressTransform;
    [HideInInspector] public bool canBeUse;

    public int weight;
    public int dressID;

    public DressAsset(MeshRenderer _go, int _weight, Transform _transform, bool type, int id)
    {
        dress = _go;
        weight = _weight;
        dressTransform = _transform;
        canBeUse = type;

        dressID = id;
    }
}
