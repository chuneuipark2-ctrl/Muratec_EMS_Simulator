using System;

namespace EMS_TEST_SIMULATOR
{
    /// <summary>
    /// 타워램프(파스텍 슬레이브 ID 2번 DO 마스크). Rail_DIO 출력 스레드에서 주기적으로 ApplyHardware 호출.
    /// </summary>
    public enum TowerLampVisualMode
    {
        /// <summary>평상시: 레일 5대 미전부 연결</summary>
        IdleRedSteady,
        /// <summary>RAIL I.O 성공 후 20~24 전부 연결</summary>
        RailAllOkBlueSteady,
        /// <summary>일부만 연결 실패 등</summary>
        RailErrorRedBlink,
        /// <summary>AUTO 확인 대화상자 전</summary>
        AutoConfirmGreenBlink,
        /// <summary>AUTO Yes 후 ~ 인터록/1초 대기 끝</summary>
        AutoAfterConfirmBlueBlink,
        /// <summary>H4 접수 대기</summary>
        AutoH4WaitGreenBlink,
        /// <summary>H2 사이클 가동</summary>
        AutoH2RunGreenBlink,
        /// <summary>사용자 사이클 정지 처리</summary>
        AutoCycleStopYellowBlink,
        /// <summary>EMO 동작 중</summary>
        EmoRedBlink,
        /// <summary>EMS 동작이상·H4 타임아웃 등(해제 시까지 상시 적)</summary>
        EmsFaultRedSteady,
    }

    public static class TowerLamp
    {
        private const byte BoardId = 2;
        private static readonly object _sync = new object();
        private static TowerLampVisualMode _mode = TowerLampVisualMode.IdleRedSteady;

        public static void SetMode(TowerLampVisualMode mode)
        {
            lock (_sync) { _mode = mode; }
        }

        public static TowerLampVisualMode GetMode()
        {
            lock (_sync) { return _mode; }
        }

        public static void ApplyHardware()
        {
            TowerLampVisualMode mode;
            lock (_sync) { mode = _mode; }

            bool half = (Environment.TickCount / 500) % 2 == 0;

            switch (mode)
            {
                case TowerLampVisualMode.IdleRedSteady:
                case TowerLampVisualMode.EmsFaultRedSteady:
                    FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x80000000, 0x7FFFFFFF);
                    break;

                case TowerLampVisualMode.RailAllOkBlueSteady:
                    FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x10000000, 0xEFFFFFFF);
                    break;

                case TowerLampVisualMode.RailErrorRedBlink:
                case TowerLampVisualMode.EmoRedBlink:
                    if (half)
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x80000000, 0x7FFFFFFF);
                    else
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x00000000, 0xFFFFFFFF);
                    break;

                case TowerLampVisualMode.AutoConfirmGreenBlink:
                case TowerLampVisualMode.AutoH4WaitGreenBlink:
                case TowerLampVisualMode.AutoH2RunGreenBlink:
                    if (half)
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x2AAA0000, 0xD555FFFF);
                    else
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x00000000, 0xFFFFFFFF);
                    break;

                case TowerLampVisualMode.AutoAfterConfirmBlueBlink:
                    if (half)
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x10000000, 0xEFFFFFFF);
                    else
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x00000000, 0xFFFFFFFF);
                    break;

                case TowerLampVisualMode.AutoCycleStopYellowBlink:
                    if (half)
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x4AAA0000, 0xB555FFFF);
                    else
                        FASTECH.EziMOTIONPlusELib.FAS_SetOutput(BoardId, 0x00000000, 0xFFFFFFFF);
                    break;
            }
        }
    }
}
