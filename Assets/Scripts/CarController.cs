using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Car 이동 설정")]
    [SerializeField] private float movementSpeed = 5f; // Car의 초당 이동 속도
    private float startZ;
    private Vector3 startPosition;
    private float targetDistance = 25f;
    private bool isMoving = false;

    private void OnEnable()
    {
        // 활성화시 초기화
        startPosition = transform.position;
        startZ = startPosition.z;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        float deltaMove = -movementSpeed * Time.deltaTime; // -Z 방향으로 이동
        transform.Translate(0, 0, deltaMove, Space.World);

        // 현재 이동 거리를 계산해서 10 이상이면 비활성화
        if (startZ - transform.position.z >= targetDistance)
        {
            isMoving = false;
            gameObject.SetActive(false); // 비활성화 (pool 사용 대비)
        }
    }

    // ★ 외부에서 호출할 정지 함수
    public void StopMove()
    {
        isMoving = false;
    }

    private void OnDisable()
    {
        // 비활성화 시 초기 위치로 복귀
        transform.position = startPosition;
    }
}
