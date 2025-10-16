using System.IO;
using UnityEngine;

public static class CSVLogger
{
    private static string logFilePath;
    private static bool headerWritten = false;

    // ★ 後方互換オーバーロード（AIType 省略呼び出し用）
    public static void Log(string type, string name, string state, string action,
                           string target, Vector3 targetPos, Vector3 selfPos)
    {
        Log(type, name, state, action, target, targetPos, selfPos, "N/A");
    }

    // 現行版：AIType まで出力
    public static void Log(string type, string name, string state, string action,
                           string target, Vector3 targetPos, Vector3 selfPos, string aiType)
    {
        if (logFilePath == null)
        {
            const string kCustomRoot = @"D:\AloneGitHub\TDAI\DebugLogs";
            string folder = kCustomRoot;

            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CSVLogger] 指定フォルダ作成に失敗: {folder}\n{e.Message}\n" +
                                 $" → persistentDataPath にフォールバックします。");
                folder = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(folder);
            }

            string fileName = $"AI_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            logFilePath = Path.Combine(folder, fileName);
        }

        // 初回のみヘッダ出力
        if (!headerWritten)
        {
            string header = "Frame,Time,Object,Type,State,Action,Target," +
                            "TargetX,TargetY,TargetZ,SelfX,SelfY,SelfZ,AIType";
            File.WriteAllText(logFilePath, header + "\n");
            headerWritten = true;
        }

        string line = $"{Time.frameCount},{Time.time:F2},{name},{type},{state},{action},{target}," +
                      $"{targetPos.x:F2},{targetPos.y:F2},{targetPos.z:F2}," +
                      $"{selfPos.x:F2},{selfPos.y:F2},{selfPos.z:F2},{aiType}";

        File.AppendAllText(logFilePath, line + "\n");
    }
}


//using System.IO;
//using UnityEngine;

//public static class CSVLogger
//{
//    private static string logFilePath;
//    private static bool headerWritten = false;

//    public static void Log(string type, string name, string state, string action,
//                           string target, Vector3 targetPos, Vector3 selfPos)
//    {
//        //if (logFilePath == null)
//        //{
//        //    string folder = Path.Combine(Application.persistentDataPath, "Logs");
//        //    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

//        //    string fileName = $"AI_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
//        //    logFilePath = Path.Combine(folder, fileName);
//        //}
//        // これまでの logFilePath 初期化ブロックを まるっと 置き換え
//        if (logFilePath == null)
//        {
//            // ★ここで出力先を固定★
//            const string kCustomRoot = @"D:\AloneGitHub\TDAI\DebugLogs";

//            string folder = kCustomRoot; // 必要なら Path.Combine(kCustomRoot, "Logs") などサブフォルダ化も可
//            try
//            {
//                if (!Directory.Exists(folder))
//                    Directory.CreateDirectory(folder);
//            }
//            catch (System.Exception e)
//            {
//                // もし作成に失敗したら安全なフォールバックへ
//                Debug.LogWarning($"[CSVLogger] 指定フォルダ作成に失敗: {folder}\n{e.Message}\n" +
//                                 $" → persistentDataPath にフォールバックします。");
//                folder = Path.Combine(Application.persistentDataPath, "Logs");
//                Directory.CreateDirectory(folder);
//            }

//            string fileName = $"AI_Log_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
//            logFilePath = Path.Combine(folder, fileName);
//        }


//        // 初回のみヘッダ出力
//        if (!headerWritten)
//        {
//            string header = "Frame,Time,Object,Type,State,Action,Target," +
//                            "TargetX,TargetY,TargetZ,SelfX,SelfY,SelfZ";
//            File.WriteAllText(logFilePath, header + "\n");
//            headerWritten = true;
//        }

//        string line = $"{Time.frameCount},{Time.time:F2},{name},{type},{state},{action},{target}," +
//                      $"{targetPos.x:F2},{targetPos.y:F2},{targetPos.z:F2}," +
//                      $"{selfPos.x:F2},{selfPos.y:F2},{selfPos.z:F2}";

//        File.AppendAllText(logFilePath, line + "\n");
//    }
//}
