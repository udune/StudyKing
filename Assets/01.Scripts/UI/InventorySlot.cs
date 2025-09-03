using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

/// <summary>
/// 인벤토리 슬롯을 관리하는 클래스
/// 아이템의 아이콘과 상태를 표시합니다
/// </summary>
public class InventorySlot : MonoBehaviour
{
    [Header("UI 컴포넌트들")]
    [SerializeField] private Image iconImage;      // 아이템 아이콘을 표시하는 이미지
    [SerializeField] private Outline outline;     // 슬롯 테두리 (선택 표시용)
    [SerializeField] private Button slotButton;   // 슬롯 클릭 감지용 버튼
    
    [Header("슬롯 설정")]
    [SerializeField] private Color normalColor = Color.white;     // 기본 색상
    [SerializeField] private Color emptyColor = Color.clear;      // 빈 슬롯 색상
    [SerializeField] private Color selectedColor = Color.yellow;  // 선택된 슬롯 색상
    
    // 현재 슬롯에 저장된 아이템
    private InventoryItem _currentItem;
    
    // 슬롯 상태
    private bool _isEmpty = true;
    private bool _isSelected;
    
    // 슬롯 클릭 이벤트
    private System.Action<InventorySlot> _onSlotClicked;

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        // 필수 컴포넌트 확인 및 자동 연결
        ValidateAndSetupComponents();
        
