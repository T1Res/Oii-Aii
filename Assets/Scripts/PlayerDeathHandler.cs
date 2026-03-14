using System.Collections;
using UnityEngine;

/// <summary>
/// 차량과의 충돌 시 사운드, 이펙트, 모델 비활성화, GameOver 통보까지
/// "플레이어 게임오버 연출"을 전담하는 컴포넌트.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("모델 / 이펙트")]
    [SerializeField] private Animator expAnimator;
    [SerializeField] private GameObject oiiaModelObject;

    [Header("오디오")]
    [SerializeField] private AudioSource deathAudioSource;

    /// <summary>
    /// 차량과 충돌했을 때 호출해 주는 메서드.
    /// </summary>
    public void DieByCar(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (deathAudioSource != null)
        {
            deathAudioSource.Play();
        }

        // 부딪힌 차에 Rigidbody 효과 적용: 하늘로 빙글빙글...
        Rigidbody carRb = other.attachedRigidbody != null
            ? other.attachedRigidbody
            : other.gameObject.GetComponentInParent<Rigidbody>();

        if (carRb != null)
        {
            carRb.isKinematic = false;
            carRb.velocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
            carRb.constraints = RigidbodyConstraints.None;

            Vector3 forceDir = new Vector3(0, 1, -0.3f).normalized;
            float forcePower = 10f;
            carRb.AddForce(forceDir * forcePower, ForceMode.Impulse);

            Vector3 crazyTorque = new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f)
            );
            carRb.AddTorque(crazyTorque, ForceMode.Impulse);
        }

        // GameOver 트리거 애니메이션 실행
        if (expAnimator != null)
        {
            expAnimator.SetTrigger("GameOver");
        }

        // 0.2초 뒤에 OIIA_Model을 비활성화
        if (oiiaModelObject != null)
        {
            StartCoroutine(HideOIIAModelAfterDelay(0.2f));
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager 인스턴스를 찾지 못해 GameOver를 전파하지 못했습니다.");
        }
    }

    private IEnumerator HideOIIAModelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (oiiaModelObject != null)
        {
            oiiaModelObject.SetActive(false);
        }
    }
}
