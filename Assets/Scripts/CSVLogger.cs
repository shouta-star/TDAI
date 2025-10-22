using System.IO;
using System.Globalization;
using UnityEngine;

/// <summary>
/// CSV ログ出力ユーティリティ。
/// Target = 最終目的地（ゴール）
/// Current = 現在フレームの位置
/// Next = 次フレームで実際に進む座標（nextPosition）
/// 出力列:
/// Frame,Time,Object,Type,State,Action,Target,
/// TargetX,TargetY,TargetZ,CurrentX,CurrentY,CurrentZ,
/// NextX,NextY,NextZ,AIType,CPU(ms)
///
/// ・同一CSVに「処理時間」を出したい場合は、Log(... cpuMs: xxx) の形で呼び出す。
/// ・cpuMs を渡さない既存呼び出しは空欄として出力される。
/// ・ヘッダー未作成時は自動で「,CPU(ms)」付きヘッダーを作成。
/// ・既存CSVにCPU列が無い場合は、最初の追記時にヘッダーへ自動追記。
/// </summary>
public static class CSVLogger
{
    private static string logFilePath;
    private static bool headerWritten = false;

    //===========================
    // 互換オーバーロード（AIType省略 & CPU列なし）
    //===========================
    public static void Log(
        string type, string name, string state, string action,
        string target, Vector3 targetPos, Vector3 currentPos, Vector3 nextPos
    )
    {
        Log(type, name, state, action, target, targetPos, currentPos, nextPos, "N/A", null);
    }

    //===========================
    // メイン：AIType と CPU(ms) を含む版（cpuMs は任意）
    //===========================
    public static void Log(
        string type, string name, string state, string action,
        string target, Vector3 targetPos, Vector3 currentPos, Vector3 nextPos,
        string aiType, float? cpuMs = null
    )
    {
        EnsureFileInitialized();
        EnsureHeaderContainsCpuColumn();

        var ci = CultureInfo.InvariantCulture;

        // 末尾CPU列（空欄許容）
        string cpuCol = cpuMs.HasValue ? cpuMs.Value.ToString("F3", ci) : "";

        // 1行生成
        string line =
            $"{Time.frameCount}," +
            $"{Time.time.ToString("F2", ci)}," +
            $"{name}," +
            $"{type}," +
            $"{state}," +
            $"{action}," +
            $"{target}," +
            $"{targetPos.x.ToString("F2", ci)}," +
            $"{targetPos.y.ToString("F2", ci)}," +
            $"{targetPos.z.ToString("F2", ci)}," +
            $"{currentPos.x.ToString("F2", ci)}," +
            $"{currentPos.y.ToString("F2", ci)}," +
            $"{currentPos.z.ToString("F2", ci)}," +
            $"{nextPos.x.ToString("F2", ci)}," +
            $"{nextPos.y.ToString("F2", ci)}," +
            $"{nextPos.z.ToString("F2", ci)}," +
            $"{aiType}," +
            $"{cpuCol}";

        SafeAppendAllText(logFilePath, line + "\n");
    }

