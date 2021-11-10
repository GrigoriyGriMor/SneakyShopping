/* Класс содержащий набор материалов */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAsset : MonoBehaviour
{
    private static MaterialAsset instance;
    public static MaterialAsset Instance => instance;

    [SerializeField] private Material[] dressMaterials = new Material[7];

    private void Awake()
    {
        instance = this;
    }

    public Material GetRandomMaterials()
    {
        return dressMaterials[Random.Range(0, dressMaterials.Length - 1)];
    }



}
