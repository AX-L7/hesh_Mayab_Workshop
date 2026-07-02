using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;

    InputActionMap playerMap;
    InputAction moveAction;
    InputAction lookAction;
    InputAction fireAction;
    InputAction reloadAction;
    InputAction dashAction;
    InputAction grenadeAction;
    InputAction meleeAction;
    InputAction sprintAction;
    InputAction weaponScrollAction;
    InputAction weaponNextAction;
    InputAction pauseAction;
    InputAction interactAction;

    public Vector2 Move => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 MousePosition => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool Fire => fireAction?.IsPressed() ?? false;
    public bool FireDown => fireAction?.WasPressedThisFrame() ?? false;
    public bool Sprint => sprintAction?.IsPressed() ?? false;
    public bool DashPressed => dashAction?.WasPressedThisFrame() ?? false;
    public bool ReloadPressed => reloadAction?.WasPressedThisFrame() ?? false;
    public bool GrenadePressed => grenadeAction?.WasPressedThisFrame() ?? false;
    public bool MeleePressed => meleeAction?.WasPressedThisFrame() ?? false;
    public bool PausePressed => pauseAction?.WasPressedThisFrame() ?? false;
    public bool InteractPressed => interactAction?.WasPressedThisFrame() ?? false;
    public bool WeaponNextPressed => weaponNextAction?.WasPressedThisFrame() ?? false;
    public float WeaponScroll => weaponScrollAction?.ReadValue<float>() ?? 0f;

    void Awake()
    {
        playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        fireAction = playerMap.FindAction("Fire");
        reloadAction = playerMap.FindAction("Reload");
        dashAction = playerMap.FindAction("Dash");
        grenadeAction = playerMap.FindAction("Grenade");
        meleeAction = playerMap.FindAction("Melee");
        sprintAction = playerMap.FindAction("Sprint");
        weaponScrollAction = playerMap.FindAction("WeaponScroll");
        weaponNextAction = playerMap.FindAction("WeaponNext");
        pauseAction = playerMap.FindAction("Pause");
        interactAction = playerMap.FindAction("Interact");
    }

    void OnEnable() => playerMap?.Enable();
    void OnDisable() => playerMap?.Disable();
}