        // 초기 상태 설정
        InitializeSlot();
    }
    
    /// <summary>
    /// 필수 컴포넌트들을 확인하고 자동으로 연결하는 함수
    /// </summary>
    private void ValidateAndSetupComponents()
    {
        // 아이콘 이미지 자동 찾기
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null)
            {
                Logger.LogWarning($"{GetType()}::iconImage가 없습니다. Image 컴포넌트를 추가해주세요.");
            }
        }
        
        // 아웃라인 자동 찾기
        if (outline == null)
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                Logger.LogWarning($"{GetType()}::outline이 없습니다. 선택 표시를 위해 Outline 컴포넌트를 추가하는 것을 권장합니다.");
            }
        }
        
        // 버튼 자동 찾기
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
            if (slotButton == null)
            {
                // 버튼이 없으면 자동으로 추가
                slotButton = gameObject.AddComponent<Button>();
                Logger.Log($"{GetType()}::Button 컴포넌트를 자동으로 추가했습니다");
            }
        }
        
        // 버튼 클릭 이벤트 연결
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotButtonClicked);
        }
    }
    
    /// <summary>
    /// 슬롯을 초기 상태로 설정하는 함수
    /// </summary>
    private void InitializeSlot()
    {
        // 빈 슬롯으로 초기화
        ClearSlot();
        
        Logger.Log($"{GetType()}::인벤토리 슬롯이 초기화되었습니다");
    }

    /// <summary>
    /// 슬롯에 아이템을 적용하는 함수
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    public void ApplyItem(InventoryItem item)
    {
        if (item == null)
        {
            Logger.LogWarning($"{GetType()}::적용하려는 아이템이 null입니다");
            ClearSlot();
            return;
        }
        
        try
        {
            Logger.Log($"{GetType()}::아이템 적용 - ID: {item.id}");
            
            // 현재 아이템 저장
            _currentItem = item;
            _isEmpty = false;
            
            // 아이콘 스프라이트 로드 및 적용
            LoadAndApplyIcon(item.id);
            
            // 아이콘 색상 설정
            SetIconColor(normalColor);
            
            // 아웃라인 활성화
            SetOutlineEnabled(true);
            
            Logger.Log($"{GetType()}::아이템 적용 완료 - {item.id}");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::아이템 적용 중 오류 발생: {e.Message}");
            ClearSlot();
        }
    }
    
    /// <summary>
    /// 아이콘 스프라이트를 로드하고 적용하는 함수
    /// </summary>
    private void LoadAndApplyIcon(string itemId)
    {
        if (iconImage == null || string.IsNullOrEmpty(itemId))
        {
            return;
        }
        
        try
        {
            // Resources 폴더에서 스프라이트 로드
            Sprite itemSprite = Resources.Load<Sprite>($"Texture/{itemId}");
            
            if (itemSprite != null)
            {
                iconImage.sprite = itemSprite;
                Logger.Log($"{GetType()}::아이콘 로드 성공 - {itemId}");
            }
            else
            {
                Logger.LogWarning($"{GetType()}::아이콘을 찾을 수 없습니다 - Texture/{itemId}");
                // 기본 아이콘으로 대체
                LoadDefaultIcon();
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::아이콘 로드 중 오류: {e.Message}");
            LoadDefaultIcon();
        }
    }
    
    /// <summary>
    /// 기본 아이콘을 로드하는 함수
    /// </summary>
    private void LoadDefaultIcon()
    {
        try
        {
            Sprite defaultSprite = Resources.Load<Sprite>($"Texture/DefaultItem");
            iconImage.sprite = defaultSprite != null ? defaultSprite : null;
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::기본 아이콘 로드 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 슬롯을 비우는 함수
    /// </summary>
    public void ClearSlot()
    {
        Logger.Log($"{GetType()}::슬롯을 비웁니다");
        
        try
        {
            // 아이템 정보 초기화
            _currentItem = null;
            _isEmpty = true;
            _isSelected = false;
            
            // 아이콘 제거
            if (iconImage != null)
            {
                iconImage.sprite = null;
            }
            
            // 아이콘 색상을 투명하게 설정
            SetIconColor(emptyColor);
            
            // 아웃라인 비활성화
            SetOutlineEnabled(false);
            
            Logger.Log($"{GetType()}::슬롯 비우기 완료");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::슬롯 비우기 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 아이콘 색상을 설정하는 함수
    /// </summary>
    private void SetIconColor(Color color)
    {
        if (iconImage != null)
        {
            iconImage.color = color;
        }
    }
    
    /// <summary>
    /// 아웃라인 활성화/비활성화를 설정하는 함수
    /// </summary>
    private void SetOutlineEnabled(bool _enabled)
    {
        if (outline != null)
        {
            outline.enabled = _enabled;
        }
    }

    /// <summary>
    /// 슬롯 선택 상태를 설정하는 함수
    /// </summary>
    /// <param name="selected">선택 여부</param>
    private void SetSelected(bool selected)
    {
        _isSelected = selected;
        
        if (outline != null)
        {
            outline.enabled = selected || !_isEmpty; // 선택되거나 아이템이 있으면 아웃라인 표시
            
            if (selected)
            {
                outline.effectColor = selectedColor;
                outline.effectDistance = new Vector2(3, -3); // 선택 시 더 굵게
            }
            else if (!_isEmpty)
            {
                outline.effectColor = normalColor;
                outline.effectDistance = new Vector2(1, -1); // 평상시 얇게
            }
        }
        
        Logger.Log($"{GetType()}::슬롯 선택 상태 변경: {selected}");
    }
    
    /// <summary>
    /// 슬롯 버튼이 클릭되었을 때 호출되는 함수
    /// </summary>
    private void OnSlotButtonClicked()
    {
        Logger.Log($"{GetType()}::슬롯이 클릭되었습니다 - 비어있음: {_isEmpty}");
        
        try
        {
            // 클릭 이벤트 호출
            _onSlotClicked?.Invoke(this);
            
            // 선택 상태 토글 (비어있지 않은 경우에만)
            if (!_isEmpty)
            {
                SetSelected(!_isSelected);
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::슬롯 클릭 처리 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 슬롯에 마우스가 올라갔을 때의 효과
    /// </summary>
    public void OnPointerEnter()
    {
        if (!_isEmpty && !_isSelected)
        {
            // 호버 효과 (약간 밝게)
            SetIconColor(Color.Lerp(normalColor, Color.white, 0.3f));
        }
    }
    
    /// <summary>
    /// 슬롯에서 마우스가 벗어났을 때의 효과
    /// </summary>
    public void OnPointerExit()
    {
        if (!_isEmpty && !_isSelected)
        {
            // 원래 색상으로 복원
            SetIconColor(normalColor);
        }
    }

    #region 프로퍼티들 (외부에서 접근 가능한 정보들)
    
    /// <summary>
    /// 현재 슬롯의 아이템을 반환
    /// </summary>
    public InventoryItem CurrentItem => _currentItem;
    
    /// <summary>
    /// 슬롯이 비어있는지 확인
    /// </summary>
    public bool IsEmpty => _isEmpty;
    
    /// <summary>
    /// 슬롯이 선택되었는지 확인
    /// </summary>
    public bool IsSelected => _isSelected;
    
    /// <summary>
    /// 슬롯의 아이템 ID 반환 (null이면 empty string)
    /// </summary>
    private string ItemId => _currentItem?.id ?? "";
    
    #endregion

    #region 유틸리티 함수들
    
    /// <summary>
    /// 슬롯 상태를 디버그 로그로 출력하는 함수
    /// </summary>
    public void LogSlotStatus()
    {
        Logger.Log($"{GetType()}::슬롯 상태 - 비어있음: {_isEmpty}, 선택됨: {_isSelected}, 아이템: {ItemId}");
    }
    
    /// <summary>
    /// 슬롯을 강제로 새로고침하는 함수
    /// </summary>
    public void RefreshSlot()
    {
        if (_currentItem != null)
        {
            // 현재 아이템으로 다시 적용
            var tempItem = _currentItem;
            ClearSlot();
            ApplyItem(tempItem);
        }
        else
        {
            ClearSlot();
        }
        
        Logger.Log($"{GetType()}::슬롯을 새로고침했습니다");
    }
    
    /// <summary>
    /// 슬롯의 색상 테마를 변경하는 함수
    /// </summary>
    public void SetColorTheme(Color normal, Color empty, Color selected)
    {
        normalColor = normal;
        emptyColor = empty;
        selectedColor = selected;
        
        // 현재 상태에 맞게 색상 적용
        if (_isEmpty)
        {
            SetIconColor(emptyColor);
        }
        else if (_isSelected)
        {
            SetIconColor(selectedColor);
        }
        else
        {
            SetIconColor(normalColor);
        }
        
        Logger.Log($"{GetType()}::색상 테마가 변경되었습니다");
    }
    
    #endregion
    
    /// <summary>
    /// 오브젝트가 파괴될 때 호출되는 함수
    /// </summary>
    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
        }
        
        // 이벤트 콜백 정리
        _onSlotClicked = null;
        
        Logger.Log($"{GetType()}::인벤토리 슬롯이 정리되었습니다");
    }

    #region 레거시 호환성 (기존 코드와의 호환성)
    
    /// <summary>
    /// 레거시 호환성을 위한 함수 (Apply -> ApplyItem)
    /// </summary>
    [System.Obsolete("Apply 메서드는 더 이상 사용되지 않습니다. ApplyItem을 사용하세요.")]
    public void Apply(InventoryItem item)
    {
        ApplyItem(item);
    }
    
    /// <summary>
    /// 레거시 호환성을 위한 함수 (Clear -> ClearSlot)
    /// </summary>
    [System.Obsolete("Clear 메서드는 더 이상 사용되지 않습니다. ClearSlot을 사용하세요.")]
    public void Clear()
    {
        ClearSlot();
    }
    
    #endregion
}