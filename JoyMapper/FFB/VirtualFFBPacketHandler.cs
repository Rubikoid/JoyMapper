using JoyMapper.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.FFB {
    public static class VirtualFFBPacketHandler {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("VFFBPacket");

        public delegate void FFBDataReceiveEventHandler(VirtualFFBPacket e);
        public static event FFBDataReceiveEventHandler FFBDataReceived;
        private static FFBDataReceiveEventHandler[] handlers = new FFBDataReceiveEventHandler[16];

        private const uint ERROR_SUCCESS = 0x0;
        private const uint ERROR_INVALID_PARAMETER = 0x57;
        private const uint ERROR_INVALID_DATA = 0xD;

        private static vJoy joystick;

        public static void Init() {
            joystick = ControllerCache.vc.joystick;
            joystick.FfbRegisterGenCB(FFBPacketCallback, null);
            FFBDataReceived += FFBDataReceivedHandler;
        }

        public static void AddFFBHandler(uint joyId, FFBDataReceiveEventHandler handler) {
            if (joyId >= 16)
                return;
            handlers[joyId] = handler;
        }

        public static void RemoveFFBHandler(uint joyId) {
            if (joyId >= 16)
                return;
            handlers[joyId] = null;
        }

        private static void FFBDataReceivedHandler(VirtualFFBPacket e) {
            uint id = e.ID;
            handlers[id]?.Invoke(e);
        }

        public static void FFBPacketCallback(IntPtr data, object userData) {
            if (data != IntPtr.Zero)
                Task.Run(() => { ProcessFFBPacket(data, userData); });
        }

        public static void ProcessFFBPacket(IntPtr data, object userData) {
            VirtualFFBPacket FFBPacket = new VirtualFFBPacket();
            if (joystick.Ffb_h_Type(data, ref FFBPacket._FFBPType) == ERROR_SUCCESS) {
                joystick.Ffb_h_DeviceID(data, ref FFBPacket.ID);
                joystick.Ffb_h_EffNew(data, ref FFBPacket.FFBENextType);
                switch (FFBPacket._FFBPType) {
                    case FFBPType.PT_EFFREP: // Effect Report (also named EFFECT_CONST)
                        if (joystick.Ffb_h_Eff_Report(data, ref FFBPacket.FFB_EFF_REPORT) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = FFBPacket.FFB_EFF_REPORT.EffectBlockIndex;

                            /*int num = 0;
                            int num2 = 0;
                            if (FFBPacket.FFB_EFF_REPORT.Polar) {
                                Utils.GetDICartesianXY((int)FFBPacket.FFB_EFF_REPORT.DirX, ref num, ref num2);
                            } else {
                                num = (int)FFBPacket.FFB_EFF_REPORT.DirX;
                                num2 = (int)FFBPacket.FFB_EFF_REPORT.DirY;
                            }*/

                            StringBuilder dat = new StringBuilder();
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Effect Report");
                            dat.AppendFormat($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Effect Report\n");
                            dat.AppendFormat($"\t EBI={FFBPacket.FFB_EFF_REPORT.EffectBlockIndex}\n");
                            dat.AppendFormat($"\t Type={FFBPacket.FFB_EFF_REPORT.EffectType}\n");
                            dat.AppendFormat($"\t Dir={FFBPacket.FFB_EFF_REPORT.Direction}\n");
                            dat.AppendFormat($"\t DirX={FFBPacket.FFB_EFF_REPORT.DirX}\n");
                            dat.AppendFormat($"\t DirY={FFBPacket.FFB_EFF_REPORT.DirY}\n");
                            dat.AppendFormat($"\t Duration={FFBPacket.FFB_EFF_REPORT.Duration}\n");
                            dat.AppendFormat($"\t Gain={FFBPacket.FFB_EFF_REPORT.Gain}\n");
                            dat.AppendFormat($"\t Polar={FFBPacket.FFB_EFF_REPORT.Polar}\n");
                            dat.AppendFormat($"\t SamplePrd={FFBPacket.FFB_EFF_REPORT.SamplePrd}\n");
                            dat.AppendFormat($"\t TriggerBtn={FFBPacket.FFB_EFF_REPORT.TrigerBtn}\n");
                            dat.AppendFormat($"\t TriggerRpt={FFBPacket.FFB_EFF_REPORT.TrigerRpt}");
                            //dat.AppendFormat($"\t \n");
                            //dat.AppendFormat($"\t DirX={num}; DirY={num2}\n");
                            logger.Trace(dat.ToString());
                        }
                        break;
                    case FFBPType.PT_ENVREP: // Envelope Report
                        if (joystick.Ffb_h_Eff_Envlp(data, ref FFBPacket.FFB_EFF_ENVLP) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_ENVLP.EffectBlockIndex;
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Envelope Report");
                        }
                        break;
                    case FFBPType.PT_CONDREP: // Condition Report !!
                        if (joystick.Ffb_h_Eff_Cond(data, ref FFBPacket.FFB_EFF_COND) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_COND.EffectBlockIndex;
                            StringBuilder dat = new StringBuilder();
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Condition Report");
                            dat.AppendFormat($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Condition Report\n");
                            dat.AppendFormat($"\t EBI={FFBPacket.FFB_EFF_COND.EffectBlockIndex}\n");
                            dat.AppendFormat($"\t Center={FFBPacket.FFB_EFF_COND.CenterPointOffset}\n");
                            dat.AppendFormat($"\t DeadBand={FFBPacket.FFB_EFF_COND.DeadBand}\n");
                            dat.AppendFormat($"\t isY={FFBPacket.FFB_EFF_COND.isY}\n");
                            dat.AppendFormat($"\t NegCoeff={FFBPacket.FFB_EFF_COND.NegCoeff}\n");
                            dat.AppendFormat($"\t NegSatur={FFBPacket.FFB_EFF_COND.NegSatur}\n");
                            dat.AppendFormat($"\t PosCoeff={FFBPacket.FFB_EFF_COND.PosCoeff}\n");
                            dat.AppendFormat($"\t PosSatur={FFBPacket.FFB_EFF_COND.PosSatur}");
                            logger.Trace(dat);
                        }
                        break;
                    case FFBPType.PT_PRIDREP: // Periodic Report
                        if (joystick.Ffb_h_Eff_Period(data, ref FFBPacket.FFB_EFF_PERIOD) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_PERIOD.EffectBlockIndex;
                            StringBuilder dat = new StringBuilder();
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Periodic Report");
                            dat.AppendFormat($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Periodic Report\n");
                            dat.AppendFormat($"\t EBI={FFBPacket.FFB_EFF_PERIOD.EffectBlockIndex}\n");
                            dat.AppendFormat($"\t Magnitude={FFBPacket.FFB_EFF_PERIOD.Magnitude}\n");
                            dat.AppendFormat($"\t Offset={FFBPacket.FFB_EFF_PERIOD.Offset}\n");
                            dat.AppendFormat($"\t Period={FFBPacket.FFB_EFF_PERIOD.Period}\n");
                            dat.AppendFormat($"\t Phase={FFBPacket.FFB_EFF_PERIOD.Phase}");
                            logger.Trace(dat);
                        }
                        break;
                    case FFBPType.PT_CONSTREP: // Constant Force Report
                        if (joystick.Ffb_h_Eff_Constant(data, ref FFBPacket.FFB_EFF_CONSTANT) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_CONSTANT.EffectBlockIndex;
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Constant Force Report, magnitude={FFBPacket.FFB_EFF_CONSTANT.Magnitude}");
                        }
                        break;
                    case FFBPType.PT_RAMPREP: // Ramp Force Report
                        if (joystick.Ffb_h_Eff_Ramp(data, ref FFBPacket.FFB_EFF_RAMP) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_RAMP.EffectBlockIndex;
                            StringBuilder dat = new StringBuilder();
                            logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Ramp Force Report");
                            dat.AppendFormat($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Ramp Force Report\n");
                            dat.AppendFormat($"\t EBI={FFBPacket.FFB_EFF_RAMP.EffectBlockIndex}\n");
                            dat.AppendFormat($"\t Start={FFBPacket.FFB_EFF_RAMP.Start}\n");
                            dat.AppendFormat($"\t End={FFBPacket.FFB_EFF_RAMP.End}\n");
                            logger.Trace(dat);
                        }
                        break;
                    case FFBPType.PT_CSTMREP: // Custom Force Data Report
                        logger.Debug($"Custom Force Data Report");
                        break;
                    case FFBPType.PT_SMPLREP: // Download Force Sample
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Download Force Sample");
                        break;
                    case FFBPType.PT_EFOPREP: // Effect Operation Report
                        if (joystick.Ffb_h_EffOp(data, ref FFBPacket.FFB_EFF_OP) == ERROR_SUCCESS) {
                            FFBPacket.BlockIndex = (uint)FFBPacket.FFB_EFF_OP.EffectBlockIndex;
                            // logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Effect Operation Report loop={FFBPacket.FFB_EFF_OP.LoopCount} action={FFBPacket.FFB_EFF_OP.EffectOp}");
                        }
                        break;
                    case FFBPType.PT_BLKFRREP: // PID Block Free Report
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] PID Block Free Report");
                        break;
                    case FFBPType.PT_CTRLREP: // PID Device Control
                        joystick.Ffb_h_DevCtrl(data, ref FFBPacket.FFB_CTRL);
                        logger.Debug($"[{FFBPacket.ID}][_] PID Device Control {FFBPacket.FFB_CTRL}");
                        break;
                    case FFBPType.PT_GAINREP: // Device Gain Report
                        joystick.Ffb_h_DevGain(data, ref FFBPacket.Gain);
                        logger.Debug($"[{FFBPacket.ID}][_] Device Gain Report={FFBPacket.Gain}");
                        break;
                    case FFBPType.PT_SETCREP: // Set Custom Force Report
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Set Custom Force Report");
                        break;
                    case FFBPType.PT_NEWEFREP: // Create New Effect Report !!
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        joystick.Ffb_h_CreateNewEffect(data, ref FFBPacket.FFBENextType, ref FFBPacket.NextBlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Create New Effect Report, next={FFBPacket.FFBENextType}[nextEBI={FFBPacket.NextBlockIndex}]");
                        break;
                    case FFBPType.PT_BLKLDREP: // Block Load Report
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] Block Load Report");
                        break;
                    case FFBPType.PT_POOLREP: // PID Pool Report
                        joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        logger.Debug($"[{FFBPacket.ID}][EBI={FFBPacket.BlockIndex}] PID Pool Report");
                        break;
                }
                FFBDataReceived?.Invoke(FFBPacket);
            }
            return;
        }
    }
}
