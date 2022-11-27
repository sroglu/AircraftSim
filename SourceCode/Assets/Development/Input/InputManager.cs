using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // For other class to access this singleton
    static InputManager instance;
    public static InputManager Instance { 
        get { return instance; } 
        private set { 
            if(instance == null)
                instance = value;
            else
                Destroy(value);
        } 
    }

    Vector2 stick;
    float rudder;
    float elevator;
    float aileron;
    float throttle;
    float leftBrake;
    float rightBrake;

    private InputActions aircraftInputs;
    public InputActions AircraftInputs { get { return aircraftInputs; } }

    private ManagementActions managerInputs;
    public ManagementActions ManagerInputs { get { return managerInputs; } }

    public SimObjInputData AircraftInput { get { return aircraftInput; } }

    private SimObjInputData aircraftInput;
    public SimHotasInputData HotasInputData { get { return hotasInputData; } }

    private SimHotasInputData hotasInputData;


    public struct SimHotasInputData
    {
        public Vector2 cursorMovement;
        public Vector2 DMS_TMS_Data;
    }

    // Start is called before the first frame update
    void Start()
    {
        aircraftInputs.AircraftInputs_Development.ThrottleUpdate.performed += ThrottleUpdate;
        aircraftInputs.AircraftInputs_Development.ThrottleUpdate.canceled += ResetThrottle;
        aircraftInputs.AircraftInputs_Development.StickUpdate.performed += StickUpdate;
        aircraftInputs.AircraftInputs_Development.StickUpdate.canceled += ResetStick;
        aircraftInputs.AircraftInputs_Development.RudderUpdate.performed += RudderUpdate;
        aircraftInputs.AircraftInputs_Development.RudderUpdate.canceled += ResetRudder;
        aircraftInputs.AircraftInputs_Development.ResetPress.performed += ResetPress;

        aircraftInputs.AircraftInputs_Development.CursorMovement.performed += CursorMovement;
        aircraftInputs.AircraftInputs_Development.DMS_TMS_Movement.performed += DMS_TMS_Movement;



        aircraftInputs.AircraftInputs_Development.UseChaff.started += (InputAction.CallbackContext obj) =>UseChaff(true);
        aircraftInputs.AircraftInputs_Development.UseChaff.canceled += (InputAction.CallbackContext obj) =>UseChaff(false);
        aircraftInputs.AircraftInputs_Development.UseFlare.started += (InputAction.CallbackContext obj) => UseFlare(true);
        aircraftInputs.AircraftInputs_Development.UseFlare.canceled += (InputAction.CallbackContext obj) => UseFlare(false);
        aircraftInputs.AircraftInputs_Development.Fire.started += (InputAction.CallbackContext obj) => UseMissle(true);
        aircraftInputs.AircraftInputs_Development.Fire.canceled += (InputAction.CallbackContext obj) => UseMissle(false);
        aircraftInputs.AircraftInputs_Development.Fire.started += (InputAction.CallbackContext obj) => UseJammer(true);
        aircraftInputs.AircraftInputs_Development.Fire.canceled += (InputAction.CallbackContext obj) => UseJammer(false);


        aircraftInput = new SimObjInputData();
    }
    Vector2 last_DMS_TMS = Vector2.zero;
    private void FixedUpdate()
    {
        hotasInputData.DMS_TMS_Data = Vector2.zero;
    }

    private void CursorMovement(InputAction.CallbackContext obj)
    {
        hotasInputData.cursorMovement = obj.ReadValue<Vector2>();
    }
    private void DMS_TMS_Movement(InputAction.CallbackContext obj)
    {
        if (obj.action.WasPressedThisFrame())
            last_DMS_TMS = obj.ReadValue<Vector2>();
        else
        {
            hotasInputData.DMS_TMS_Data = last_DMS_TMS;
            last_DMS_TMS = Vector2.zero;
        }
    }

    private void UseJammer(bool isUsing = false)
    {
        aircraftInput.jammerTrigger = (uint)(isUsing ? 1 : 0);
    }

    private void UseMissle(bool isUsing = false)
    {
        aircraftInput.missileTrigger = (uint)(isUsing ? 1 : 0);
    }

    private void UseFlare(bool isUsing = false)
    {
        aircraftInput.flareTrigger = (uint)(isUsing ? 1 : 0);
    }

    private void UseChaff(bool isUsing=false)
    {
        aircraftInput.chaffTrigger = (uint)(isUsing ? 1 : 0);
    }


    public float Rudder()
    {
        return rudder;
    }

    public float Elevator()
    {
        return elevator;
    }

    public float Aileron()
    {
        return aileron;
    }

    public float Throttle()
    {
        return throttle;
    }

    public void StickUpdate(InputAction.CallbackContext context)
    {
        stick = context.ReadValue<Vector2>();
        aileron = stick.x;
        elevator = stick.y;

        aircraftInput.jostickX = aileron;
        aircraftInput.jostickY = elevator;
    }
    public void ResetStick(InputAction.CallbackContext context)
    {
        aileron = 0;
        elevator = 0;

        aircraftInput.jostickX = aileron;
        aircraftInput.jostickY = elevator;
    }

    public void ThrottleUpdate(InputAction.CallbackContext context)
    {
        throttle = context.ReadValue<float>();

        aircraftInput.throttle = throttle;
    }

    public void ResetThrottle(InputAction.CallbackContext context)
    {
        throttle = 0;
        aircraftInput.throttle = throttle;
    }

    public void RudderUpdate(InputAction.CallbackContext context)
    {
        rudder = context.ReadValue<float>();

        aircraftInput.jostickZ = rudder;
    }
    public void ResetRudder(InputAction.CallbackContext context)
    {
        rudder = 0;

        aircraftInput.jostickZ = rudder;
    }
    public void LeftBrakeUpdate(InputAction.CallbackContext context)
    {
        leftBrake = context.ReadValue<float>();
    }

    public void RightBrakeUpdate(InputAction.CallbackContext context)
    {
        rightBrake = context.ReadValue<float>();
    }

    public void ResetPress(InputAction.CallbackContext context)
    {
        //xrHeight.RecenterCam();
    }


    public Action SetPlaneOnGround, SetPlaneInAir;


    private void Awake()
    {
        Instance = this;
        aircraftInputs = new InputActions();
        managerInputs = new ManagementActions();

        managerInputs.Keyboard.SetPlaneOnGround.performed+= (val)=>SetPlaneOnGround.Invoke();
        managerInputs.Keyboard.SetPlaneInAir.performed+= (val) => SetPlaneInAir.Invoke();
    }

    private void OnEnable()
    {
        aircraftInputs.Enable();
        managerInputs.Enable();
    }

    private void OnDisable()
    {
        aircraftInputs.Disable();
        managerInputs.Disable();
    }
}
