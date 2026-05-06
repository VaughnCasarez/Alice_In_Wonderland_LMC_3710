using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ScalePlayerOnTrigger : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isHeld = false;
    private bool wasTriggerDown = false;
    private bool ownsTriggerAction = false;
    private bool createdTriggerAction = false;

    [Header("References")]
    public Transform xrOrigin;

    [Header("Input")]
    public InputActionProperty triggerAction;
    [Range(0.01f, 1f)] public float triggerPressThreshold = 0.8f;

    [Header("Scaling")]
    public float scaleMultiplier = 1.2f;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("ScalePlayerOnTrigger requires an XRGrabInteractable on the same GameObject.");
        }

        EnsureDefaultTriggerAction();
    }

    private void Reset()
    {
        EnsureDefaultTriggerAction();
    }

    private void OnValidate()
    {
        EnsureDefaultTriggerAction();
    }

    private void EnsureDefaultTriggerAction()
    {
        if (triggerAction.action != null) return;

        // Preset trigger action so it has visible bindings in the Inspector and works at runtime.
        var defaultTriggerAction = new InputAction("Scale Trigger", InputActionType.Value);

        // Generic XR bindings.
        defaultTriggerAction.AddBinding("<XRController>{LeftHand}/trigger");
        defaultTriggerAction.AddBinding("<XRController>{RightHand}/trigger");
        defaultTriggerAction.AddBinding("<XRController>{LeftHand}/triggerPressed");
        defaultTriggerAction.AddBinding("<XRController>{RightHand}/triggerPressed");

        // Oculus Touch fallback bindings.
        defaultTriggerAction.AddBinding("<OculusTouchController>{LeftHand}/trigger");
        defaultTriggerAction.AddBinding("<OculusTouchController>{RightHand}/trigger");

        triggerAction = new InputActionProperty(defaultTriggerAction);
        createdTriggerAction = true;
    }

    private void OnEnable()
    {
        if (grabInteractable == null) return;

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        if (triggerAction.action != null && !triggerAction.action.enabled)
        {
            triggerAction.action.Enable();
            ownsTriggerAction = true;
        }
    }

    private void OnDisable()
    {
        if (grabInteractable == null) return;

        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);

        if (ownsTriggerAction && triggerAction.action != null)
        {
            triggerAction.action.Disable();
            ownsTriggerAction = false;
        }
    }

    private void OnDestroy()
    {
        if (createdTriggerAction && triggerAction.action != null)
        {
            triggerAction.action.Dispose();
            createdTriggerAction = false;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        Debug.Log("Object grabbed");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        Debug.Log("Object released");
    }

    void Update()
    {
        if (!isHeld) return;
        if (triggerAction.action == null) return;

        float triggerValue = triggerAction.action.ReadValue<float>();
        bool isTriggerDown = triggerValue >= triggerPressThreshold;

        if (isTriggerDown && !wasTriggerDown)
        {
            Debug.Log($"Trigger pressed ({triggerValue:0.00})");
            ScalePlayer();
        }

        wasTriggerDown = isTriggerDown;
    }

    void ScalePlayer()
    {
        if (xrOrigin == null)
        {
            Debug.LogWarning("XR Origin not assigned!");
            return;
        }

        xrOrigin.localScale *= scaleMultiplier;
        Debug.Log("Player scaled to: " + xrOrigin.localScale);
    }
}