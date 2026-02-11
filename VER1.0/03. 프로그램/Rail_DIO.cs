using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Windows.Forms;

namespace EMS_TEST_SIMULATOR
{
    public class Rail_DIO                        
    {
        //------------------------- 통신 설정 --------------------------//
        static string ipAddress_1 = "192.168.0.20";
        private const byte SLAVE_ID = 2; // 확실히 2번일 경우
        private bool Fastech_connect = false;

        //------------------------------------------------------------//

        public static Rail_DIO Instance = new Rail_DIO();

        //----------------------- 쓰래드 종료할때 쓸 객체-------------//




        public void FASTECH_CONNECT() // 외부에서 이 함수를 호출하여 연결 시작
        {
            int nRtn = 0;
            StringBuilder IpBuff = new StringBuilder(256);
            string Version = ""; // 라이브러리 public 함수 요구사항: ref string
            byte nType = 0;

            // 1. TCP 연결 시도
            Fastech_connect = FASTECH.EziMOTIONPlusELib.FAS_ConnectTCP(IPAddress.Parse(ipAddress_1), SLAVE_ID);



            if (Fastech_connect == false)
            {
                MessageBox.Show("연결 실패! IP 주소와 케이블을 확인하세요.");
                return;
            }
            else
            {

                // 연결 직후 바로 명령을 날리면 장비가 1(Fail)을 반환
                Thread.Sleep(500);

                // 2. 해당 국번(ID)의 보드가 실제로 존재하는지 확인
                int Is_Slave_Exist = FASTECH.EziMOTIONPlusELib.FAS_IsSlaveExist(SLAVE_ID);

                if (Is_Slave_Exist == 0)
                {
                    MessageBox.Show($"{SLAVE_ID}번 국번의 보드정보가 없습니다. ID 설정을 확인하세요.");
                    FASTECH.EziMOTIONPlusELib.FAS_Close(SLAVE_ID);
                    Fastech_connect = false;
                    return;
                }

                // 3. 보드 정보 읽기 (세션 최종 확정)
                // 제공해주신 라이브러리 원본에 따라 마지막 인자는 ref Version(string)을 사용합니다.
                nRtn = FASTECH.EziMOTIONPlusELib.FAS_GetSlaveInfo(SLAVE_ID, ref nType, IpBuff, ref Version);



                if (nRtn != FASTECH.EziMOTIONPlusELib.FMM_OK)
                {
                    MessageBox.Show($"세션 확립 실패. 에러코드: {nRtn}\n다른 프로그램이 점유 중인지 확인하세요.");
                    return;
                }

                // 모든 검증 완료 후 I.O 테스트 실행
                IO_TEST();
            }
        }

        //------------------------- I.O 실행 --------------------------//

        public void IO_TEST()
        {
            // 입력/출력 상태를 감시할 별도 스레드 시작
            Thread inputThread = new Thread(new ThreadStart(Func_input_status)) { IsBackground = true };
            Thread outputThread = new Thread(new ThreadStart(Func_output_status)) { IsBackground = true };

            inputThread.Start();
            outputThread.Start();

        }

        //------------------- 입력 상태 모니터링 --------------------------//
        private void Func_input_status()
        {
            uint In_get = 0;
            uint In_latch = 0;

            while (Fastech_connect)
            {
                // 주기적으로 입력을 읽어옴
                int nRtn = FASTECH.EziMOTIONPlusELib.FAS_GetInput(SLAVE_ID, ref In_get, ref In_latch);

                if (nRtn != 0)
                {
                    // 통신 에러 발생 시 로그 출력 등 처리
                    MessageBox.Show("에러발생1");
                }

                Thread.Sleep(20); // CPU 점유율 방지 (50Hz)
            }
        }

        //------------------- 출력 제어 및 상태 --------------------------//
        private void Func_output_status()
        {
            // 예시: 모든 출력을 OFF 상태로 유지하거나 특정 시퀀스 제어
            uint Out_set = 0xFFF00000;
            uint Out_clear = 0x000F0000; // 

            while (Fastech_connect)
            {

                if (!Fastech_connect) break;

                int nRtn = FASTECH.EziMOTIONPlusELib.FAS_SetOutput(SLAVE_ID, Out_set, Out_clear);




                if (nRtn != 0)
                {
                    MessageBox.Show("에러발생");
                }

                Thread.Sleep(100);
            }
        }


        public void DISCONNECT()
        {

            DataReset();

            Thread.Sleep(200);// 2. 스레드가 루프를 빠져나올 시간을 잠시 줌

            Fastech_connect = false; // 1. 스레드 루프 중단 신호


            Thread.Sleep(500);// 2. 스레드가 루프를 빠져나올 시간을 잠시 줌


    


            FASTECH.EziMOTIONPlusELib.FAS_Close(SLAVE_ID); // 3. 그 후 채널 닫기
        }

        public void DataReset()
        {
            FASTECH.EziMOTIONPlusELib.FAS_SetOutput(SLAVE_ID, 0x00000000, 0xFFFF0000);


        }

    }
}





























////study용 내가쓴코드랑 비교분석 뭐가문제인지 확인필요


///*
// using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
////----- TCP IP 통신 네임스페이스------//
//using System.Threading;
//using System.Net;
//using System.Net.Sockets;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;
//using System.Configuration;
////------------------------------------//




//namespace EMS_TEST_SIMULATOR
//{
//   public class Rail_DIO
//    {

//      //------------------------- 통신부분  --------------------------//

//        String[] DIO_IPs = { "192.168.0.20", "192.168.0.20", "192.168.0.20", "192.168.0.20", "192.168.0.20" };//RIO IP개수 
//        /* 순서대로 
//         * PanelDIO
//         * SR50 DIO1
//         * SR50 DIO2
//         * SR150 DIO1
//         * SR150 DIO2
//        */




