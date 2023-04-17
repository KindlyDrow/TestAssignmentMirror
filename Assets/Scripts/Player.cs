using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;

    [SerializeField] private float _dashTime = 1f;
    [SerializeField] private float _dashPrepTime = 1f;
    [SerializeField] private float _dashForce = 10f;

    private bool isDash = false;

    private Rigidbody playerRb;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        GameInput.Instance.OnDashPerfomed += GameInput_OnDashPerfomed;
    }

    private void GameInput_OnDashPerfomed()
    {
        if (!isDash)
        {
            isDash = true;
            StartCoroutine(StartDash(_dashPrepTime, _dashTime, _dashForce));
        }
    }

    private void FixedUpdate()
    {
        if (!isDash) HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero) transform.position += (moveDir * Time.deltaTime * _moveSpeed);
    }

    private IEnumerator StartDash(float dashPrepTime, float dashTime, float dashForce)
    {
        yield return new WaitForSeconds(dashPrepTime / 10);

        Vector3 forceDir = GetMouseVectorNormalized();

        playerRb.AddForce(forceDir * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashTime / 10);

        playerRb.velocity = Vector3.zero;
        isDash = false;
    }

    private Vector3 GetMouseVectorNormalized()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.y));
        mousePosition.y = transform.position.y;

        Vector3 forceDir = (mousePosition - transform.position).normalized;

        return forceDir;
    }
}
