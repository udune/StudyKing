# 📚 공부왕 (Study King)

Unity로 개발된 개인 맞춤형 학습 관리 앱으로, 귀여운 캐릭터와 함께 공부 습관을 만들어가는 스터디 트래커입니다.

## ✨ 주요 기능

### 📖 학습 관리 시스템
- **과목별 학습 계획** 생성 및 관리
- **실시간 타이머**를 통한 학습 시간 측정
- **체크리스트 방식**으로 오늘의 학습 목표 달성 추적
- **일시정지/재개** 기능으로 유연한 학습 세션 관리

### 📊 학습 분석 & 히스토리
- **총 학습 시간** 및 **주간 학습 시간** 통계
- **과목별 학습 시간** 상세 분석
- **최근 7일간 요일별** 학습 패턴 추적
- **3개월 자동 정리** - 오래된 기록은 자동 삭제하여 최적화
- **날짜별 학습 히스토리** 목록으로 나의 공부 여정 확인

### 🤖 AI 기반 학습 조언
- **OpenAI API 연동**으로 개인 맞춤형 학습 조언 제공
- 학습 패턴 분석을 통한 **일일 학습 방향 추천**
- 데이터 기반의 **동기부여 메시지** 생성

### 🎮 인터랙티브 UI/UX
- **3D 학습 공간(Room3)** - 실제 공부방 같은 몰입감
- **캐릭터 애니메이션** - 학습 상태에 따른 동적 반응
- **직관적인 터치 인터페이스** - 심플하고 사용하기 쉬운 설계
- **모달 및 알림 시스템** - 적절한 피드백과 안내

## 🏗️ 기술 스택

### Core Framework
- **Unity 2022** - 크로스플랫폼 게임 엔진
- **C# (.NET)** - 핵심 로직 및 스크립팅

### Backend & Database
- **Firebase Authentication** - Google 계정 연동 로그인
- **Firebase Firestore** - 실시간 클라우드 데이터베이스
- **Firebase Analytics** - 사용자 행동 분석
- **Firebase Remote Config** - 원격 설정 관리

### External APIs
- **Google Sign-In SDK** - 간편 로그인 구현
- **OpenAI API** - AI 기반 학습 조언 생성

### UI Framework
- **GPM UI (Infinite Scroll)** - 효율적인 스크롤 목록 관리
- **TextMeshPro** - 고품질 텍스트 렌더링
- **Custom UI System** - 모듈화된 UI 관리 아키텍처

## 📱 주요 화면 구성

```
Title Scene → Account (Google Login) → Lobby (Main Study Room)
                                        ↓
                                   Study Planning
                                        ↓
                                   Study Session
                                        ↓
                                   History & Dashboard
```

### 1. **메인 스터디룸** (`Lobby Scene`)
   - 3D 환경의 학습 공간
   - 캐릭터 인터랙션
   - 학습 시작 버튼

### 2. **학습 계획** (`StudyUI`)
   - 과목 추가/삭제/수정
   - 학습 목표 설정
   - 계획 유효성 검증

### 3. **학습 세션** (`StudyingUI`)
   - 실시간 타이머
   - 과목별 체크리스트
   - 일시정지/완료 기능

### 4. **히스토리 & 대시보드** (`HistoryTabUI`, `DashboardTabUI`)
   - 일별 학습 기록
   - 통계 차트 및 분석
   - AI 기반 학습 조언

## 🔧 핵심 아키텍처

### Data Management Pattern
```csharp
IUserData Interface
├── UserStudyData (학습 계획)
├── UserHistoryData (학습 기록)
├── UserTimeData (총 학습시간)
├── UserDailyTimeData (일별 시간)
├── UserSubjectTimeData (과목별 시간)
└── UserLastAdviceData (AI 조언)
```

### Singleton Managers
- **FirebaseManager** - 인증 및 데이터베이스 관리
- **UserDataManager** - 사용자 데이터 중앙 관리
- **UIManager** - UI 생명주기 및 상태 관리
- **LobbyManager** - 학습 세션 상태 관리
- **SceneLoader** - 씬 전환 관리

### Event-Driven Architecture
- **Observer Pattern**으로 UI 상태 동기화
- **Firebase Realtime Updates** 지원
- **Coroutine 기반** 비동기 처리

## 🚀 특별한 기술적 특징

1. **클라우드 기반 데이터 동기화** - 기기 간 학습 기록 동기화
2. **자동 데이터 정리** - 3개월 이상 된 기록 자동 삭제로 성능 최적화
3. **실시간 타이머 시스템** - 앱 포커스 상태 감지 및 자동 일시정지
4. **모듈화된 UI 시스템** - 재사용 가능한 컴포넌트 설계
5. **Firebase Analytics 연동** - 사용자 행동 패턴 분석
6. **Cross-Platform 지원** - Android/iOS 통합 빌드

## 📈 향후 개발 계획

- [ ] **통계 차트 시각화** 강화
- [ ] **목표 설정 시스템** 추가
- [ ] **학습 리마인더** 푸시 알림
- [ ] **소셜 기능** - 친구와 학습 기록 공유
- [ ] **게임화 요소** - 레벨업, 뱃지 시스템

---

> **공부왕**은 단순한 타이머 앱이 아닌, 사용자의 학습 여정을 함께하는 **디지털 스터디 파트너**입니다. 🎓
