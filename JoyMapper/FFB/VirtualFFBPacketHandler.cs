using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper {
    public class VirtualFFBPacketHandler {
        private vJoy joystick;

        private const uint ERROR_SUCCESS = 0x0;
        private const uint ERROR_INVALID_PARAMETER = 0x57;
        private const uint ERROR_INVALID_DATA = 0xD;

        public VirtualFFBPacketHandler(vJoy joystick) {
            this.joystick = joystick;
        }

        public ReVirtualController.vJoy.vJoyFFBPacket ProcessFFBPacket(IntPtr FfbPacket, object userData) {
            ReVirtualController.vJoy.vJoyFFBPacket vJoyFFBPacket = new ReVirtualController.vJoy.vJoyFFBPacket();
            if (this.joystick.Ffb_h_Type(FfbPacket, ref vJoyFFBPacket.FFBPType) == 0u) {
                this.joystick.Ffb_h_EffNew(FfbPacket, ref vJoyFFBPacket.FFBENextType);
                switch (vJoyFFBPacket.FFBPType) {
                    case FFBPType.PT_EFFREP:
                        if (this.joystick.Ffb_h_Eff_Report(FfbPacket, ref vJoyFFBPacket.FFB_EFF_REPORT) == 0u) {
                            Console.WriteLine("Effect Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_REPORT.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_ENVREP:
                        if (this.joystick.Ffb_h_Eff_Envlp(FfbPacket, ref vJoyFFBPacket.FFB_EFF_ENVLP) == 0u) {
                            Console.WriteLine("Envelope Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_ENVLP.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_CONDREP: // !!
                        if (this.joystick.Ffb_h_Eff_Cond(FfbPacket, ref vJoyFFBPacket.FFB_EFF_COND) == 0u) {
                            Console.WriteLine("Condition Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_COND.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_PRIDREP:
                        if (this.joystick.Ffb_h_Eff_Period(FfbPacket, ref vJoyFFBPacket.FFB_EFF_PERIOD) == 0u) {
                            Console.WriteLine("Periodic Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_PERIOD.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_CONSTREP:
                        if (this.joystick.Ffb_h_Eff_Constant(FfbPacket, ref vJoyFFBPacket.FFB_EFF_CONSTANT) == 0u) {
                            Console.WriteLine("Constant Force Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_CONSTANT.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_RAMPREP:
                        if (this.joystick.Ffb_h_Eff_Ramp(FfbPacket, ref vJoyFFBPacket.FFB_EFF_RAMP) == 0u) {
                            Console.WriteLine("Ramp Force Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_RAMP.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_SMPLREP:
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        Console.WriteLine("Download Force Sample");
                        break;
                    case FFBPType.PT_EFOPREP:
                        if (this.joystick.Ffb_h_EffOp(FfbPacket, ref vJoyFFBPacket.FFB_EFF_OP) == 0u) {
                            Console.WriteLine("Effect Operation Report");
                            vJoyFFBPacket.BlockIndex = (int)vJoyFFBPacket.FFB_EFF_OP.EffectBlockIndex;
                        }
                        break;
                    case FFBPType.PT_BLKFRREP:
                        Console.WriteLine("PID Block Free Report");
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        break;
                    case FFBPType.PT_CTRLREP:
                        Console.WriteLine("PID Device Contro");
                        this.joystick.Ffb_h_DevCtrl(FfbPacket, ref vJoyFFBPacket.FFB_CTRL);
                        break;
                    case FFBPType.PT_GAINREP:
                        Console.WriteLine("Device Gain Report");
                        this.joystick.Ffb_h_DevGain(FfbPacket, ref vJoyFFBPacket.Gain);
                        break;
                    case FFBPType.PT_SETCREP:
                        Console.WriteLine("Set Custom Force Report");
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        break;
                    case FFBPType.PT_NEWEFREP: // !!
                        Console.WriteLine("Create New Effect Report");
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        break;
                    case FFBPType.PT_BLKLDREP:
                        Console.WriteLine("Block Load Report");
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        break;
                    case FFBPType.PT_POOLREP:
                        Console.WriteLine("PID Pool Report");
                        this.joystick.Ffb_h_EBI(FfbPacket, ref vJoyFFBPacket.BlockIndex);
                        break;
                }
                //obj = this.FfbLock;
                //lock (obj) {
                //	this.FFBPackets.Enqueue(vJoyFFBPacket);
                //}
                //this.NewFfbItemEvent.Set();
                return vJoyFFBPacket;
            }
            return null;
            /*FFBEventArgs args = new FFBEventArgs();
            FFBPType packetType = new FFBPType();
            this.joystick.Ffb_h_Type(data, ref packetType);

            // extract FFB data from packet based on packet type
            switch (packetType) {
                case FFBPType.PT_EFFREP: // Effect Report
                    vJoy.FFB_EFF_REPORT effectReport = new vJoy.FFB_EFF_REPORT();
                    if (this.joystick.Ffb_h_Eff_Report(data, ref effectReport) == ERROR_SUCCESS) {
                        Console.WriteLine($"EffRep {effectReport.EffectType}");
                        //args.EffectGuid = VirtualController.virtualEffectGuidMap[effectReport.EffectType];
                        //args.Parameters.
                    }
                    break;
                case FFBPType.PT_ENVREP: // Envelope Report
                    vJoy.FFB_EFF_ENVLP envelopeReport = new vJoy.FFB_EFF_ENVLP();
                    if (this.joystick.Ffb_h_Eff_Envlp(data, ref envelopeReport) == ERROR_SUCCESS) {
                        Console.WriteLine($"EnvRep");
                    }
                    break;
                case FFBPType.PT_CONDREP: // Conditional Report
                    vJoy.FFB_EFF_COND conditionalReport = new vJoy.FFB_EFF_COND();
                    if (this.joystick.Ffb_h_Eff_Cond(data, ref conditionalReport) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"CondRep");
                    }
                    break;
                case FFBPType.PT_PRIDREP: // Periodic Report
                    vJoy.FFB_EFF_PERIOD periodicReport = new vJoy.FFB_EFF_PERIOD();
                    if (this.joystick.Ffb_h_Eff_Period(data, ref periodicReport) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"PeriodRep");
                    }
                    break;
                case FFBPType.PT_CONSTREP: // Constant Force Report
                    vJoy.FFB_EFF_CONSTANT constantForceReport = new vJoy.FFB_EFF_CONSTANT();
                    if (this.joystick.Ffb_h_Eff_Constant(data, ref constantForceReport) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"ConstForce");
                        //args.EffectGuid = VirtualController.virtualEffectGuidMap[FFBEType.ET_CONST];
                    }
                    break;
                case FFBPType.PT_RAMPREP: // Ramp Force Report
                    vJoy.FFB_EFF_RAMP rampForceReport = new vJoy.FFB_EFF_RAMP();
                    if (this.joystick.Ffb_h_Eff_Ramp(data, ref rampForceReport) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"RampForce");
                        //args.EffectGuid = VirtualController.virtualEffectGuidMap[FFBEType.ET_RAMP];
                    }
                    break;
                case FFBPType.PT_EFOPREP: // Effect Operation Report
                    vJoy.FFB_EFF_OP op = new vJoy.FFB_EFF_OP();
                    if (this.joystick.Ffb_h_EffOp(data, ref op) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"EffectOp: {op}");

                    }
                    break;
                case FFBPType.PT_CTRLREP: // Device Control Report
                    FFB_CTRL control = new FFB_CTRL();
                    if (this.joystick.Ffb_h_DevCtrl(data, ref control) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"Device control: {control}");
                    }
                    break;
                case FFBPType.PT_GAINREP: // Device Gain Report
                    byte gain = 0;
                    if (this.joystick.Ffb_h_DevGain(data, ref gain) == ERROR_SUCCESS) {
                        // TODO
                        Console.WriteLine($"Set global device gain {gain}");
                    }
                    break;
            }

            callback(args);*/
        }
    }
}
