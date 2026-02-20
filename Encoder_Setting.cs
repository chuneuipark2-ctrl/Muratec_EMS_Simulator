using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace EMS_TEST_SIMULATOR
{
    public class Encoder_Setting_Manager
    {
        // 핵심 데이터: Dictionary<"호기번호_포지션", 엔코더값>
        public Dictionary<string, int> vehicle_enc_datas { get; set; } = new Dictionary<string, int>();

        // UI 라벨 보관함
        private Dictionary<string, Label> _displayLabels = new Dictionary<string, Label>();

        public void Initialize(Form mainForm)
        {
            _displayLabels.Clear();
            for (int i = 101; i <= 113; i++)
            {
                string posKey = i.ToString();
                // 이미지의 속성창 확인 결과: LBL_101ENCPOS 규칙 적용
                string targetName = $"LBL_{posKey}ENCPOS";
                Control[] found = mainForm.Controls.Find(targetName, true);

                if (found.Length > 0 && found[0] is Label lb)
                {
                    _displayLabels.Add(posKey, lb);
                }
            }
        }

        // [저장] 내부 메모리에만 저장 (UI 변화 없음)
        public void ExecuteSave(string vehicleID, string pos, string inputStr)
        {
            if (string.IsNullOrEmpty(vehicleID)) { MessageBox.Show("호기번호를 선택하세요."); return; }

            if (int.TryParse(inputStr, out int val))
            {
                if (val >= 0 && val <= 530)
                {
                    string dataKey = $"{vehicleID}_{pos}";
                    if (vehicle_enc_datas.ContainsKey(dataKey))
                        vehicle_enc_datas[dataKey] = val;
                    else
                        vehicle_enc_datas.Add(dataKey, val);

                    MessageBox.Show($"{vehicleID}호기 {pos} 위치값이 메모리에 저장되었습니다.\n화면에 반영하려면 '현재값 갱신'을 누르세요.");
                }
                else { MessageBox.Show("범위 오류: 0 ~ 530"); }
            }
        }

        // [현재값 갱신] 호출 시에만 해당 호기의 데이터를 라벨에 표시
        public void DisplayVehicleData(string vehicleID)
        {
            if (string.IsNullOrEmpty(vehicleID)) return;

            foreach (var pos in _displayLabels.Keys)
            {
                string dataKey = $"{vehicleID}_{pos}";
                int val = vehicle_enc_datas.ContainsKey(dataKey) ? vehicle_enc_datas[dataKey] : -1;

                _displayLabels[pos].Text = val < 0 ? "-" : val.ToString(); // 미설정(-1)이면 -, 0 포함 설정값이면 숫자
                _displayLabels[pos].ForeColor = Color.Blue;
            }
        }

        // [초기화] 모든 호기의 데이터를 삭제하고 UI를 리셋
        public void ClearAllData()
        {
            vehicle_enc_datas.Clear(); // 메모리 삭제
            foreach (var lb in _displayLabels.Values)
            {
                lb.Text = "-"; // UI 초기화
                lb.ForeColor = Color.Black;
            }
            MessageBox.Show("모든 호기의 엔코더 설정 데이터가 초기화되었습니다.");
        }

        /// <summary>저장된 엔코더값 반환. 미설정이면 -1, 설정했으면 0~530(0 포함).</summary>
        public int GetStoredValue(string vehicleID, string pos)
        {
            string dataKey = $"{vehicleID}_{pos}";
            return vehicle_enc_datas.ContainsKey(dataKey) ? vehicle_enc_datas[dataKey] : -1;
        }
    }
}