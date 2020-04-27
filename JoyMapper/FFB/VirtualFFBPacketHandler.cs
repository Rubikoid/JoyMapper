using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace JoyMapper {
    class VirtualFFBPacketHandler {
        private vJoy joystick;

        private const uint ERROR_SUCCESS = 0x0;
        private const uint ERROR_INVALID_PARAMETER = 0x57;
        private const uint ERROR_INVALID_DATA = 0xD;

        public VirtualFFBPacketHandler(vJoy joystick) {
            this.joystick = joystick;
        }

        public void ProcessFFBPacket(IntPtr data, object userData, Action<FFBEventArgs> callback) {
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
