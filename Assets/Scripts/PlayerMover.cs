using UnityEngine;

/// <summary>
/// 플레이어 입력(A/D 키)을 직접 읽어 실제 위치를 이동시키고,
/// 이동 시 애니메이션 및 이동 사운드를 제어하는 컴포넌트.
/// </summary>
public class PlayerMover : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("애니메이션 / 오디오")]
    [SerializeField] private Animator oiiaAnimator;
    [SerializeField] private AudioSource moveAudioSource;

    /// <summary>
    /// -1 ~ 1 범위의 좌우 입력 값 (A = -1, D = 1)
    /// 외부에서 참고만 할 수 있도록 공개.
    /// </summary>
    public float Horizontal { get; private set; }

    private void Awake()
    {
        if (moveAudioSource == null)
        {
            moveAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Horizontal = 0f;
            StopMoveSound();
            SetMovingAnimation(false);
            return;
        }

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

        Horizontal = input;

        // 월드 기준 X축 방향으로만 이동
        Vector3 move = new Vector3(Horizontal * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(move, Space.World);

        bool isMoving = Horizontal != 0f;

        // 애니메이션 제어
        SetMovingAnimation(isMoving);

        // 움직이는 소리 제어
        HandleMoveSound(isMoving);
    }

    private void SetMovingAnimation(bool isMoving)
    {
        if (oiiaAnimator != null)
        {
            oiiaAnimator.SetBool("isMoving", isMoving);
        }
    }

    private void HandleMoveSound(bool isMoving)
    {
        if (moveAudioSource == null) return;

        if (isMoving)
        {
            if (!moveAudioSource.isPlaying)
            {
                moveAudioSource.Play();
            }
        }
        else
        {
            StopMoveSound();
        }
    }

    private void StopMoveSound()
    {
        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
        }
    }
}
