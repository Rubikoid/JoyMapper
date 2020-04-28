using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper.FFB {
    public class VirtualFFBPacketHandler {
        private vJoy joystick;

        private const uint ERROR_SUCCESS = 0x0;
        private const uint ERROR_INVALID_PARAMETER = 0x57;
        private const uint ERROR_INVALID_DATA = 0xD;

        public VirtualFFBPacketHandler(vJoy joystick) {
            this.joystick = joystick;
        }

        public void ProcessFFBPacket(IntPtr data, object userData, Action<FFBEventArgs> callback) {
            VirtualFFBPacket FFBPacket = new VirtualFFBPacket();
            if (this.joystick.Ffb_h_Type(data, ref FFBPacket.FFBPType) == 0u) {
                this.joystick.Ffb_h_EffNew(data, ref FFBPacket.FFBENextType);
                switch (FFBPacket.FFBPType) {
                    case FFBPType.PT_EFFREP: // Effect Report
                        if (this.joystick.Ffb_h_Eff_Report(data, ref FFBPacket.FFB_EFF_REPORT) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_REPORT.EffectBlockIndex;
                            Console.WriteLine($"Effect Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_ENVREP: // Envelope Report
                        if (this.joystick.Ffb_h_Eff_Envlp(data, ref FFBPacket.FFB_EFF_ENVLP) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_ENVLP.EffectBlockIndex;
                            Console.WriteLine($"Envelope Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_CONDREP: // Condition Report !!
                        if (this.joystick.Ffb_h_Eff_Cond(data, ref FFBPacket.FFB_EFF_COND) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_COND.EffectBlockIndex;
                            Console.WriteLine($"Condition Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_PRIDREP: // Periodic Report
                        if (this.joystick.Ffb_h_Eff_Period(data, ref FFBPacket.FFB_EFF_PERIOD) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_PERIOD.EffectBlockIndex;
                            Console.WriteLine($"Periodic Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_CONSTREP: // Constant Force Report 
                        if (this.joystick.Ffb_h_Eff_Constant(data, ref FFBPacket.FFB_EFF_CONSTANT) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_CONSTANT.EffectBlockIndex;
                            Console.WriteLine($"Constant Force Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_RAMPREP: // Ramp Force Report 
                        if (this.joystick.Ffb_h_Eff_Ramp(data, ref FFBPacket.FFB_EFF_RAMP) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_RAMP.EffectBlockIndex;
                            Console.WriteLine($"Ramp Force Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_CSTMREP: // Custom Force Data Report
                        Console.WriteLine($"Custom Force Data Report");
                        break;
                    case FFBPType.PT_SMPLREP: // Download Force Sample
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("Download Force Sample");
                        break;
                    case FFBPType.PT_EFOPREP: // Effect Operation Report
                        if (this.joystick.Ffb_h_EffOp(data, ref FFBPacket.FFB_EFF_OP) == 0u) {
                            FFBPacket.BlockIndex = (int)FFBPacket.FFB_EFF_OP.EffectBlockIndex;
                            Console.WriteLine($"Effect Operation Report {FFBPacket.BlockIndex}");
                        }
                        break;
                    case FFBPType.PT_BLKFRREP: // PID Block Free Report
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("PID Block Free Report");
                        break;
                    case FFBPType.PT_CTRLREP: // PID Device Contro
                        this.joystick.Ffb_h_DevCtrl(data, ref FFBPacket.FFB_CTRL);
                        Console.WriteLine("PID Device Contro");
                        break;
                    case FFBPType.PT_GAINREP: // Device Gain Report
                        this.joystick.Ffb_h_DevGain(data, ref FFBPacket.Gain);
                        Console.WriteLine("Device Gain Report");
                        break;
                    case FFBPType.PT_SETCREP: // Set Custom Force Report
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("Set Custom Force Report");
                        break;
                    case FFBPType.PT_NEWEFREP: // Create New Effect Report !!
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("Create New Effect Report");
                        break;
                    case FFBPType.PT_BLKLDREP: // Block Load Report
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("Block Load Report");
                        break;
                    case FFBPType.PT_POOLREP: // PID Pool Report
                        this.joystick.Ffb_h_EBI(data, ref FFBPacket.BlockIndex);
                        Console.WriteLine("PID Pool Report");
                        break;
                }
                //obj = this.FfbLock;
                //lock (obj) {
                //	this.FFBPackets.Enqueue(vJoyFFBPacket);
                //}
                //this.NewFfbItemEvent.Set();
                //return vJoyFFBPacket;
            }
            //return null;
            return;


            FFBEventArgs args = new FFBEventArgs();
            FFBPType packetType = new FFBPType();
            this.joystick.Ffb_h_Type(data, ref packetType);

            // extract FFB data from packet based on packet type
            switch (packetType) {
                case FFBPType.PT_EFFREP: // Effect Report
                    vJoy.FFB_EFF_REPORT effectReport = new vJoy.FFB_EFF_REPORT();
                    if (this.joystick.Ffb_h_Eff_Report(data, ref effectReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_ENVREP: // Envelope Report
                    vJoy.FFB_EFF_ENVLP envelopeReport = new vJoy.FFB_EFF_ENVLP();
                    if (this.joystick.Ffb_h_Eff_Envlp(data, ref envelopeReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_CONDREP: // Conditional Report
                    vJoy.FFB_EFF_COND conditionalReport = new vJoy.FFB_EFF_COND();
                    if (this.joystick.Ffb_h_Eff_Cond(data, ref conditionalReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_PRIDREP: // Periodic Report
                    vJoy.FFB_EFF_PERIOD periodicReport = new vJoy.FFB_EFF_PERIOD();
                    if (this.joystick.Ffb_h_Eff_Period(data, ref periodicReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_CONSTREP: // Constant Force Report
                    vJoy.FFB_EFF_CONSTANT constantForceReport = new vJoy.FFB_EFF_CONSTANT();
                    if (this.joystick.Ffb_h_Eff_Constant(data, ref constantForceReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_RAMPREP: // Ramp Force Report
                    vJoy.FFB_EFF_RAMP rampForceReport = new vJoy.FFB_EFF_RAMP();
                    if (this.joystick.Ffb_h_Eff_Ramp(data, ref rampForceReport) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_EFOPREP: // Effect Operation Report
                    vJoy.FFB_EFF_OP op = new vJoy.FFB_EFF_OP();
                    if (this.joystick.Ffb_h_EffOp(data, ref op) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_CTRLREP: // Device Control Report
                    FFB_CTRL control = new FFB_CTRL();
                    if (this.joystick.Ffb_h_DevCtrl(data, ref control) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
                case FFBPType.PT_GAINREP: // Device Gain Report
                    byte gain = 0;
                    if (this.joystick.Ffb_h_DevGain(data, ref gain) == ERROR_SUCCESS) {
                        // TODO
                    }
                    break;
            }

            callback(args);
        }
    }
}
