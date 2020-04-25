﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using vJoyInterfaceWrap;

namespace JoyMapper {
    /**
     * A wrapper around the vJoy Joystick class.
     **/
    class VirtualController : IController {
        public enum JoystickCapabilities {
            AXIS_X,
            AXIS_Y,
            AXIS_Z,
            AXIS_RX,
            AXIS_RY,
            AXIS_RZ,
            POV,
            SLIDER_0,
            SLIDER_1,
            WHEEL
        }

        private static Dictionary<FFBEType, Guid> virtualEffectGuidMap = new Dictionary<FFBEType, Guid>
        {
            { FFBEType.ET_CONST, EffectGuid.ConstantForce },
            { FFBEType.ET_RAMP, EffectGuid.RampForce },
            { FFBEType.ET_SQR, EffectGuid.Square },
            { FFBEType.ET_SINE, EffectGuid.Sine },
            { FFBEType.ET_TRNGL, EffectGuid.Triangle },
            { FFBEType.ET_STUP, EffectGuid.SawtoothUp },
            { FFBEType.ET_STDN, EffectGuid.SawtoothDown },
            { FFBEType.ET_SPRNG, EffectGuid.Spring },
            { FFBEType.ET_DMPR, EffectGuid.Damper },
            { FFBEType.ET_INRT, EffectGuid.Inertia },
            { FFBEType.ET_FRCTN, EffectGuid.Friction }
        };

        private static Dictionary<FFBEType, UInt32> virtualEffectUInt32Map = new Dictionary<FFBEType, UInt32>
        {
            { FFBEType.ET_CONST, 0x26 },
            { FFBEType.ET_RAMP, 0x27},
            { FFBEType.ET_SQR, 0x30 },
            { FFBEType.ET_SINE, 0x31},
            { FFBEType.ET_TRNGL, 0x32 },
            { FFBEType.ET_STUP, 0x33 },
            { FFBEType.ET_STDN, 0x34 },
            { FFBEType.ET_SPRNG, 0x40 },
            { FFBEType.ET_DMPR, 0x41 },
            { FFBEType.ET_INRT, 0x42 },
            { FFBEType.ET_FRCTN, 0x43 }
        };

        public uint ID { get; private set; }
        public bool Connected { get; private set; } = false;
        public List<JoystickCapabilities> Capabilities { get; private set; }
        public int ButtonCount { get; private set; }
        public int ContinuousPOVCount { get; private set; }
        public int DirectionalPOVCount { get; private set; }
        public IList<Guid> SupportedFFBEffects { get; private set; } = new List<Guid>();

        public delegate void FFBDataReceiveEventHandler(object sender, FFBEventArgs e);
        public event FFBDataReceiveEventHandler FFBDataReceived;

        private VirtualFFBPacketHandler ffbPacketHandler;

        public vJoy joystick;

        public VirtualController(uint ID) {
            this.ID = ID;
            this.joystick = new vJoy();
            if (!joystick.vJoyEnabled()) {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            } else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                    joystick.GetvJoyManufacturerString(),
                    joystick.GetvJoyProductString(),
                    joystick.GetvJoySerialNumberString()
                );
        }

