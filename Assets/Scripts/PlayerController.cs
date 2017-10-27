using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 3f;
    [SerializeField]
    private float thrusterForce = 1000f;
    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;
    [SerializeField]
    private LayerMask environmentMask;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [Header("Spring settings")]
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    private ConfigurableJoint joint;
    private PlayerMotor motor;
    private Animator animator;

    private void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        motor = GetComponent<PlayerMotor>();
        animator = GetComponent<Animator>();
        SetJointSettings(jointSpring);
    }

    private void Update()
    {
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        } else
        {
            joint.targetPosition = Vector3.zero;
        }

        float _xMov = Input.GetAxis("Horizontal");
        float _zMov = Input.GetAxis("Vertical");
        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;
        motor.Move(_velocity);

        animator.SetFloat("ForwardVelocity", _zMov);

        float _yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;
        motor.Rotate(_rotation);

        float _xRot = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRot * lookSensitivity;
        motor.RotateCamera(_cameraRotationX);

        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;
            if (thrusterFuelAmount > 0.01f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }
        } else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;
            SetJointSettings(jointSpring);
        }
        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);
        motor.ApplyThruster(_thrusterForce);
    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive
        {
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce
        };
    }
}