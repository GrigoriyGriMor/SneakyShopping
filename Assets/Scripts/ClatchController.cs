using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClatchController : MonoBehaviour
{
    [Header("Привязывать к hand")]
    [SerializeField] private Transform pos;

    private void Update()
    {
        gameObject.transform.position = new Vector3(pos.position.x, pos.position.y + 0.35f, pos.position.z);
    }
}
