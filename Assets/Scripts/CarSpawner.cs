using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 임시 오브젝트 풀 클래스. 차후 수정예정
[System.Serializable]
public class CarPool
{
    public GameObject prefab;
    public List<GameObject> objects = new List<GameObject>();
}

public class CarSpawner : MonoBehaviour
{
    [Header("Car 프리팹 목록 (Inspector에서 드래그 앤 드랍)")]
    [Tooltip("생성할 Car 프리팹을 Inspector 창에서 직접 드래그 앤 드랍하여 추가하세요.")]
    public List<GameObject> carPrefabs = new List<GameObject>();

    [Header("Pool 당 최대 갯수")]
    [SerializeField] private int poolSizePerPoint = 5;

    [Header("스폰 위치 설정")]
    [SerializeField, Tooltip("스폰 포인트 개수 (홀수 추천, 최소 1개)")]
    private int spawnPointCount = 7;

    [SerializeField, Tooltip("스폰 포인트들의 간격(중앙 0을 기준으로 좌우로 배치됩니다)")]
    private float spawnPointSpacing = 3f;

    // 스폰 위치들
    private Vector3[] spawnPoints;

    // 오브젝트 풀: 각 spawnPoint마다 각각 프리팹별 풀 할당
    private List<GameObject>[][] carPools;

    // spawn 코루틴 핸들
    private Coroutine[] spawnCoroutines;

    private void OnEnable()
    {
        GameManager.Instance.OnGameOver += HandleGameOver;   // ★ 이벤트 구독
    }
    private void OnDisable()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;   // ★ 구독 해제
    }

    void Awake()
    {
        // 스폰 포인트 위치들 할당
        spawnPoints = new Vector3[spawnPointCount];
        float y = transform.position.y;
        float z = transform.position.z;

        float center = (spawnPointCount - 1) / 2f;   // 중앙 인덱스 기준
        for (int i = 0; i < spawnPointCount; i++)
        {
            float xOffset = (i - center) * spawnPointSpacing;
            spawnPoints[i] = new Vector3(transform.position.x + xOffset, y, z);
        }

        // 오브젝트 풀 배열 초기화 (지점 개수, 프리팹 개수별)
        carPools = new List<GameObject>[spawnPointCount][];
        for (int i = 0; i < spawnPointCount; i++)
        {
            carPools[i] = new List<GameObject>[carPrefabs.Count];
            for (int j = 0; j < carPrefabs.Count; j++)
            {
                carPools[i][j] = new List<GameObject>();
                for (int k = 0; k < poolSizePerPoint; k++)
                {
                    GameObject obj = Instantiate(carPrefabs[j]);
                    obj.SetActive(false);
                    carPools[i][j].Add(obj);
                }
            }
        }
    }

    void Start()
    {
        // 각 스폰 포인트별로 코루틴 시작
        spawnCoroutines = new Coroutine[spawnPointCount];
        for (int i = 0; i < spawnPointCount; i++)
        {
            spawnCoroutines[i] = StartCoroutine(SpawnRoutine(spawnPoints[i], i));
        }
    }

    private IEnumerator SpawnRoutine(Vector3 spawnPos, int spawnPointIdx)
    {
        // 무한 루프
        while (true)
        {
            float delay = Random.Range(2f, 4f);
            yield return new WaitForSeconds(delay);

            if (carPrefabs == null || carPrefabs.Count == 0)
                yield break;

            int prefabIdx = Random.Range(0, carPrefabs.Count);

            GameObject car = GetPooledCar(spawnPointIdx, prefabIdx);
            car.transform.position = spawnPos;
            car.SetActive(true);
        }
    }

    private void HandleGameOver()
    {
        // 1) 스폰 코루틴 정지
        if (spawnCoroutines != null)
        {
            for (int i = 0; i < spawnCoroutines.Length; i++)
            {
                if (spawnCoroutines[i] != null)
                {
                    StopCoroutine(spawnCoroutines[i]);
                }
            }
        }
        // 2) 풀 안의 Car 움직임 정지
        if (carPools == null) return;
        for (int i = 0; i < carPools.Length; i++)
        {
            if (carPools[i] == null) continue;
            for (int j = 0; j < carPools[i].Length; j++)
            {
                if (carPools[i][j] == null) continue;
                foreach (var obj in carPools[i][j])
                {
                    if (obj == null) continue;
                    // 활성화된 Car만 처리
                    if (obj.activeInHierarchy)
                    {
                        var carMove = obj.GetComponent<CarMovement>();
                        if (carMove != null)
                        {
                            carMove.StopMove();   // ★ 움직임만 정지
                        }
                    }
                }
            }
        }
    }

    private GameObject GetPooledCar(int pointIdx, int prefabIdx)
    {
        // 비활성화된 오브젝트 반환. 없으면 풀 확장
        foreach (var obj in carPools[pointIdx][prefabIdx])
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        // 풀 크기 초과 - 새로 생성후 추가
        GameObject objNew = Instantiate(carPrefabs[prefabIdx]);
        objNew.SetActive(false);
        carPools[pointIdx][prefabIdx].Add(objNew);
        return objNew;
    }
}
