using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UIPool : SingletonBehaviour<UIPool>
{
    [Header("풀 설정")]
    [SerializeField] private int maxPoolSize = 50; // 전체 풀 크기
    [SerializeField] private int maxPoolSizePerType = 10; // 타입별 최대 풀 크기
    [SerializeField] private bool enableLogging = true; // 로그 출력 여부
    
    // 타입별 UI 오브젝트 풀 딕셔너리
    private Dictionary<Type, Queue<GameObject>> poolDictionary = new Dictionary<Type, Queue<GameObject>>();
    
    // 풀에서 현재 사용 중인 오브젝트들을 추적한다
    private Dictionary<Type, HashSet<GameObject>> activeObjects = new Dictionary<Type, HashSet<GameObject>>();
    
    // 전체 풀 크기 추적
    private int totalPooledObjects = 0;
    
    // 풀 컨테이너
    private Transform poolContainer;

    // 싱글톤 초기화
    protected override void Init()
    {
        base.Init();
        
        // 풀 컨테이너 생성
        CreatePoolContainer();
        
        Logger.Log($"{GetType()}::UIPool 초기화 완료");
    }
    
    // 풀 컨테이너를 생성하는 함수
    private void CreatePoolContainer()
    {
        GameObject containerObj = new GameObject("UIPoolContainer");
        containerObj.transform.SetParent(transform);
        containerObj.SetActive(false);
        poolContainer = containerObj.transform;
        
        Logger.Log($"{GetType()}::UIPool 컨테이너 생성 완료");
    }

    // 특정 타입의 UI 오브젝트를 풀에서 가져오는 함수
    public T GetFromPool<T>() where T : BaseUI
    {
        Type type = typeof(T);

        try
        {
            // 풀이 없으면 생성한다
            EnsurePoolExists(type);

            var pool = poolDictionary[type];
            var activeSet = activeObjects[type];

            GameObject obj = null;

            // 풀에서 오브젝트를 가져온다
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
                totalPooledObjects--;
                
                if (enableLogging)
                {
                    Logger.Log($"{GetType()}::풀에서 오브젝트를 가져왔습니다: {obj.name}");
                }
            }
            else
            {
                // 풀에 오브젝트가 없으면 새로운 오브젝트를 생성한다
                obj = CreateNewUIObject<T>();
                
                if (enableLogging)
                {
                    Logger.Log($"{GetType()}::새로운 오브젝트를 생성했습니다: {obj.name}");
                }
            }

            if (obj != null)
            {
                // 활성 오브젝트 목록에 추가한다.
                activeSet.Add(obj);
                
                // 오브젝트 활성화한다.
                obj.SetActive(true);
                
                return obj.GetComponent<T>();
            }
            
            Logger.LogError($"{GetType()}::오브젝트를 가져오는 데 실패했습니다. 타입: {type}");
            return null;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::오브젝트를 가져오는 중 오류 발생: {e.Message}");
            throw;
        }
    }
    
    // 새로운 UI 오브젝트를 생성하는 함수
    private GameObject CreateNewUIObject<T>() where T : BaseUI
    {
        Type type = typeof(T);

        try
        {
            // Resources 폴더에서 프리팹을 로드한다.
            GameObject prefab = Resources.Load<GameObject>($"UI/{type}");
            
            if (prefab == null)
            {
                Logger.LogError($"{GetType()}::프리팹을 찾을 수 없습니다: UI/{type}");
                return null;
            }

            // 프리팹을 인스턴스화한다.
            GameObject newObj = Instantiate(prefab);
            
            // 컴포넌트를 확인한다.
            T component = newObj.GetComponent<T>();
            if (component == null)
            {
                Logger.LogError($"{GetType()}::{type} 프리팹에 BaseUI 컴포넌트가 없습니다.");
                Destroy(newObj);
                return null;
            }
            
            return newObj;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::새로운 UI 오브젝트 생성 중 오류 발생: {e.Message}");
            return null;
        }
    }
    
    // UI 오브젝트를 풀에 반환하는 함수
    public void ReturnToPool<T>(T obj) where T : BaseUI
    {
        if (obj == null)
        {
            Logger.LogWarning($"{GetType()}::반환하려는 UI 오브젝트가 null입니다");
            return;
        }
        
        Type type = typeof(T);
        GameObject go = obj.gameObject;
        
        try
        {
            // 풀이 없으면 생성한다.
            EnsurePoolExists(type);
            
            var pool = poolDictionary[type];
            var activeSet = activeObjects[type];

            // 활성 오브젝트 목록에서 제거한다.
            if (activeSet.Contains(go))
            {
                activeSet.Remove(go);
            }

            // 풀이 가득차면 오브젝트 파괴
            if (pool.Count >= maxPoolSizePerType)
            {
                if (enableLogging)
                {
                    Logger.Log($"{GetType()}::풀 크기가 최대치({maxPoolSizePerType})에 도달했습니다. 오브젝트를 제거합니다: {go.name}");
                }
                
                Destroy(go);
                return;
            }
            
            // 전체 풀 크기 확인
            if (totalPooledObjects >= maxPoolSize)
            {
                // 전체 풀이 차면 가장 오래된 오브젝트를 정리한다.
                CleanOldestPoolObject();
            }
            
            // 오브젝트를 풀에 추가하기 전에 초기화
            PrepareObjectForPool(go);
            
            // 풀에 추가
            pool.Enqueue(go);
            totalPooledObjects++;

            if (enableLogging)
            {
                Logger.Log($"{GetType()}::오브젝트를 풀에 반환했습니다: {go.name}");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::오브젝트를 풀에 반환하는 중 오류 발생: {e.Message}");

            // 오류 발생 시 오브젝트 파괴
            if (go != null)
            {
                Destroy(go);
            }
        }
    }
    
    // 오브젝트를 풀에 반환하기 전에 초기화하는 함수
    private void PrepareObjectForPool(GameObject obj)
    {
        // 오브젝트를 비활성화하고 초기화
        obj.SetActive(false);
        
        // 풀 컨테이너로 이동
        obj.transform.SetParent(poolContainer);
        
        // 위치와 크기를 초기화
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }

    // 가장 오래된 오브젝트를 정리하는 함수
    private void CleanOldestPoolObject()
    {
        foreach (var poorPair in poolDictionary)
        {
            var pool = poorPair.Value;
            if (pool.Count > 0)
            {
                GameObject oldestObj = pool.Dequeue();
                totalPooledObjects--;

                if (oldestObj != null)
                {
                    Destroy(oldestObj);
                }

                if (enableLogging)
                {
                    Logger.Log($"{GetType()}::가장 오래된 오브젝트를 제거했습니다: {oldestObj.name}");
                }

                return;
            }
        }
    }
    
    // 해당 타입의 풀이 존재하는지, 없으면 생성하는 함수
    private void EnsurePoolExists(Type type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary[type] = new Queue<GameObject>();
            activeObjects[type] = new HashSet<GameObject>();
            
            if (enableLogging)
            {
                Logger.Log($"{GetType()}::타입 {type}의 풀을 생성했습니다");
            }
        }
    }
    
    // 특정 타입의 풀을 정리하는 함수
    public void ClearPool<T>() where T : BaseUI
    {
        Type type = typeof(T);
        ClearPool(type);
    }

    // 특정 타입의 풀을 정리하는 함수
    private void ClearPool(Type type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Logger.LogWarning($"{GetType()}::타입 {type}의 풀은 존재하지 않습니다.");
            return;
        }

        try
        {
            var pool = poolDictionary[type];
            var activeSet = activeObjects[type];

            // 풀에 있는 모든 오브젝트를 제거한다.
            int destroyedCount = 0;
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                    destroyedCount++;
                    totalPooledObjects--;
                }
            }
            
            // 활성 오브젝트 목록에서 제거
            foreach (var activeObj in activeSet)
            {
                if (activeObj != null)
                {
                    Destroy(activeObj);
                    destroyedCount++;
                }
            }
            
            activeSet.Clear();
            
            Logger.Log($"{GetType()}::타입 {type}의 풀을 정리했습니다. 제거된 오브젝트 수: {destroyedCount}");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::타입 {type}의 풀을 정리하는 중 오류 발생: {e.Message}");
            throw;
        }
    }

    // 모든 풀을 정리하는 함수
    public void ClearAllPools()
    {
        Logger.Log($"{GetType()}::모든 UI 풀을 정리합니다");

        try
        {
            List<Type> typesToClear = new List<Type>(poolDictionary.Keys);

            foreach (var type in typesToClear)
            {
                ClearPool(type);
            }
            
            poolDictionary.Clear();
            activeObjects.Clear();
            totalPooledObjects = 0;
            
            Logger.Log($"{GetType()}::모든 UI 풀 정리 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::모든 UI 풀 정리 중 오류 발생: {e.Message}");
            throw;
        }
    }

    // 풀 상태를 로그로 출력하는 함수
    public void LogPoolStatus()
    {
        Logger.Log($"{GetType()}::현재 UI 풀 상태:");
        Logger.Log($"총 풀 크기: {totalPooledObjects}/{maxPoolSize}");

        foreach (var poolPair in poolDictionary)
        {
            Type type = poolPair.Key;
            Queue<GameObject> pool = poolPair.Value;
            HashSet<GameObject> activeSet = activeObjects[type];

            Logger.Log($"타입: {type}, 풀 크기: {pool.Count}, 활성 오브젝트 수: {activeSet.Count}");
        }
        
        Logger.Log("풀 상태 로그 완료");
    }

    // 특정 타입의 활성화된 오브젝트 수를 반환하는 함수
    public int GetActiveObjectCount<T>() where T : BaseUI
    {
        Type type = typeof(T);

        if (activeObjects.ContainsKey(type))
        {
            return activeObjects[type].Count;
        }

        return 0;
    }

    // 특정 타입의 풀 크기를 반환하는 함수
    public int GetPoolSize<T>() where T : BaseUI
    {
        Type type = typeof(T);

        if (poolDictionary.ContainsKey(type))
        {
            return poolDictionary[type].Count;
        }

        return 0;
    }
    
    // 전체 풀 크기를 반환하는 함수
    public int GetTotalPoolSize()
    {
        return totalPooledObjects;
    }
    
    protected override void OnDestroy()
    {
        // 모든 풀 정리
        ClearAllPools();
        
        base.OnDestroy();
        
        Logger.Log($"{GetType()}::UIPool이 파괴되었습니다");
    }
}
