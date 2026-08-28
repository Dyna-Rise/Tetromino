using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    // グリッドのサイズ（幅と高さ）
    public int width = 10;
    public int height = 22; // 通常テトリスは隠れた2行を含め20+2段

    // グリッドの状態を保持する2次元配列（コメントを修正）
    // 各要素にはその位置に存在するブロック（子Transform）が格納される
    public Transform[,] grid;

    // AwakeはStartの前に呼ばれる初期化処理
    void Awake()
    {
        // グリッド配列を初期化（指定された幅と高さでメモリを確保）
        // C#の多次元配列[,]は、[x, y]でアクセスします
        grid = new Transform[width, height];
    }

    // --- 外部公開メソッド ---

    /// <summary>
    /// テトロミノが着地した際、その構成ブロックをグリッド配列に登録する
    /// </summary>
    public void AddTetrominoToGrid(Tetromino tetromino)
    {
        // テトロミノの子オブジェクト（個々のブロック）を順に処理
        foreach (Transform child in tetromino.transform)
        {
            // ブロックのワールド座標から、整数グリッド座標を算出
            int x = Mathf.RoundToInt(child.position.x);
            int y = Mathf.RoundToInt(child.position.y);

            // グリッドの範囲内か念のためチェック
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                // グリッドの該当位置にブロックのTransform情報を記録
                grid[x, y] = child;
            }
            else
            {
                Debug.LogWarning($"ブロックがグリッド範囲外で固定されようとしました: ({x}, {y})");
            }
        }
    }

    /// <summary>
    /// 揃ったラインがあるかチェックし、あれば削除して上を詰める
    /// </summary>
    public void CheckForFullLines()
    {
        // 下の行から順にチェックしていく
        int y = 0;
        while (y < height)
        {
            if (IsFullLine(y))
            {
                DeleteLine(y);
                // 行を削除したら、その上の全ての行を一段下げる
                MoveAllRowsDown(y + 1);

                // 行を詰めたので、同じ高さ(y)をもう一度チェックするため、
                // yをインクリメントせずに次のループへ（continueでも可）
                // ※これにより、複数行同時に消えた場合も対応できる
            }
            else
            {
                // 揃っていなければ次の行へ
                y++;
            }
        }
    }

    // --- 内部プライベートメソッド ---

    /// <summary>
    /// 指定されたY座標のラインが全て埋まっているかチェック
    /// </summary>
    private bool IsFullLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            // 一つでもnull（空き）があれば false
            if (grid[x, y] == null)
            {
                return false;
            }
        }
        // 全て埋まっていたら true
        return true;
    }

    /// <summary>
    /// 指定されたY座標のライン上のブロックを削除する
    /// </summary>
    private void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                // 【重要】シーン上のゲームオブジェクト自体を削除
                Destroy(grid[x, y].gameObject);
                // グリッド配列上の参照をnullにする
                grid[x, y] = null;
            }
        }
    }

    /// <summary>
    /// 指定されたY座標より上の全ての行を一段下げる
    /// </summary>
    /// <param name="startY">下げ始める開始Y座標（削除した行の次）</param>
    private void MoveAllRowsDown(int startY)
    {
        // 削除された行の次から、一番上までループ
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    // 1. 一つ下のグリッドに現在のブロック情報をコピー
                    grid[x, y - 1] = grid[x, y];
                    // 2. 元の位置を空にする
                    grid[x, y] = null;

                    // 3. 実際のGameObjectの位置（Y座標）を一つ下に移動させる
                    // 注意：Transformの参照が生きているので、直接操作可能
                    grid[x, y - 1].position += new Vector3(0, -1, 0);
                }
            }
        }
    }
}