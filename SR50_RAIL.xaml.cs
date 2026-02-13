using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EMS_TEST_SIMULATOR
{
    public partial class UserControl1 : UserControl
    {
        private DispatcherTimer _timer;
        private double _currentX = 50;   // 실제 표시 위치 (부드럽게 보간)
        private double _targetX = 50;    // EMS 현재 섹션에 따른 목표 X
        private const double LerpFactor = 0.14; // 0~1, 클수록 빠르게 따라감

        // 센서들을 관리할 리스트
        private List<Rectangle> _sensors = new List<Rectangle>();
        private const int SensorCount = 13;
        private const double StartPos = 50;
        private const double EndPos = 700;

        string[] sensorNames = { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113" };


        public UserControl1()
        {
            InitializeComponent();
            CreateSensors(); // 센서 동적 생성

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void CreateSensors()
        {
            double interval = (EndPos - StartPos) / (SensorCount - 1);

            for (int i = 0; i < SensorCount; i++)
            {
                Grid sensorUnit = new Grid();

                Rectangle rect = new Rectangle
                {
                    Width = 35,
                    Height = 25,
                    Fill = Brushes.Gray, // 기본 색상
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    RadiusX = 3,
                    RadiusY = 3
                };

                TextBlock text = new TextBlock
                {
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White
                };

                // --- 조건별 색상 및 문구 설정 ---
                if (i == 0) // 4번째 박스를 U/D로 설정
                {
                    text.Text = "U/D";
                    rect.Fill = Brushes.SteelBlue; // U/D는 파란 계열
                    rect.Tag = "UD"; // 나중에 UpdateSensors에서 색 복원용으로 사용
                }
                else if (i == 12) // 8번째 박스를 L/D로 설정
                {
                    text.Text = "L/D";
                    rect.Fill = Brushes.IndianRed; // L/D는 붉은 계열
                    rect.Tag = "LD";
                }
                else if(i==9)
                {
                    text.Text = "Wait";
                    rect.Fill = Brushes.Violet;
                    rect.Tag = "wait";
                }

                else
                {
                    text.Text = sensorNames[i];
                    rect.Fill = Brushes.Gray;
                    rect.Tag = "NORMAL";
                }

                sensorUnit.Children.Add(rect);
                sensorUnit.Children.Add(text);
                sensorUnit.Margin = new Thickness(i == 0 ? 0 : interval - 35, 0, 0, 0);

                _sensors.Add(rect);
                SensorContainer.Children.Add(sensorUnit);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // EMS 현재 위치로 목표 X만 갱신
            if (!string.IsNullOrEmpty(RailStatus.CurrentSectionCount) && int.TryParse(RailStatus.CurrentSectionCount, out int sectionNum) && sectionNum >= 101 && sectionNum <= 113)
            {
                int index = sectionNum - 101;
                double interval = (EndPos - StartPos) / (SensorCount - 1);
                _targetX = StartPos + (index * interval);
            }

            // 현재 위치를 목표로 부드럽게 보간 (휙휙 점프 방지)
            double diff = _targetX - _currentX;
            if (Math.Abs(diff) < 0.5)
                _currentX = _targetX;
            else
                _currentX += diff * LerpFactor;

            if (MovingTransform != null)
                MovingTransform.X = _currentX;

            UpdateSensors();
        }

        private void UpdateSensors()
        {
            double interval = (EndPos - StartPos) / (SensorCount - 1);

            for (int i = 0; i < _sensors.Count; i++)
            {
                double sensorX = StartPos + (i * interval);

                if (Math.Abs(_currentX - sensorX) < 15)
                {
                    _sensors[i].Fill = Brushes.LimeGreen; // 감지 시 공통 초록색
                }
                else
                {
                    // 감지되지 않을 때 원래 색상으로 복구
                    string type = _sensors[i].Tag.ToString();
                    if (type == "UD") _sensors[i].Fill = Brushes.SteelBlue;
                    else if (type == "LD") _sensors[i].Fill = Brushes.IndianRed;
                    else if (type == "wait") _sensors[i].Fill = Brushes.Violet;
                    else _sensors[i].Fill = Brushes.Gray;
                }
            }
        }
    }
}