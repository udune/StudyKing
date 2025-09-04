using System;
using UnityEngine;
using Logger = Common.Logger;

public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;

    public BaseUIData()
    {
        OnShow = null;
        OnClose = null;
    }

    public BaseUIData(Action onShow, Action onClose)
    {
        OnShow = onShow;
        OnClose = onClose;
    }
}

public class BaseUI : MonoBehaviour
{
    [Header("UI 애니메이션")] 
    [SerializeField] protected Animation openAnimation;
    [SerializeField] protected Animation closeAnimation;

    [Header("UI 설정")] 
    // 배경 클릭 시 닫기
    [SerializeField] protected bool closeOnBgClick;
    // UI가 열릴 때 일시정지
    [SerializeField] protected bool pauseGameWhenOpen;

    // 초기화 완료
    private bool _isInit;
    
    // 현재 표시되고 있는지
    private bool _isShow;
    
    private Action _onShow;
    private Action _onClose;

    // 시간 스케일
    private float _originTimeScale = 1.0f;

    public virtual void Init(Transform anchor)
    {
        if (_isInit)
        {
            Logger.Log($"{GetType()}::Init is already init");
            return;
        }
        
        Logger.Log($"{GetType()}::Init");

        try
        {
            // 부모 설정
            SetupParent(anchor);

            // RectTrn 설정
            SetupRectTrn();

            // 배경 클릭 이벤트 설정
            SetupBgClickHandler();

            OnInit();
            
            _isInit = true;
            Logger.Log($"{GetType()}::Init is done");
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}::Init is failed");
            throw;
        }
    }

    private void SetupParent(Transform anchor)
    {
        if (anchor == null)
        {
            Logger.Log($"{GetType()}::anchor is null");
            return;
        }
        
        transform.SetParent(anchor, false);
    }

    private void SetupRectTrn()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            Logger.Log($"{GetType()}::rect is null");
            return;
        }
        
        // 화면 전체를 덮도록 한다.
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetupBgClickHandler()
    {
        if (!closeOnBgClick) { }
        
        // TODO
    }

    public virtual void Setting(BaseUIData data)
    {
        if (data == null)
        {
            Logger.Log($"{GetType()}::data is null");
            // 기본 데이터 생성
            data = new BaseUIData();
        }
        
        Logger.Log($"{GetType()}::Setting");

        try
        {
            _onShow = data.OnShow;
            _onClose = data.OnClose;

            OnSetting(data);
            
            Logger.Log($"{GetType()}::Setting is done");
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}::Setting is failed");
            throw;
        }
    }

    public virtual void ShowUI()
    {
        if (_isShow)
        {
            Logger.Log($"{GetType()}::is already show");
            return;
        }
        
        Logger.Log($"{GetType()}::ShowUI");

        try
        {
            _isShow = true;

            PauseHandle(true);

            PlayOpenAnim();

            ExecCallback(_onShow, "OnShow");

            OnShow();
            
            Logger.Log($"{GetType()}::ShowUI is done");
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}::ShowUI is failed");
            _isShow = false;
            throw;
        }
    }

    private void PauseHandle(bool isPause)
    {
        if (!pauseGameWhenOpen)
        {
            return;
        }

        if (isPause)
        {
            _originTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;
            Logger.Log($"{GetType()}::Pause");
        }
        else
        {
            Time.timeScale = _originTimeScale;
            Logger.Log($"{GetType()}::Resume");
        }
    }

    private void PlayOpenAnim()
    {
        if (openAnimation != null && openAnimation.clip != null)
        {
            openAnimation.Play();
            Logger.Log($"{GetType()}::PlayOpenAnim");
        }
    }

    private void PlayCloseAnim()
    {
        if (closeAnimation != null && closeAnimation.clip != null)
        {
            closeAnimation.Play();
            Logger.Log($"{GetType()}::PlayCloseAnim");
        }
    }

    private void ExecCallback(Action callback, string methodName)
    {
        if (callback == null)
        {
            return;
        }

        try
        {
            callback.Invoke();
            Logger.Log($"{GetType()}::{methodName} is done");
        }
        catch (Exception)
        {
            Logger.Log($"{GetType()}::{methodName} is failed");
            throw;
        }
    }
    
    public virtual void CloseUI(bool isForce = false)
    {
        if (!_isShow && !isForce)
        {
            Logger.Log($"{GetType()}::이미 닫혀있는 UI입니다");
            return;
        }
    
        Logger.Log($"{GetType()}::CloseUI");

        try
        {
            _isShow = false;

            PauseHandle(false);

            PlayCloseAnim();

            ExecCallback(_onClose, "OnClose");

            OnClose();
        
            // UIManager에 UI 닫힘을 알림
            UIManager.Instance?.OnUIClosed(this);
        
            Logger.Log($"{GetType()}::CloseUI 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::CloseUI 실패: {e.Message}");
        }
    }

    private void ClearCallbacks()
    {
        _onShow = null;
        _onClose = null;
    }

    public virtual void OnClickClose()
    {
        Logger.Log($"{GetType()}::OnClickClose");
        CloseUI();
    }

    protected virtual void Update()
    {
        // 뒤로 가기 키 감지(안드로이드)
        if (Input.GetKeyDown(KeyCode.Escape) && _isShow)
        {
            OnBackKeyPressed();
        }
    }

    protected virtual void OnBackKeyPressed()
    {
        Logger.Log($"{GetType()}::OnBackKeyPressed");
        CloseUI();
    }
    
    #region Virtual
    // UI 초기화 시 호출
    protected virtual void OnInit()
    {
        // 상속받는 클래스에서 구현
    }

    // UI 설정 시 호출
    protected virtual void OnSetting(BaseUIData data)
    {
        // 상속받는 클래스에서 구현
    }

    // UI 표시 시 호출
    protected virtual void OnShow()
    {
        // 상속받는 클래스에서 구현
    }

    // UI 닫기 시 호출
    protected virtual void OnClose()
    {
        // 상속받는 클래스에서 구현
    }
    #endregion
    
    #region Util
    // UI가 초기화되었는가.
    public bool IsInit => _isInit;
    
    // UI가 현재 표시되고 있는가.
    public bool IsShow => _isShow;

    // UI를 강제로 표시 상태로 변경
    public void SetShowState(bool show)
    {
        _isShow = show;
        Logger.Log($"{GetType()}::SetShowState {show}");
    }

    // UI 배경 클릭 닫기 변경
    public void SetCloseBgClick(bool enabled)
    {
        closeOnBgClick = enabled;
        Logger.Log($"{GetType()}::SetCloseBgClick {enabled}");
    }

    // UI 일시정지 설정 변경
    public void SetPauseWhenOpen(bool enabled)
    {
        pauseGameWhenOpen = enabled;
        Logger.Log($"{GetType()}::SetPauseWhenOpen {enabled}");
    }
    #endregion

    // UI 파괴 시 호출
    // 메모리 누수 방지
    protected virtual void OnDestroy()
    {
        if (pauseGameWhenOpen && Time.timeScale == 0f)
        {
            Time.timeScale = _originTimeScale;
        }
        
        ClearCallbacks();
        
        Logger.Log($"{GetType()}::OnDestroy");
    }
}