        public void Connect() {
            if (!this.Connected) {
                // ensure device is available
                VjdStat status = this.joystick.GetVJDStatus(this.ID);
                switch (status) {
                    case VjdStat.VJD_STAT_OWN:
                        Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", this.ID);
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        Console.WriteLine("vJoy Device {0} is free\n", this.ID);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", this.ID);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", this.ID);
                        return;
                    default:
                        Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", this.ID);
                        return;
                };

                // check driver version against local DLL version
                uint DllVer = 0, DrvVer = 0;
                if (!this.joystick.DriverMatch(ref DllVer, ref DrvVer)) {
                    Console.WriteLine(String.Format("Version of vJoy Driver ({0:X}) does not match vJoy DLL Version ({1:X})!\n", DrvVer, DllVer));
                }

                this.loadCapabilities();

                // now aquire the vJoy device
                if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(this.ID))))
                    Console.WriteLine("Failed to acquire vJoy device number {0}.", this.ID);
                else
                    Console.WriteLine("Acquired: vJoy device number {0}.", this.ID);

                if (this.SupportedFFBEffects.Count > 0) {
                    //this.ffbPacketHandler = new VirtualFFBPacketHandler(this.joystick);
                    //this.joystick.FfbRegisterGenCB(this.OnVirtualFFBDataReceived, null);
                }

                this.joystick.ResetVJD(this.ID);

                this.Connected = true;
            }
        }

        public void Disconnect() {
            if (this.Connected) {
                if (this.SupportedFFBEffects.Count > 0) {
                    //this.joystick.FfbRegisterGenCB(null, null);
                    //this.ffbPacketHandler = new VirtualFFBPacketHandler(this.joystick);
                }
                this.joystick.ResetVJD(this.ID);
                Thread.Sleep(1000);
                this.joystick.RelinquishVJD(this.ID);
                this.Connected = false;
            }
        }
        public void FillInfo(ref State state) { }

        public void UpdateInfo(in State inState) {
            vJoy.JoystickState state = new vJoy.JoystickState();
            state.bDevice = (byte)this.ID;

            //state.AxisX = inState.AxisX.getVal();
            //state.AxisY = inState.AxisY.getVal();
            //state.AxisZ = inState.AxisZ.getVal();

            state.AxisXRot = inState.AxisXR.getVal();
            //state.AxisYRot = inState.AxisYR.getVal();
            //state.AxisZRot = inState.AxisZR.getVal();

            state.Buttons = 0;
            state.ButtonsEx1 = 0;

            for (int i = 0; i < inState.buttons.Length; i++) {
                if (i < 32) {
                    state.Buttons |= (inState.buttons[i] ? (uint)1 : 0) << i;
                } else if (i >= 32 && i < 64) {
                    state.ButtonsEx1 |= (inState.buttons[i] ? (uint)1 : 0) << (i - 32);
                }
            }

            this.joystick.UpdateVJD(this.ID, ref state);
        }

        private void loadCapabilities() {
            this.Capabilities = new List<JoystickCapabilities>();

            // Check which axes are supported
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_X)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_X);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_Y)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_Y);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_Z)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_Z);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RX)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RX);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RY)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RY);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_RZ)) {
                this.Capabilities.Add(JoystickCapabilities.AXIS_RZ);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_POV)) {
                this.Capabilities.Add(JoystickCapabilities.POV);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_SL0)) {
                this.Capabilities.Add(JoystickCapabilities.SLIDER_0);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_SL1)) {
                this.Capabilities.Add(JoystickCapabilities.SLIDER_1);
            }
            if (this.joystick.GetVJDAxisExist(this.ID, HID_USAGES.HID_USAGE_WHL)) {
                this.Capabilities.Add(JoystickCapabilities.WHEEL);
            }

            // get supported FFB effects
            if (this.joystick.IsDeviceFfb(this.ID)) {
                foreach (KeyValuePair<FFBEType, UInt32> entry in VirtualController.virtualEffectUInt32Map) {
                    if (this.joystick.IsDeviceFfbEffect(this.ID, entry.Value)) {
                        this.SupportedFFBEffects.Add(VirtualController.virtualEffectGuidMap[entry.Key]);
                    }
                }
            }

            // Get the number of buttons and POV Hat switches supported by this vJoy device
            this.ButtonCount = this.joystick.GetVJDButtonNumber(this.ID);
            this.ContinuousPOVCount = this.joystick.GetVJDContPovNumber(this.ID);
            this.DirectionalPOVCount = this.joystick.GetVJDDiscPovNumber(this.ID);
        }

        private void OnVirtualFFBDataReceived(IntPtr data, object userData) {
            // handle FFB packets asynchronously
            Task.Run(() => this.ffbPacketHandler.ProcessFFBPacket(data, userData, this.OnFFBDataReceived));
        }

        private void OnFFBDataReceived(FFBEventArgs e) {
            if (this.FFBDataReceived != null) {
                this.FFBDataReceived(this, e);
            }
        }
    }
}