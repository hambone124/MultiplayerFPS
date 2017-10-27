using UnityEngine;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    private RectTransform thrusterFuelFill;
    private PlayerController controller;

    private void SetFuelAmount (float _amount)
    {
        thrusterFuelFill.localScale = new Vector3(1f, _amount, 1f);
    }

    public void SetController (PlayerController _controller)
    {
        controller = _controller;
    }

    private void Update()
    {
        SetFuelAmount(controller.GetThrusterFuelAmount());
    }
}
