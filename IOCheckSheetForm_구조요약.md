# IOCheckSheetForm 구조 요약

## 1. 전체 흐름 (한 줄 요약)

**폼 로드 / I/O 적용 클릭** → 콤보값 읽기 → **BuildTableFromEmbeddedData()** → **ProcessEmbeddedInputData()** + **ProcessEmbeddedOutputData()** → `_allIoTable`에 행 추가 → **ApplyFilter()** → **DataGridView**에 표시

---

## 2. 데이터 저장소

| 이름 | 타입 | 용도 |
|------|------|------|
| **`_allIoTable`** | `DataTable` | 실제 그리드에 보여줄 **전체 I/O 행** (INPUT+OUTPUT). 여기에 행을 Add 한다. |
| **`EmbeddedInputData`** | `List<string[]>` | INPUT용 **원본 행 데이터** (어드레스, BIT, 신호명/논리 조합). |
| **`EmbeddedOutputData`** | `List<string[]>` | OUTPUT용 **원본 행 데이터**. |

- **Embedded 데이터**는 **신호명/논리 선택용**만 사용됨.  
  **IO PIN-NO, IF 기판명칭 등 중간 4열**은 Embedded에 없고, `Rows.Add(…, "", "", "", "", …)` 에서 **빈 문자열로만** 넣고 있음.

---

## 3. _allIoTable 열 구조 (인덱스 순서)

| 인덱스 | 열 이름 | Rows.Add에서 채우는 값 (현재) |
|--------|---------|------------------------------|
| 0 | 어드레스 | `addr` |
| 1 | IO구분 | `"INPUT"` / `"OUTPUT"` |
| 2 | BIT | `bit` |
| 3 | IO 커넥터No. | `"I/O기판"` (고정) |
| 4 | IO PIN-NO | `""` ← **여기서부터 직접 수정 가능** |
| 5 | IF 기판명칭 | `""` |
| 6 | IF 커넥터NO | `""` |
| 7 | IF PIN-NO. | `""` |
| 8 | 신호명칭 | `finalSignal` |
| 9 | 논리 | `finalLogic` |
| 10 | I.O 체크 | `false` |
| 11 | 비고 | `""` |

- **행을 추가하는 곳**:  
  - INPUT: **ProcessEmbeddedInputData()** 끝부분 `_allIoTable.Rows.Add(…)` (한 군데)  
  - OUTPUT: **ProcessEmbeddedOutputData()** 안의 `_allIoTable.Rows.Add(…)` (여러 군데, 800018 추가 행 포함)

---

## 4. EmbeddedInputData 한 행 구조 (row[인덱스])

- **0**: 어드레스  
- **1**: BIT  
- **2**: 기본신호  
- **3**: 기본논리  
- **4,5**: CHUCK 신호, 논리  
- **6,7**: CAGE 신호, 논리  
- **8,9**: 컨베이어 신호, 논리  
- **10,11**: 충돌방지  
- **12,13**: ROP  
- **14,15**: 8bit  
- **16,17**: SS무선  
- **18,19**: 직선  
- **20,21**: 분기  
- **22,23**: 옵션  

→ **ProcessEmbeddedInputData()** 안에서 `hoistType`, `commType`, `collision`, `formType`, `branchDevice`, `layout`, `cargoProtrusion`, `liftStop`, `extraOpt` 등에 따라 **row[2]~row[23]** 중에서 `finalSignal`, `finalLogic` 을 골라서 사용.

---

## 5. EmbeddedOutputData 한 행 구조 (row[인덱스])

- **0**: 어드레스, **1**: BIT, **2**: 기본신호, **3**: 기본논리  
- **4,5**: CHUCK  
- **6,7**: CAGE  
- **8,9**: 컨베이어  
- **10,11**: ROP  
- **12,13**: 분기  
- **14,15**: 옵션  

→ **ProcessEmbeddedOutputData()** 안에서 위와 비슷하게 **finalSignal / finalLogic** 선택.

---

## 6. 주요 메서드 역할

| 메서드 | 줄 번호 대략 | 역할 |
|--------|--------------|------|
| **IOCheckSheetForm_Load** | 189 | 콤보 기본값 설정 → BuildTableFromEmbeddedData → FillAddressFilterCombo → ApplyFilter → SetGridColumnWidths |
| **BuildTableFromEmbeddedData** | 224 | _allIoTable 컬럼 정의 + 콤보값 수집 → ProcessEmbeddedInputData / ProcessEmbeddedOutputData 호출 |
| **ProcessEmbeddedInputData** | 296 | EmbeddedInputData 한 행씩 읽어서, 옵션/비트 제외 후 **finalSignal/finalLogic** 결정 → **_allIoTable.Rows.Add(…)** (INPUT 행 1곳) |
| **ProcessEmbeddedOutputData** | 595 | EmbeddedOutputData 동일 방식 → **_allIoTable.Rows.Add(…)** (OUTPUT 여러 곳 + 800018 추가) |
| **ApplyFilter** | 819 | comboFilter / checkBoxUsedSignalFilter 에 따라 _allIoTable 을 필터링해 DataGridView 에 바인딩 |
| **SetGridColumnWidths** | 775 | 그리드 열 너비·IO구분 숨김 설정 |
| **FillAddressFilterCombo** | 791 | _allIoTable 의 어드레스 목록으로 comboFilter 채움 |

---

## 7. 중간 4열(IO PIN-NO, IF 기판명칭 등) 수정 시 참고

- **값을 넣으려면**:  
  **ProcessEmbeddedInputData** / **ProcessEmbeddedOutputData** 안의  
  `_allIoTable.Rows.Add(addr, "INPUT", bit, "I/O기판", "", "", "", "", finalSignal, finalLogic, false, "");`  
  에서 **5번째~8번째 인자** (`"", "", "", ""`) 를 원하는 값 또는 변수로 바꾸면 됨.

- **Embedded 데이터에서 가져오려면**:  
  EmbeddedInputData/EmbeddedOutputData 의 각 행(`string[]`)에 **새 열(인덱스)** 를 추가하고,  
  Process 메서드 안에서 `row[새인덱스]` 로 읽어서 Rows.Add 의 5~8번째 자리에 넣으면 됨.

---

## 8. 옵션/필터 관련

- **ExcludedInputBits / ExcludedOutputBits**: 특정 (어드레스, BIT) 는 그리드에 아예 넣지 않음.  
- **옵션설정 콤보** (충돌검출, 극한검출, 이재모터 수, 8bit전송정지): **GetOptionSettingIndex()** 로 1 또는 2 → Process 메서드 안에서 특정 BIT 표시/제외.  
- **comboFilter**: 어드레스별 필터. **checkBoxUsedSignalFilter**: I.O 체크된 행만 보기.

이 문서는 현재 코드 기준으로 작성됨. 열 이름이 Designer/다른 부분과 다르면 그쪽과 맞춰서 수정하면 됨.
