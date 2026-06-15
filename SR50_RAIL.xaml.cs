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
        private double _currentX = RailVisualMotion.StartPos;
        private double _targetX = RailVisualMotion.StartPos;
        private double _anchorX = RailVisualMotion.StartPos;
        private const double LerpFactor = 0.12;

        // 센서들을 관리할 리스트
        private List<Rectangle> _sensors = new List<Rectangle>();
        private const int SensorCount = RailVisualMotion.SensorCount;
        private const double StartPos = RailVisualMotion.StartPos;
        private const double EndPos = RailVisualMotion.EndPos;

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

                // --- 조건별 색상 및 문구 설정 (ST01~ST03 표기) ---
                if (i == 0)
                {
                    text.Text = "ST01";
                    rect.Fill = Brushes.SteelBlue;
                    rect.Tag = "UD";
                }
                else if (i == 12)
                {
                    text.Text = "ST03";
                    rect.Fill = Brushes.IndianRed;
                    rect.Tag = "LD";
                }
                else if (i == 9)
                {
                    text.Text = "ST02";
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
            RailVisualMotion.Tick(ref _currentX, ref _targetX, ref _anchorX, LerpFactor);

            if (MovingTransform != null)
                MovingTransform.X = _currentX;

            UpdateSensors();
        }

        private void UpdateSensors()
        {
            double interval = RailVisualMotion.DogInterval;

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