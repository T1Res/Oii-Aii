using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    public static event Action OnGameOver;

    private Animator oiiaAnimator;
    private Animator expAnimator;
    private bool isGameOver = false;
    private GameObject oiiaModelObject; 

    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioSource deathAudioSource;

    private void Awake()
    {
        Transform playerModel = transform.Find("PlayerModel");
        if (playerModel != null)
        {
            Transform oiiaModel = playerModel.Find("OIIA_Model");
            if (oiiaModel != null)
            {
                oiiaAnimator = oiiaModel.GetComponent<Animator>();
                oiiaModelObject = oiiaModel.gameObject;

                if (oiiaAnimator != null)
                {
                    /*
                    Debug.Log("OIIA_Model의 Animator를 찾았습니다."); 
                    */
                }
                else
                {
                    Debug.LogWarning("OIIA_Model에서 Animator를 찾지 못했습니다.");
                }
                
            }
            else
            {
                Debug.LogWarning("PlayerModel의 하위에 OIIA_Model을 찾지 못했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerModel을 찾지 못했습니다.");
        }

        // ExplodeEffect의 Animator 할당
        Transform explodeEffect = transform.Find("ExplodeEffect");
        if (explodeEffect != null)
        {
            expAnimator = explodeEffect.GetComponent<Animator>();
            if (expAnimator != null)
            {
                /*
                Debug.Log("ExplodeEffect의 Animator를 찾았습니다.");
                */
            }
            else
            {
                Debug.LogWarning("ExplodeEffect에서 Animator를 찾지 못했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("ExplodeEffect를 찾지 못했습니다.");
        }

          moveAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isGameOver) return;

        float input = 0f;
        // A 키: 왼쪽(-1), D 키: 오른쪽(+1)
        if (Input.GetKey(KeyCode.A))
        {
            input = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            input = 1f;
        }
        else
        {
            input = 0f;
        }

        // 월드 기준 X축 방향으로만 이동
        Vector3 move = new Vector3(input * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(move, Space.World);
        
        bool isMoving = input != 0f;

        // 애니메이션 제어
        if (oiiaAnimator != null)
        {
            oiiaAnimator.SetBool("isMoving", isMoving);
        }

        // 움직이는 소리 제어
        if (moveAudioSource != null)
        {
            if (isMoving)
            {
                if (!moveAudioSource.isPlaying)
                {
                    moveAudioSource.Play();
                }
            }
            else
            {
                if (moveAudioSource.isPlaying)
                {
                    moveAudioSource.Stop();
                }
            }
        }
    }

    // 차량과 충돌 시 게임 오버 처리
    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        // Car 태그와 충돌했는지 확인
        if (other.CompareTag("Car"))
        {
            isGameOver = true;

            // 움직이는 소리 정지 및 죽음 소리 재생
            if (moveAudioSource != null && moveAudioSource.isPlaying)
            {
                moveAudioSource.Stop();
            }

            if (deathAudioSource != null)
            {
                deathAudioSource.Play();
            }

            // 부딪힌 차에 Rigidbody 효과 적용: 하늘로 빙글빙글...
            // collider가 자식에 붙어있을 수 있으니 attachedRigidbody를 우선 사용
            Rigidbody carRb = other.attachedRigidbody != null
                ? other.attachedRigidbody
                : other.gameObject.GetComponentInParent<Rigidbody>();
            if (carRb != null)
            {
                // 물리 작동 활성화
                carRb.isKinematic = false;
                carRb.velocity = Vector3.zero; // 기존 속도 제거
                carRb.angularVelocity = Vector3.zero;
                // 혹시 제약이 걸려있다면 풀어줘서 날아가기가 확실히 보이게 함
                carRb.constraints = RigidbodyConstraints.None;

                // 위로 + 약간 뒤로 강하게 튕겨내기 (Y up, -Z 뒤)
                Vector3 forceDir = new Vector3(0, 1, -0.3f).normalized;
                float forcePower = 10f;
                carRb.AddForce(forceDir * forcePower, ForceMode.Impulse);

                // X, Y, Z축 전부 토크로 빙글빙글
                Vector3 crazyTorque = new Vector3(
                    UnityEngine.Random.Range(-50f, 50f),
                    UnityEngine.Random.Range(-50f, 50f),
                    UnityEngine.Random.Range(-50f, 50f)
                );
                carRb.AddTorque(crazyTorque, ForceMode.Impulse);
            }

            // GameOver 트리거 애니메이션 실행
            if (expAnimator != null)
            {
                expAnimator.SetTrigger("GameOver");
                /*
                Debug.Log("GameOver 트리거를 실행했습니다.");
                */
            }

            // 0.2초 뒤에 OIIA_Model을 비활성화
            if (oiiaModelObject != null)
            {
                StartCoroutine(HideOIIAModelAfterDelay(0.2f));
            }

            OnGameOver?.Invoke();
        }
    }

    // OIIA_Model을 비활성화하는 코루틴
    private IEnumerator HideOIIAModelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (oiiaModelObject != null)
        {
            oiiaModelObject.SetActive(false);
        }
    }
}
