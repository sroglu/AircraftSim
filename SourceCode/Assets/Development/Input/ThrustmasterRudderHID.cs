using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[InputControlLayout(stateType = typeof(RudderHIDInputReport))]
#if UNITY_EDITOR
[InitializeOnLoad] // Make sure static constructor is called during startup.
#endif

public class ThrustmasterRudderHID : Joystick
{
    static ThrustmasterRudderHID()
    {
        // This is one way to match the Device.
        InputSystem.RegisterLayout<ThrustmasterRudderHID>(matches: new InputDeviceMatcher()
            .WithInterface("HID")
            .WithManufacturer("Thrustmaster")
            .WithProduct("T-Pendular-Rudder"));   // may need to change

        // Alternatively, you can also match by PID and VID, which is generally
        // more reliable for HIDs.
        InputSystem.RegisterLayout<ThrustmasterRudderHID>(
           matches: new InputDeviceMatcher()
               .WithInterface("HID")
               .WithCapability("vendorId", 1103) // Thrustmaster
               .WithCapability("productId", 46735)); // T-Pendular-Rudder
    }

    // In the Player, to trigger the calling of the static constructor,
    // create an empty method annotated with RuntimeInitializeOnLoadMethod.
    [RuntimeInitializeOnLoadMethod]
    static void Init() { }
}


[StructLayout(LayoutKind.Explicit, Size = 4)]
struct RudderHIDInputReport : IInputStateTypeInfo
{
    public FourCC format => new FourCC('H', 'I', 'D');

    // HID input reports can start with an 8-bit report ID. It depends on the device
    // whether this is present or not. On the PS4 DualShock controller, it is
    // present. We don't really need to add the field, but let's do so for the sake of
    // completeness. This can also help with debugging.
    [FieldOffset(0)] public byte reportId;

    [InputControl(name = "RightBrake", layout = "Axis", format = "BYTE",
        parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,invert")]
    [FieldOffset(2)] public byte rightBrake;

    [InputControl(name = "LeftBrake", layout = "Axis", format = "BYTE",
        parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,invert")]
    [FieldOffset(4)] public byte leftBrake;

    [InputControl(name = "Rudder", layout = "Axis", format = "BYTE",
        parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0")]
    [FieldOffset(6)] public byte rudder;

}


/*
    {
    "interface": "HID",
    "type": "",
    "product": "T-Pendular-Rudder",
    "serial": "",
    "version": "272",
    "manufacturer": "Thrustmaster",
    "capabilities": "{\"vendorId\":1103,\"productId\":46735,\"usage\":4,\"usagePage\":1,\"inputReportSize\":31,\"outputReportSize\":0,\"featureReportSize\":64,\"elements\":[],\"collections\":[]}"
}

*/