///*


//StreamReader streamreader; // 스트림 리더 변수
//StreamWriter streamwriter; // 스트림 라이터 변수



//private void connect_rail_dio()
//{
//    TcpClient tcpClient1 = new TcpClient();  // TcpClient 객체 생성
//    IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("192.168.0.20"), int.Parse("2002"));  // Fastech IP주소와 Port번호를 할당

//    try
//    {
//        var result = tcpClient1.BeginConnect(ipEnd.Address, ipEnd.Port,null,null);
//        bool success = result.AsyncWaitHandle.WaitOne(1500); //1.5초 대기

//        if (!success)
//        {
//            throw new Exception("타임아웃: 서버응답 없음");
//        }
//        tcpClient1.EndConnect(result);
//        MessageBox.Show("연결 성공!");


//        //tcpClient1.Connect(ipEnd);  // 서버에 연결 요청
//        //MessageBox.Show("연결에 성공하였습니다.\n");
//    }
//    catch (Exception com_fault)
//    {
//        MessageBox.Show("연결에 실패하였습니다. 서버측을 점검하세요\n"+ com_fault.Message);

//        return;
//    }



//    streamreader = new StreamReader(tcpClient1.GetStream());  // 읽기 스트림 연결
//    streamwriter = new StreamWriter(tcpClient1.GetStream());  // 쓰기 스트림 연결
//    streamwriter.AutoFlush = true;  // 쓰기 버퍼 즉각 처리

//    while (tcpClient1.Connected)  // 클라이언트가 연결되어 있는 동안
//    {
//        string receivedata = streamreader.ReadLine();  // 수신 데이타를 읽어서 receivedata 변수에 저장
//        Thread.Sleep(10);//CPU 과도사용 방지
//     }
//}

//*/



////------------------------------------------------------------//
////FASTECH 라이브러리 사용 버전



//static string ipAddress_1 = "192.168.0.20";
//private const byte SLAVE_ID = 2;


//// Boolean Fastech_fault_return = FASTECH.EziMOTIONPlusELib.FAS_GetSlaveInfo(); 통신먼저 뚫고 나서 보자



//private void FASTECH_CONNECT()//통신 성공시 OR 통신 실패시 시퀀스
//{
//    //Byte sb1 = 192, sb2= 168, sb3=0, sb4=2;
//    //Byte iBdID = 0; //보드 국번
//    //StringBuilder buf = new StringBuilder(256);
//    //int nBuffSize = 256;
//    //Byte nType;
//    //int nRtn;

//    int nRtn = 0;
//    StringBuilder IpBuff = new StringBuilder(256);
//    string Version = "";
//    byte nType = 0;

//    Boolean Fastech_connect = FASTECH.EziMOTIONPlusELib.FAS_ConnectTCP(IPAddress.Parse(ipAddress_1), SLAVE_ID); // IP와 RIO 국번할당






//    if (Fastech_connect == false)
//    {
//        MessageBox.Show("연결 실패!");
//        return;
//    }

//    else
//    {

//        System.Threading.Thread.Sleep(500);

//        int Is_Slave_Exist = FASTECH.EziMOTIONPlusELib.FAS_IsSlaveExist(SLAVE_ID);


//        if (Is_Slave_Exist == 0)
//        {
//            MessageBox.Show("보드정보가 없습니다.");
//            return;
//        }

//        nRtn = FASTECH.EziMOTIONPlusELib.FAS_GetSlaveInfo(SLAVE_ID, ref nType, IpBuff, ref Version);
//        if (nRtn != FASTECH.EziMOTIONPlusELib.FMM_OK)
//        {
//            MessageBox.Show("명령수행 불가.");
//            return;
//        }

//        IO_TEST();//I.O TEST 프로시져 실행

//        return;
//    }



//}




////------------------------- I.O부분 --------------------------//

///**/
////임시로 사용하고 삭제할 예정(프로그램상 I.O TEST 용)

//public void IO_TEST() //임시함수
//{

//    Thread inputThread = new Thread(new ThreadStart(Func_input_status)) { IsBackground = true }; //멀티쓰래드 input
//    Thread outputThread = new Thread(new ThreadStart(Func_output_status)) { IsBackground = true }; //멀티쓰래드 output
//    inputThread.Start();
//    outputThread.Start();

//}

////-------------------------------------------------------------//


////------------------- 입력 상태 부분 --------------------------//
//private void Func_input_status()
//{

//    uint In_get = 0;
//    uint In_latch = 0;


//    int nRtn = FASTECH.EziMOTIONPlusELib.FAS_GetInput(SLAVE_ID, ref In_get, ref In_latch);
//    MessageBox.Show(nRtn.ToString());

//    //while (true)
//    //{
//    //    int FASTECH_RIO1_INPUT = FASTECH.EziMOTIONPlusELib.FAS_GetInput(SLAVE_ID, ref In_get, ref In_latch);
//    //    Thread.Sleep(10);//10ms 주기로 상태 업데이트 = CPU 점유율 폭주방지
//    //}
//}
////-------------------------------------------------------------//

////------------------- 출력 상태 부분 --------------------------//
//private void Func_output_status()
//{

//    uint Out_set = 0x00000000;
//    uint Out_clear = 0x00000000;

//    int nRtn = FASTECH.EziMOTIONPlusELib.FAS_SetIOOutput(SLAVE_ID, Out_set, Out_clear);
//    MessageBox.Show(nRtn.ToString());
//    while (nRtn == 0)
//    {
//        FASTECH.EziMOTIONPlusELib.FAS_SetIOOutput(SLAVE_ID, Out_set, Out_clear);
//    }

//}
//        //-------------------------------------------------------------//




//    }
//}

// */