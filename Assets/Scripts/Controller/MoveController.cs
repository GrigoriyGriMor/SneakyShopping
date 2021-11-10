/* =======================================MoveController===================================
 * Класс контролирует движение персонажа
 * ========================================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    [Header("Rotate")]
    [SerializeField] private GameObject visualPlayer;
   //[SerializeField] private float rotateSpeedMultiple = 5;

    [Header("MoveForward")]
    [SerializeField] private float speed = 5f;
    private float _speed = 5f;

    [SerializeField] private Animator[] anim;
    [SerializeField] private Animator animCar;

    private void Awake()
    {
        _speed = speed;
    }

    void FixedUpdate()
    {
        if (nextLevelAnimPlay)
        {
            anim[0].transform.SetParent(animCar.gameObject.transform);
            anim[0].transform.position = nextLevelCar.transform.position;
            anim[0].transform.localRotation = nextLevelCar.transform.localRotation;

            if (anim[0].GetBool("SitInCar"))
                return;


            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("SitInCar", true);
            }

            animCar.SetTrigger("GONextLevel");
            StartCoroutine(WaitEndGameAnim());

            nextLevelAnimPlay = false;
        }

        if (!GameController.Instance.gameIsPlayed) return;

        Move();
    }

    private bool nextLevelAnimPlay = false;
    [SerializeField] private Transform nextLevelCar;
    public void NextLevelAnim()
    {
        nextLevelAnimPlay = true;
    }

    private IEnumerator WaitEndGameAnim()
    {
        yield return new WaitForSeconds(3);

        GameController.Instance.ReadyToGONextLevel();
    }

    private void Move()
    {
        float horizMove = JoystickStick.Instance.HorizontalAxis();
        float verticalMove = JoystickStick.Instance.VerticalAxis();

        if (horizMove == 0.0f && verticalMove == 0.0f)
        {
            if (anim.Length != 0)
            {
                for (int i = 0; i < anim.Length; i++)
                {
                    anim[i].SetBool("Run", false);
                    anim[i].SetBool("HardRun", false);
                }
            }

            return;
        }

     //   Vector2 moveVector = new Vector2(horizMove, verticalMove);
    //    Vector2 currentVector = new Vector2(transform.forward.x, transform.forward.z);

        //float angle = Mathf.Clamp(Vector2.SignedAngle(moveVector, currentVector), -1, 1);

        float angle = Mathf.Atan2(JoystickStick.Instance.HorizontalAxis(), JoystickStick.Instance.VerticalAxis()) * Mathf.Rad2Deg;
        visualPlayer.transform.rotation = Quaternion.Euler(0, angle, 0);

        //*= Quaternion.Euler(0, angle, 0);

        transform.transform.position = new Vector3(transform.transform.position.x + verticalMove * _speed, transform.transform.position.y, transform.transform.position.z + (-horizMove * _speed));//(new Vector3(verticalMove * speed, 0, -horizMove * speed), Space.World);

        if (anim.Length != 0)
        {
            for (int i = 0; i < anim.Length; i++)
                if (_speed == speed)
                    anim[i].SetBool("Run", true);
                else
                    anim[i].SetBool("HardRun", true);
        }
    }

    public void ActiveHardRun(float multiply)
    {
        for (int i = 0; i < anim.Length; i++)
            anim[i].SetBool("HardRun", true);

        _speed = speed / multiply; 
    }

    public void ActiveEaseRun()
    {
        for (int i = 0; i < anim.Length; i++)
            anim[i].SetBool("HardRun", false);

        _speed = speed;
    }


    public void WinGame()
    {
        if (anim.Length != 0)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("Run", false);
                anim[i].SetBool("HardRun", false);
                anim[i].SetBool("WinGame", true);
            }
        }
    }

    public void LoseGame()
    {
        if (anim.Length != 0)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].SetBool("Run", false);
                anim[i].SetBool("HardRun", false);
                anim[i].SetBool("LoseGame", true);
            }
        }
    }
}
