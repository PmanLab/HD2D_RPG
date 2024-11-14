﻿using UnityEngine;
using UniRx;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //=== 格納用インスタンス ===
    private PlayerInput playerInput;
    private PlayerMove playerMove;

    // PlayerInputのアクションマッピング
    private InputAction moveForwardAction;
    private InputAction moveBackwardAction;
    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction attackAction;

    //=== 変数宣言 ===
    private ReactiveProperty<Vector3> moveInput = new ReactiveProperty<Vector3>();  // Vector2からVector3に変更
    private ReactiveProperty<bool> isAttacking = new ReactiveProperty<bool>(false);

    /// <summary>
    /// 第一初期化メソッド
    /// </summary>
    private void Start()
    {
        // PlayerInputの設定
        playerInput = GetComponent<PlayerInput>();

        // Actionの取得
        var playerControls = playerInput.actions;
        moveForwardAction = playerControls["MoveForward"];
        moveBackwardAction = playerControls["MoveBackward"];
        moveLeftAction = playerControls["MoveLeft"];
        moveRightAction = playerControls["MoveRight"];
        attackAction = playerControls["Attack"];

        playerMove = GetComponent<PlayerMove>();

        // 入力をReactivePropertyにバインド
        moveForwardAction.performed += ctx => UpdateMoveInput();
        moveBackwardAction.performed += ctx => UpdateMoveInput();
        moveLeftAction.performed += ctx => UpdateMoveInput();
        moveRightAction.performed += ctx => UpdateMoveInput();

        moveForwardAction.canceled += ctx => UpdateMoveInput();
        moveBackwardAction.canceled += ctx => UpdateMoveInput();
        moveLeftAction.canceled += ctx => UpdateMoveInput();
        moveRightAction.canceled += ctx => UpdateMoveInput();

        attackAction.performed += _ => isAttacking.Value = true;

        // プレイヤーの状態に応じた動作
        isAttacking.Subscribe(attacking =>
        {
            if (attacking)
            {
                // 攻撃処理
                playerMove.Attack();
            }
        });
    }

    private void UpdateMoveInput()
    {
        // 各キー入力に基づいて移動方向を決定
        float x = 0f;
        float z = 0f;

        // Wキー (前進)
        if (moveForwardAction.ReadValue<float>() > 0f) z = 1f;
        // Sキー (後退)
        if (moveBackwardAction.ReadValue<float>() > 0f) z = -1f;
        // Aキー (左移動)
        if (moveLeftAction.ReadValue<float>() > 0f) x = -1f;
        // Dキー (右移動)
        if (moveRightAction.ReadValue<float>() > 0f) x = 1f;

        // 斜め移動を考慮するため、ベクトルを正規化
        Vector3 direction = new Vector3(x, 0f, z);

        // ベクトルの大きさを1にして、斜め移動でも一定速度にする
        if (direction.magnitude > 1f){ direction.Normalize(); }

        // 移動方向をVector3として設定
        moveInput.Value = direction;  // 3D移動に合わせたVector3
    }

    private void FixedUpdate()
    {
        // 移動の処理
        playerMove.Move(moveInput.Value);
    }

    /// <summary>
    /// OnDestroyメソッド 
    /// </summary>
    private void OnDestroy()
    {
        // ReactivePropertyを解放
        moveInput.Dispose();
        isAttacking.Dispose();
    }
}