    //===========================
    // 初期化：出力先ファイルを決定
    //===========================
    private static void EnsureFileInitialized()
    {
        if (logFilePath != null) return;

        const string kCustomRoot = @"D:\AloneGitHub\TDAI\DebugLogs";
        string folder = kCustomRoot;

        try
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                $"[CSVLogger] 指定フォルダ作成に失敗: {folder}\n{e.Message}\n" +
                $" → persistentDataPath にフォールバックします。"
            );
            folder = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(folder);
        }

        string fileName = $"AI_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        logFilePath = Path.Combine(folder, fileName);
    }

    //===========================
    // ヘッダー生成 or 既存CSVのヘッダーを拡張
    //===========================
    private static void EnsureHeaderContainsCpuColumn()
    {
        // 未書き込みなら新規ヘッダーを出す（CPU列込み）
        if (!headerWritten || !File.Exists(logFilePath) || new FileInfo(logFilePath).Length == 0)
        {
            string header =
                "Frame,Time,Object,Type,State,Action,Target," +
                "TargetX,TargetY,TargetZ," +
                "CurrentX,CurrentY,CurrentZ," +
                "NextX,NextY,NextZ,AIType,CPU(ms)";
            SafeWriteAllText(logFilePath, header + "\n");
            headerWritten = true;
            return;
        }

        // 既存ファイルの先頭行を見て、CPU列が無ければ追記
        try
        {
            string all = File.ReadAllText(logFilePath);
            if (string.IsNullOrEmpty(all))
            {
                string header =
                    "Frame,Time,Object,Type,State,Action,Target," +
                    "TargetX,TargetY,TargetZ," +
                    "CurrentX,CurrentY,CurrentZ," +
                    "NextX,NextY,NextZ,AIType,CPU(ms)";
                SafeWriteAllText(logFilePath, header + "\n");
                headerWritten = true;
                return;
            }

            int nl = all.IndexOf('\n');
            if (nl < 0) // 1行しかないなど異常時は上書き
            {
                string header =
                    "Frame,Time,Object,Type,State,Action,Target," +
                    "TargetX,TargetY,TargetZ," +
                    "CurrentX,CurrentY,CurrentZ," +
                    "NextX,NextY,NextZ,AIType,CPU(ms)";
                SafeWriteAllText(logFilePath, header + "\n");
                headerWritten = true;
                return;
            }

            string first = all.Substring(0, nl);
            headerWritten = true; // 既に何らかのヘッダーはある

            if (!first.Contains("CPU(ms)"))
            {
                string newAll = first + ",CPU(ms)" + all.Substring(nl);
                SafeWriteAllText(logFilePath, newAll);
                Debug.Log("[CSVLogger] 既存CSVのヘッダーに CPU(ms) を追加しました。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[CSVLogger] ヘッダー確認/更新に失敗: {e.Message}");
        }
    }

    //===========================
    // IOユーティリティ（簡易リトライ付き）
    //===========================
    private static void SafeWriteAllText(string path, string content, int retry = 2)
    {
        for (int i = 0; i <= retry; i++)
        {
            try
            {
                File.WriteAllText(path, content);
                return;
            }
            catch (IOException)
            {
                if (i == retry) throw;
            }
        }
    }

    private static void SafeAppendAllText(string path, string content, int retry = 2)
    {
        for (int i = 0; i <= retry; i++)
        {
            try
            {
                File.AppendAllText(path, content);
                return;
            }
            catch (IOException)
            {
                if (i == retry) throw;
            }
        }
    }
}


//using System.IO;
//using System.Globalization;
//using UnityEngine;
//using Unity.Profiling;
//using System.Linq;

///// <summary>
///// CSV ログ出力ユーティリティ。
///// Target = 最終目的地（ゴール）
///// Current = 現在フレームの位置
///// Next = 次フレームで実際に進む座標（nextPosition）
///// 出力列:
///// Frame,Time,Object,Type,State,Action,Target,
///// TargetX,TargetY,TargetZ,CurrentX,CurrentY,CurrentZ,
///// NextX,NextY,NextZ,AIType
///// </summary>
//public static class CSVLogger
//{
//    private static string logFilePath;
//    private static bool headerWritten = false;

//    /// <summary>
//    /// ログ出力（AITypeを省略する互換版）
//    /// </summary>
//    public static void Log(
//        string type, string name, string state, string action,
//        string target, Vector3 targetPos, Vector3 currentPos, Vector3 nextPos
//    )
//    {
//        Log(type, name, state, action, target, targetPos, currentPos, nextPos, "N/A");
//    }

//    /// <summary>
//    /// 現行版。Target / Current / Next の3種類の座標を出力。
//    /// </summary>
//    public static void Log(
//        string type, string name, string state, string action,
//        string target, Vector3 targetPos, Vector3 currentPos, Vector3 nextPos, string aiType
//    )
//    {
//        // ファイル初期化
//        if (logFilePath == null)
//        {
//            const string kCustomRoot = @"D:\AloneGitHub\TDAI\DebugLogs";
//            string folder = kCustomRoot;

