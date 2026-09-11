using UnityEngine;
// using UnityEngine.InputSystem; // 【削除】Input Systemの名前空間は不要

// クラス名とファイル名（Tetromino.cs）が一致していること
public class Tetromino : MonoBehaviour
{
    // --- タイマー関連 ---
    private float fallCounter;
    public float fallSpeed = 1.0f;

    // --- 参照関連 ---
    public Vector3 rotationPoint;
    [HideInInspector] public GridManager gridManager;

    private bool isLocked = false;

    // --- 入力検知用のタイマー（連続入力防止用）---
    private float inputHoldTime;
    private float inputHoldDelay = 0.2f; // 初回入力後の待ち時間
    private float inputRepeatRate = 0.05f; // 押しっぱなし時の連続入力間隔(左右移動用)
    private float keyRepeatTimer;
    //ソフトドロップ用の速度調整タイマー
    private float softDropTimer;
    //この値を大きくするほどソフトドロップの連続落下が遅くなります(例 0.1fなら1秒間に10回,0.5fなら2回)
    private float softDropSpeed = 0.15f;

    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            HandleInput();
        }

        if (!IsValidGridPos())
        {
            Debug.Log("ゲームオーバー：初期位置で衝突");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isLocked || gridManager == null) return;

        HandleFalling();
        HandleInput(); // 【追加】古いInputManagerの入力を処理するメソッド
    }

    // --- 【改造】古いInputManagerによる入力処理 ---
    private void HandleInput()
    {
        // --- 横移動（左右・WASD）---
        // GetAxisRawは滑らかさのない(-1, 0, 1)の値を返します
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0)
        {
            // 押しっぱなし処理
            if (Time.time > keyRepeatTimer)
            {
                Move(new Vector3((int)Mathf.Sign(horizontal), 0, 0));

                // 初回は長く、その後は短くタイマーを設定
                if (inputHoldTime < inputHoldDelay)
                {
                    keyRepeatTimer = Time.time + inputHoldDelay;
                    inputHoldTime = inputHoldDelay;
                }
                else
                {
                    keyRepeatTimer = Time.time + inputRepeatRate;
                }
            }
        }
        else
        {
            // 入力が離されたらリセット
            inputHoldTime = 0;
            keyRepeatTimer = 0;
        }

        // --- 縦移動（下・Sキー）---
        // GetKey系はボタンが押されている間trueを返します
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            //TrySoftDrop();
            if (Time.time > softDropTimer)
            {
                TrySoftDrop();
                softDropTimer = Time.time + softDropSpeed;
            }
        }
        else
        {
            softDropTimer = 0;
        }

        // --- 回転（スペース・上キー・Wキー）---
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            RotateTetromino();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HardDropTetromino();
        }

    }

    // --- 落下処理 ---
    private void HandleFalling()
    {
        fallCounter += Time.deltaTime;
        if (fallCounter >= fallSpeed)
        {
            FallOneStep();
        }
    }

    private void FallOneStep()
    {
        fallCounter = 0;
        transform.position += new Vector3(0, -1, 0);
        if (!IsValidGridPos())
        {
            transform.position -= new Vector3(0, -1, 0);
            LockTetromino();
        }
    }

    private void TrySoftDrop()
    {
        transform.position += new Vector3(0, -1, 0);
        if (!IsValidGridPos())
        {
            transform.position -= new Vector3(0, -1, 0);
        }
        else
        {
            fallCounter = 0; // 移動成功したので落下タイマーリセット
        }
    }
    private void HardDropTetromino()
    {
        while(IsValidGridPos())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        //行き過ぎたものを1つ戻す
        transform.position -= new Vector3(0,-1,0);
        LockTetromino();
    }
    

    // --- 移動・回転ロジック ---
    private void Move(Vector3 direction)
    {
        transform.position += direction;
        if (!IsValidGridPos())
        {
            transform.position -= direction;
        }
    }

    private void RotateTetromino()
    {
        Vector3 pivot = transform.TransformPoint(rotationPoint);
        transform.RotateAround(pivot, Vector3.forward, 90);

        if (!IsValidGridPos())
        {
            transform.RotateAround(pivot, Vector3.forward, -90);
        }
    }

   

    // --- 固定処理 ---
    private void LockTetromino()
    {
        if (isLocked) return;
        isLocked = true;
        gridManager.AddTetrominoToGrid(this);
        gridManager.CheckForFullLines();
        this.enabled = false; // Updateを停止

        // 次のテトロミノ生成（BlockSpawnerのメソッド名はそのまま利用）
        BlockSpawner spawner = FindObjectOfType<BlockSpawner>();
        if (spawner != null)
        {
            spawner.SpawnNextTetromino();
        }
    }

    // --- 衝突判定 ---
    private bool IsValidGridPos()
    {
        foreach (Transform child in transform)
        {
            Vector3 blockPos = child.position;
            int x = Mathf.RoundToInt(blockPos.x);
            int y = Mathf.RoundToInt(blockPos.y);

            if (x < 0 || x >= gridManager.width || y < 0)
            {
                return false;
            }
            if (y >= gridManager.height)
            {
                continue;
            }
            if (gridManager.grid[x, y] != null)
            {
                return false;
            }
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(rotationPoint), 0.2f);
    }
}