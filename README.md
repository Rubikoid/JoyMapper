# JoyMapper-NG
Continuation and development of the JoyMapper project to working condition.
# JoyMapper
A vJoy feeder which maps one or more physical devices to a vJoy virtual device. Written in C# for Windows.

The main goal of this project is to support force-feedback of multiple devices by way of the vJoy device.

# FFB
FFB proxy currently working, but not stable enough: sometimes crashes, bad exceptions, poor effects loading happens. 

So, i'm working on stability fixes and new features, and I will be glad of any help and support in the development, especially in matters relating to the work of the FFB and its protocol.

# vJoy
Obviously, you need to download and install the lastest version of vJoy - [vJoy fork by njz3](https://github.com/njz3/vJoy). This fork contains many fixes and features used for ffb proxy.

# HidGuardian
Also, for comfortable using this tool, i recommend install [HidGuardian](https://github.com/ViGEm/HidGuardian) paired with [HidVanguard](https://github.com/dixonte/HidVanguard) - a perfect configuration tool.

So, this is a fix for problem with getting joystick in exclusive mode: sometimes some games very much want to capture all the controllers in the system, and this brokes ffb proxy part.