//            try
//            {
//                if (!Directory.Exists(folder))
//                    Directory.CreateDirectory(folder);
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning(
//                    $"[CSVLogger] 指定フォルダ作成に失敗: {folder}\n{e.Message}\n" +
//                    $" → persistentDataPath にフォールバックします。"
//                );
//                folder = Path.Combine(Application.persistentDataPath, "Logs");
//                Directory.CreateDirectory(folder);
//            }

//            string fileName = $"AI_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
//            logFilePath = Path.Combine(folder, fileName);
//        }

//        // 初回ヘッダー出力
//        if (!headerWritten)
//        {
//            string header =
//                "Frame,Time,Object,Type,State,Action,Target," +
//                "TargetX,TargetY,TargetZ," +
//                "CurrentX,CurrentY,CurrentZ," +
//                "NextX,NextY,NextZ,AIType";
//            File.WriteAllText(logFilePath, header + "\n");
//            headerWritten = true;
//        }

//        var ci = CultureInfo.InvariantCulture;

//        // CSV1行生成
//        string line =
//            $"{Time.frameCount}," +
//            $"{Time.time.ToString("F2", ci)}," +
//            $"{name}," +
//            $"{type}," +
//            $"{state}," +
//            $"{action}," +
//            $"{target}," +
//            $"{targetPos.x.ToString("F2", ci)}," +
//            $"{targetPos.y.ToString("F2", ci)}," +
//            $"{targetPos.z.ToString("F2", ci)}," +
//            $"{currentPos.x.ToString("F2", ci)}," +
//            $"{currentPos.y.ToString("F2", ci)}," +
//            $"{currentPos.z.ToString("F2", ci)}," +
//            $"{nextPos.x.ToString("F2", ci)}," +
//            $"{nextPos.y.ToString("F2", ci)}," +
//            $"{nextPos.z.ToString("F2", ci)}," +
//            $"{aiType}";

//        File.AppendAllText(logFilePath, line + "\n");
//    }

//    // ==========================================================
//    // 経路探索性能ログ（AI_Log_xxx.csv に統合出力）
//    // ==========================================================
//    public static void LogPathPerformance(string aiType, float elapsedMs)
//    {
//        if (logFilePath == null)
//        {
//            Debug.LogError("[CSVLogger] AIログファイルが初期化されていません。");
//            return;
//        }

//        var ci = CultureInfo.InvariantCulture;

//        // まずヘッダー行を取得
//        string[] allLines = File.Exists(logFilePath)
//            ? File.ReadAllLines(logFilePath)
//            : new string[0];

//        // ヘッダーが存在し、"CPU(ms)" が無ければ追加
//        if (allLines.Length > 0 && !allLines[0].Contains("CPU(ms)"))
//        {
//            allLines[0] = allLines[0] + ",CPU(ms)";
//            try
//            {
//                File.WriteAllLines(logFilePath, allLines);
//                Debug.Log("[CSVLogger] CPU(ms) 列をヘッダーに追加しました。");
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning($"[CSVLogger] ヘッダー更新に失敗: {e.Message}");
//            }
//        }

//        // データ行を生成して追記
//        string line =
//            $"{Time.frameCount}," +
//            $"{Time.time.ToString("F2", ci)}," +
//            $"AIManager," +          // Object
//            $"Performance," +        // Type
//            $"Pathfinding," +        // State
//            $"Executed," +           // Action
//            $"{aiType}," +           // Target（アルゴリズム名）
//            $"0,0,0," +              // TargetX,Y,Z（未使用）
//            $"0,0,0," +              // CurrentX,Y,Z（未使用）
//            $"0,0,0," +              // NextX,Y,Z（未使用）
//            $"{aiType}," +           // AIType
//            $"{elapsedMs.ToString("F3", ci)}"; // ★ CPU処理時間（ms）

//        File.AppendAllText(logFilePath, line + "\n");
//    }
//}