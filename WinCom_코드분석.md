# WinCom 화면 ↔ 시뮬레이터 코드 분석

## 1. WinCom 화면 정리 (첨부 사진 기준)

| WinCom 항목 | 값 | 의미 |
|-------------|-----|------|
| **프로토콜** | UDP | WinCom이 **UDP**로 동작 중 |
| **Local Port** | 5000 | WinCom이 **UDP 5000번**으로 수신 대기 |
| **Remote IP** | 10.223.21.176 | 패킷을 보낸 쪽(시뮬레이터) IP |
| **Remote Port** | 60909 | 시뮬레이터 UDP 클라이언트 포트 (자동 할당) |
| **RX Data 계** | 6312 | WinCom이 **받은** 패킷 수 → 시뮬레이터가 **보낸** 것 |
| **TX Data 계** | 45 | WinCom이 **보낸** 패킷 수 → 시뮬레이터가 **받은** 것 |

→ **시뮬레이터 ↔ WinCom 간 송수신은 실제로 이루어지고 있음.**

---

## 2. 시뮬레이터 코드 흐름 (UDP 선택 시)

```
[Connect 창] IP 4칸 + Port(5000) 입력 → "연결하기" 클릭
    ↓
EMS_TCP_UDP_Connect.button1_Click()
    ↓
radioButton2.Checked → UdpComm 생성
    ↓
CommLoggingWrapper로 감쌈 (모든 송/수신 시 mainForm.AddCommLog 호출)
    ↓
UdpComm.Connect(ip, 5000)
    → _remoteEP = (WinCom IP, 5000)
    → UdpClient(0) 로 로컬 포트 자동 할당 (예: 60909)
    → ReceiveLoop() 백그라운드 시작
    ↓
F1 패킷 전송 → SendData → 래퍼에서 AddCommLog("송신", ...) → UdpComm.SendAsync(_remoteEP)
    ↓
WinCom이 5000번에서 수신 → RX 증가
WinCom이 10.223.21.176:60909 로 응답 전송
    ↓
UdpComm.ReceiveLoop() 가 ReceiveAsync() 로 수신
    → OnDataReceived 콜백 → 래퍼에서 AddCommLog("수신", ...) 호출
    ↓
Main.AddCommLog() → dgvCommLog에 한 줄 추가 (UI 스레드에서)
```

**정리:** WinCom에서 RX/TX가 증가한다면, 위 경로대로 `AddCommLog`가 호출되는 구조가 맞음.

---

## 3. 반드시 맞춰야 할 것

- **WinCom이 UDP** 이므로 시뮬레이터 Connect 창에서 **반드시 UDP(radioButton2)** 선택 후 연결해야 함.  
  TCP로 연결하면 WinCom 5000번(UDP)과는 연결되지 않음.
- **Port 5000** = WinCom 수신 포트이므로, 시뮬레이터 "연결하기" 시 **포트 5000** 입력.
- **Remote 60909** 는 시뮬레이터 쪽 UDP 클라이언트 포트(자동 할당)라서, 코드에서 따로 60909 입력할 필요 없음.

---

## 4. 통신 로그가 안 찍히던 가능 원인 및 수정 내용

1. **Invoke 대상**
   - 예전: `dgvCommLog.BeginInvoke(DoAdd)`  
     → 그리드가 "통신로그" **서브탭** 안에 있어서, 해당 탭을 안 열면 **핸들이 아직 없을 수 있음** → Invoke 실패/미동작 가능.
   - 수정: **Main 폼(this)** 기준으로 `InvokeRequired` / `BeginInvoke(DoAdd)` 사용.  
     → Main은 항상 떠 있으므로 핸들이 있어서, 수신 스레드에서도 UI 스레드로 안정적으로 넘어감.

2. **IsHandleCreated 체크**
   - 그리드가 서브탭 안에 있어 `IsHandleCreated`가 false인 동안 로그 추가를 막고 있을 수 있어, 해당 조건은 제거함.

---

## 5. 확인 순서

1. 시뮬레이터 **Connect** → **UDP 선택** → IP(예: 10.223.21.176) + Port **5000** → 연결하기  
2. 연결 성공 시 "연결 성공 - 통신 로그 테스트" 한 줄이 통신 로그에 찍히는지 확인  
3. **통신로그** 탭 → **통신로그** 서브탭 선택 후, H1 주기 송수신으로 줄이 계속 늘어나는지 확인  

위대로 해도 로그가 안 보이면, `AddCommLog` 호출 여부를 확인할 수 있도록 디버그 출력(예: `System.Diagnostics.Debug.WriteLine`)을 잠시 넣어서 확인하는 것을 권장함.
