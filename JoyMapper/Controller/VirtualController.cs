using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JoyMapper.Controller.Internal;
using JoyMapper.FFB;
using SharpDX.DirectInput;
using vJoyInterfaceWrap;

namespace JoyMapper.Controller {
    /**
     * A wrapper around the vJoy Joystick class.
     **/
    public class VirtualController : IController {
        public static Dictionary<FFBEType, Guid> virtualEffectGuidMap = new Dictionary<FFBEType, Guid>
        {
            { FFBEType.ET_NONE, EffectGuid.ConstantForce },
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
            { FFBEType.ET_FRCTN, EffectGuid.Friction },
            { FFBEType.ET_CSTM, EffectGuid.CustomForce}
        };
        public static Dictionary<Guid, string> virtualEffectGuidMapToString = new Dictionary<Guid, string>
        {
            { EffectGuid.ConstantForce, "ConstantForce"},
            { EffectGuid.RampForce, "RampForce"},
            { EffectGuid.Square, "Square"},
            { EffectGuid.Sine, "Sine"},
            { EffectGuid.Triangle, "Triangle"},
            { EffectGuid.SawtoothUp, "SawtoothUp"},
            { EffectGuid.SawtoothDown, "SawtoothDown"},
            { EffectGuid.Spring, "Spring"},
            { EffectGuid.Damper, "Damper"},
            { EffectGuid.Inertia, "Inertial"},
            { EffectGuid.Friction, "Friction"},
            { EffectGuid.CustomForce, "CustomForce"}
        };
        public static Dictionary<FFBEType, UInt32> virtualEffectUInt32Map = new Dictionary<FFBEType, UInt32>
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
        private NLog.Logger _logger = null;
        public NLog.Logger logger {
            get {
                if (_logger == null)
                    _logger = NLog.LogManager.GetLogger($"VContr_{ID}");
                return _logger;
            }
        }
        public uint ID { get; private set; }
        public string Name { get { return $"vJoy[{ID}]"; } }
        public bool Connected { get; private set; } = false;

        public List<JoystickCapabilities> Capabilities { get; private set; }
        public int ButtonCount { get; private set; }
        public int ContinuousPOVCount { get; private set; }
        public int DirectionalPOVCount { get; private set; }
        public IList<Guid> SupportedFFBEffects { get; private set; }

        private State internalState { get; set; }
        public IList<IMap> Mappings { get; private set; } = new List<IMap>();
        public int[] FFBAxes { get; private set; } = new int[] { 0, 1 };

        public event VirtualFFBPacketHandler.FFBDataReceiveEventHandler FFBDataReceived;

        public vJoy joystick;

        public VirtualController(uint ID) {
            this.ID = ID;
            this.joystick = new vJoy();
            if (!ControllerCache.VCCheck) {
                if (!joystick.vJoyEnabled()) {
                    logger.Fatal("vJoy driver not enabled: Failed Getting vJoy attributes.");
                    return;
                } else
                    logger.Info("Vendor: {0}; Product :{1}; Version Number:{2}",
                        joystick.GetvJoyManufacturerString(),
                        joystick.GetvJoyProductString(),
                        joystick.GetvJoySerialNumberString()
                    );

                // check driver version against local DLL version
                uint DllVer = 0, DrvVer = 0;
                if (!this.joystick.DriverMatch(ref DllVer, ref DrvVer)) {
                    logger.Error("Version of vJoy Driver ({0:X}) NOT MATCH NOT MATCH vJoy DLL Version ({1:X})", DrvVer, DllVer);
                } else
                    logger.Info("Version of vJoy Driver ({0:X}), vJoy DLL Version ({1:X})", DrvVer, DllVer);
                ControllerCache.VCCheck = true;
            }
        }

        public void Connect() {
            if (!this.Connected) {
                // ensure device is available
                VjdStat status = this.joystick.GetVJDStatus(this.ID);
                switch (status) {
                    case VjdStat.VJD_STAT_OWN:
                        logger.Warn("vJoy Device {0} is already owned by this feeder", this.ID);
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        logger.Info("vJoy Device {0} is free", this.ID);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        logger.Fatal("vJoy Device {0} is already owned by another feeder\nCannot continue", this.ID);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        logger.Fatal("vJoy Device {0} is not installed or disabled\nCannot continue", this.ID);
                        return;
                    default:
                        logger.Fatal("vJoy Device {0} general error\nCannot continue", this.ID);
                        return;
                };

                this.loadCapabilities();

                // now aquire the vJoy device
                if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(this.ID)))) {
                    logger.Fatal("Failed to acquire vJoy device number {0}.", this.ID);
                    return;
                } else
                    logger.Info("Acquired: vJoy device number {0}.", this.ID);

                if (this.SupportedFFBEffects.Count > 0) {
                    VirtualFFBPacketHandler.AddFFBHandler(this.ID, this.FFBDataReceivedHandler);
                    // this.ffbPacketHandler = new VirtualFFBPacketHandler(this.joystick, this);
                    // this.joystick.FfbRegisterGenCB(this.OnVirtualFFBDataReceived, null);
                }

                this.joystick.ResetVJD(this.ID);

                this.Connected = true;
            }
        }

        public void Disconnect() {
            if (this.Connected) {
                if (this.SupportedFFBEffects.Count > 0) {
                    VirtualFFBPacketHandler.RemoveFFBHandler(this.ID);
                    //this.joystick.FfbRegisterGenCB(null, null);
                    //this.ffbPacketHandler = new VirtualFFBPacketHandler(this.joystick);
                }
                this.joystick.ResetVJD(this.ID);
                Thread.Sleep(1000);
                this.joystick.RelinquishVJD(this.ID);
                logger.Info("Dropped: vJoy device number {0}.", this.ID);
                this.Connected = false;
            }
        }

        public void FillExternalInfo(ref State state) { }

        public void UpdateInfo(in State inState) {
            vJoy.JoystickState state = new vJoy.JoystickState();
            state.bDevice = (byte)this.ID;

            state.AxisX = inState.AxisX.getVal();
            state.AxisY = inState.AxisY.getVal();
            state.AxisZ = inState.AxisZ.getVal();

            state.AxisXRot = inState.AxisXR.getVal();
            state.AxisYRot = inState.AxisYR.getVal();
            state.AxisZRot = inState.AxisZR.getVal();

            state.Buttons = 0;
            state.ButtonsEx1 = 0;

            for (int i = 0; i < inState.buttons.Count; i++) {
                if (i < 32) {
                    state.Buttons |= (inState.buttons[i].getVal() ? (uint)1 : 0) << i;
                } else if (i >= 32 && i < 64) {
                    state.ButtonsEx1 |= (inState.buttons[i].getVal() ? (uint)1 : 0) << (i - 32);
                }
            }

            this.joystick.UpdateVJD(this.ID, ref state);
        }

        public void loadCapabilities() {
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

            this.SupportedFFBEffects = new List<Guid>();
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

        public void FFBDataReceivedHandler(VirtualFFBPacket e) {
            this.FFBDataReceived?.Invoke(e);
        }
    }
}
