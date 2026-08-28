using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Tetromino Settings")]
    public GameObject[] tetrominoPrefabs;// テトロミノのprefabの配列

    [Header("References")]
    [SerializeField] private GridManager gridManager; // インスペクターで設定推奨

    // Start is called before the first frame update
    void Start()
    {
        // GridManagerの参照が設定されていなければ探す
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManagerが見つかりません！シーンにGridManagerを配置してください。", this);
        }

        SpawnNextTetromino();
    }

    /// <summary>
    /// 次のテトロミノをランダムに生成し、初期化する
    /// </summary>
    public void SpawnNextTetromino()
    {
        // prefabが設定されているかどうか確認
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("テトロミノのprefabがBlockSpawnerに設定されていません！", this);
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManagerへの参照がないためスポーンできません。", this);
            return;
        }

        // --- 生成処理 ---

        // ランダムにインデックスを選択
        int randomIndex = Random.Range(0, tetrominoPrefabs.Length);

        // Spawnerの位置に生成
        // 回転はデフォルト（Quaternion.identity）
        GameObject newTetrominoObj = Instantiate(
            tetrominoPrefabs[randomIndex],
            transform.position,
            Quaternion.identity
        );

        // --- テトロミノの初期化設定 ---

        Tetromino newTetrominoScript = newTetrominoObj.GetComponent<Tetromino>();

        if (newTetrominoScript != null)
        {
            // 1. GridManagerへの参照を直接渡す（FindObjectOfTypeを回避）
            newTetrominoScript.gridManager = gridManager;

            // 2. スポーン時の微調整（オプション）
            // テトロミノの実際の中心（子オブジェクトのBounds中心など）が
            // Spawnerの位置（グリッドの基準線）に合うように調整する例。
            // もしテトロミノのPivotが適切に設定されていれば、この処理は不要
            // ここではColliderの中心を取得してオフセットを計算する簡易的な方法をとる
            //（プレハブにColliderが付いている前提）
            AdjustSpawnPosition(newTetrominoObj, newTetrominoScript);

        }
        else
        {
            Debug.LogError("生成されたテトロミノのprefabにTetrominoスクリプトがアタッチされていません！", newTetrominoObj);
        }
    }

    /// <summary>
    /// テトロミノの見た目の中心がSpawnerの位置（グリッドライン）に合うように位置を微調整する
    /// </summary>
    private void AdjustSpawnPosition(GameObject tetrominoObj, Tetromino script)
    {
        // テトロミノが持つ全ColliderのBoundsを計算して、実際の中心点を求める
        Collider2D[] colliders = tetrominoObj.GetComponentsInChildren<Collider2D>();
        if (colliders.Length == 0) return;

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        // Boundsの中心とSpawnerの位置の差分を計算
        Vector3 centerOffset = bounds.center - tetrominoObj.transform.position;

        // その差分だけテトロミノ全体をずらすことで、Boundsの中心がSpawnerの位置に来るようにする
        // ここでは単純にオフセット分を引いて、親の位置を補正
        tetrominoObj.transform.position -= centerOffset;
    }
}