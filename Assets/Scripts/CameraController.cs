/* Класс управления камерой, можно настроить позицию кода подлетает камера в конце уровня и то куда она будет смотреть */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform endTarget;
    [SerializeField] private Transform endPos;

    [SerializeField] private Transform startCameraPos;

    [SerializeField] private float moveSpeed;

    private bool endGame = false;

    private void Start()
    {
        //StartCoroutine(InvisibleControl());
        if (startCameraPos == null) startCameraPos = gameObject.transform;
    }

    public void GameEnd()
    {
        endGame = true;
        StartCoroutine(InvisibleControl());
    }

    private bool nextLevelAnim = false;
    public void NextLevelGOAnimation()
    {
        endGame = false;
        nextLevelAnim = true;
    }

    private void FixedUpdate()
    {
        if (endGame)
        {
            gameObject.transform.LookAt(endTarget);
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, endPos.position, moveSpeed * Time.deltaTime);
            playerVisual.transform.LookAt(new Vector3(endPos.position.x, playerVisual.position.y, endPos.position.z));
        }

        if (nextLevelAnim)
        {
            gameObject.transform.position = startCameraPos.position;
            gameObject.transform.rotation = startCameraPos.rotation;

            transform.SetParent(FindObjectOfType<GameController>().transform);
            // nextLevelAnim = false;
        }
    }

    [SerializeField] private GameObject invisibleObj;
    private IEnumerator InvisibleControl()
    {
        yield return new WaitForSeconds(0.2f);

        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, (gameObject.transform.position - playerVisual.position) * -100, out hit, 8))
        {
            if (hit.collider.GetComponent<MeshRenderer>())
            {
                GameObject newObj = hit.collider.gameObject;
                if (newObj != invisibleObj)
                {
                    StartCoroutine(VisibleDeactive(newObj.GetComponent<MeshRenderer>()));
                    if (invisibleObj != null) StartCoroutine(VisibleActivate(invisibleObj.GetComponent<MeshRenderer>()));
                    invisibleObj = newObj;
                }
            }
        }
        else
            if (invisibleObj != null)
        {
            StartCoroutine(VisibleActivate(invisibleObj.GetComponent<MeshRenderer>()));
            invisibleObj = null;
        }

        StartCoroutine(InvisibleControl());
    }

    private IEnumerator VisibleDeactive(MeshRenderer meshColor)
    {
        while (meshColor.material.color.a > 0.05f)
        {
            meshColor.material.color = new Color(meshColor.material.color.r, meshColor.material.color.g, meshColor.material.color.b, meshColor.material.color.a - 0.2f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator VisibleActivate(MeshRenderer meshColor)
    {
        while (meshColor.material.color.a < 1)
        {
            meshColor.material.color = new Color(meshColor.material.color.r, meshColor.material.color.g, meshColor.material.color.b, meshColor.material.color.a + 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
