using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EMS_TEST_SIMULATOR
{
    public partial class UserControl2 : UserControl
    {
        private DispatcherTimer _timer;
        private double _currentX = 50;
        private int direction = 0; // 0: 전진, 1: 후진

        private List<Rectangle> _sensors = new List<Rectangle>();
        private const int SensorCount = 13;
        private const double StartPos = 50;
        private const double EndPos = 700;

        // 센서 기본 이름 정의
        string[] sensorNames = { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113" };

        public UserControl2()
        {
            InitializeComponent();
            CreateSensors(); // 업데이트 버전의 동적 생성 로직

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(20); // SR150의 부드러운 속도 유지 (50fps)
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void CreateSensors()
        {
            double interval = (EndPos - StartPos) / (SensorCount - 1);

            for (int i = 0; i < SensorCount; i++)
            {
                Grid sensorUnit = new Grid();
                Rectangle rect = new Rectangle { Width = 35, Height = 25, Stroke = Brushes.Black, StrokeThickness = 1, RadiusX = 3, RadiusY = 3 };
                TextBlock text = new TextBlock { FontSize = 10, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White };

                // --- 업데이트 버전의 Tag 기반 타입 설정 ---
                if (i == 0)
                {
                    text.Text = "U/D";
                    rect.Fill = Brushes.SteelBlue;
                    rect.Tag = "UD";
                }
                else if (i == 12)
                {
                    text.Text = "L/D";
                    rect.Fill = Brushes.IndianRed;
                    rect.Tag = "LD";
                }
                else if (i == 9)
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
                sensorUnit.Margin = new Thickness(i == 0 ? 0 : interval - 35, 0, 0, 0);// i가 0일때 0출력 0이 아닐때 (왼쪽, 위, 오른쪽, 아래: -35, 0, 0, 0 순서 적용)

                _sensors.Add(rect);
                SensorContainer.Children.Add(sensorUnit);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // SR150 특유의 이동 속도 (1.5px) 및 왕복 로직 적용
            if (direction == 0)
            {
                _currentX += 1.5;
                if (_currentX >= EndPos) direction = 1;
            }
            else
            {
                _currentX -= 1.5;
                if (_currentX <= StartPos) direction = 0;
            }

            if (RavTransform_SR150 != null) RavTransform_SR150.X = _currentX;

            UpdateSensors();
        }

        private void UpdateSensors()
        {
            double interval = (EndPos - StartPos) / (SensorCount - 1);

            for (int i = 0; i < _sensors.Count; i++)
            {
                double sensorX = StartPos + (i * interval);

                // SR150은 차체가 조금 더 크므로 감지 범위(Threshold)를 20으로 살짝 넓힘
                if (Math.Abs(_currentX - sensorX) < 20)
                {
                    _sensors[i].Fill = Brushes.LimeGreen;
                }
                else
                {
                    // Tag를 활용한 색상 복원
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