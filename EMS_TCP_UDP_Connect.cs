using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public partial class EMS_TCP_UDP_Connect : Form
    {
        // 1. 통신, 프로토콜 객체 정의
        public IDeviceComm _comm;
        private EMS_Protocol _emsProto;
        public IDeviceComm Comm => _comm;
        public EMS_Protocol EmsProto => _emsProto;

        // 2. UI 상태 제어용 변수
        private bool _isHostActive = false;
        private bool _isSkyRavActive = false;
        private bool _isClosing = false;
        private Timer _blinkTimer;
        private Timer _statusTimer;

        public EMS_TCP_UDP_Connect()
        {
            InitializeComponent();
            _emsProto = new EMS_Protocol(); // 프로토콜 객체 초기화

            // 점멸 타이머 설정 (0.5초 간격)
            _blinkTimer = new Timer();
            _blinkTimer.Interval = 500;
            _blinkTimer.Tick += BlinkTimer_Tick;
            _blinkTimer.Start();

            // 상태 문의 타이머 설정 (0.3초 간격으로 H1 패킷 전송)
            _statusTimer = new Timer();
            _statusTimer.Interval = 300;
            _statusTimer.Tick += StatusTimer_Tick;
        }

        private void EMS_TCP_UDP_Connect_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
        }

        // 주기적으로 장비에 현재 상태를 물어봅니다.
        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (_comm != null && _isHostActive)
            {
                try
                {
                    // EMS_Protocol을 통해 규격에 맞는 H1 패킷(시퀀스3자리+체크섬) 생성
                    byte[] h1Packet = _emsProto.EMS_Link_check_and_Inquire();
                    string statusRequest = Encoding.ASCII.GetString(h1Packet);

                    await _comm.SendData(statusRequest);
                }
                catch
                {
                    if (!_isClosing) _statusTimer.Stop();
                }
            }
        }

        // 연결하기 버튼
        private async void button1_Click(object sender, EventArgs e)
        {
            string ipAddress = $"{textBox1.Text}.{textBox2.Text}.{textBox3.Text}.{textBox4.Text}";
            if (!int.TryParse(textBox5.Text, out int port))
            {
                MessageBox.Show("유효한 Port 번호를 입력해주세요.");
                return;
            }

            if (radioButton1.Checked) _comm = new TcpComm();
            else if (radioButton2.Checked) _comm = new UdpComm();
            else { MessageBox.Show("TCP 또는 UDP를 선택해주세요."); return; }

            // 데이터 수신 시 프로토콜 파서로 전달
            _comm.OnDataReceived = (data) =>
            {
                _emsProto.ReceiveFromDevice(data);
                _isSkyRavActive = true;
            };

            try
            {
                await _comm.Connect(ipAddress, port);
                _isHostActive = true;

                var responseTcs = new TaskCompletionSource<bool>();
                _comm.OnDataReceived = (data) =>
                {
                    _emsProto.ReceiveFromDevice(data);
                    _isSkyRavActive = true;
                    RailStatus.CurrentSectionCount = _emsProto.Parser.CurrentStatus?.CurrentSectionCount ?? "";
                    responseTcs.TrySetResult(true);
                };

                // F1 패킷 전송
                byte[] f1Packet = _emsProto.EMS_Link_Connect();
                await _comm.SendData(Encoding.ASCII.GetString(f1Packet));

                var delayTask = Task.Delay(3000);
                var completedTask = await Task.WhenAny(responseTcs.Task, delayTask);

                if (completedTask == responseTcs.Task)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        if (this.Owner is Main mainForm)
                        {
                            mainForm.GlobalComm = this._comm;
                        }

                        _statusTimer.Start(); // H1 폴링 시작

                        button1.Enabled = false;
                        button2.Enabled = true;
                        this.DialogResult = DialogResult.OK;

                        MessageBox.Show("연결 성공! 상태 모니터링을 시작합니다.");
                        this.Hide();
                    }));
                }
                else
                {
                    _comm.Disconnect();
                    _isHostActive = false;
                    MessageBox.Show("응답 없음: 데이터링크 확립 실패");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"연결 오류: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _statusTimer.Stop();
            _comm?.Disconnect();

            _isHostActive = false;
            _isSkyRavActive = false;
            panel1.BackColor = Color.Gray;
            panel2.BackColor = Color.Gray;

            if (this.Owner is Main mainForm)
            {
                mainForm.GlobalComm = null;
                mainForm.ResetConnectButton();
            }

            button1.Enabled = true;
            button2.Enabled = false;
            MessageBox.Show("연결이 해제되었습니다.");
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            if (_isHostActive)
                panel1.BackColor = (panel1.BackColor == Color.Red) ? Color.Gray : Color.Red;
            if (_isSkyRavActive)
                panel2.BackColor = (panel2.BackColor == Color.Red) ? Color.Gray : Color.Red;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            _statusTimer?.Stop();
            _blinkTimer?.Stop();
            base.OnFormClosing(e);
        }
    }

    // ==========================================================
    // 여기서부터 통신 클래스들 (빨간 줄 해결 구간)
    // ==========================================================

    public interface IDeviceComm
    {
        Task Connect(string ip, int port);
        Task SendData(string message);
        void Disconnect();
        Action<byte[]> OnDataReceived { get; set; }
    }

    public class TcpComm : IDeviceComm
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public Action<byte[]> OnDataReceived { get; set; }

        public async Task Connect(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
            _ = Task.Run(ReceiveLoop);
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (_client != null && _client.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);
                        OnDataReceived?.Invoke(data);
                    }
                    else break;
                }
            }
            catch { }
        }

        public async Task SendData(string message)
        {
            if (_stream == null || !_client.Connected) return;
            byte[] data = Encoding.ASCII.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public void Disconnect()
        {
            try { _stream?.Dispose(); _client?.Close(); _client = null; _stream = null; } catch { }
        }
    }

    public class UdpComm : IDeviceComm
    {
        private UdpClient _udpClient;
        private IPEndPoint _remoteEP;
        public Action<byte[]> OnDataReceived { get; set; }

        public Task Connect(string ip, int port)
        {
            _udpClient = new UdpClient(0);
            _remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
            _ = Task.Run(ReceiveLoop);
            return Task.CompletedTask;
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (_udpClient != null)
                {
                    var result = await _udpClient.ReceiveAsync();
                    OnDataReceived?.Invoke(result.Buffer);
                }
            }
            catch { }
        }

        public async Task SendData(string message)
        {
            if (_udpClient == null) return;
            byte[] data = Encoding.ASCII.GetBytes(message);
            await _udpClient.SendAsync(data, data.Length, _remoteEP);
        }

        public void Disconnect()
        {
            try { _udpClient?.Dispose(); _udpClient = null; } catch { }
        }
    }
}