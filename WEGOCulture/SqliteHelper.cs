using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Platform;
using Microsoft.Data.Sqlite;
using WEGOCulture.Models;

namespace WEGOCulture
{
    public static class SqliteHelper
    {
        private static string? _dbPath;
        private static readonly object _lock = new object();

        public static void InitializeDatabase()
        {
            lock (_lock)
            {
                try
                {
                    _dbPath = GetDatabasePath();

                    if (!File.Exists(_dbPath))
                    {
                        CopyDatabaseFromAssets();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"数据库初始化失败: {ex.Message}");
                    throw;
                }
            }
        }

        private static string GetDatabasePath()
        {
            // 在 Android 上，Personal 通常指向 /data/user/0/com.CompanyName.WEGOCulture/files/Documents
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            // 关键修正：确保目标文件夹路径存在（如果不存在则自动创建）
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, "questions.db");
        }

        private static void CopyDatabaseFromAssets()
        {
            try
            {
                // 再次确保父级目录创建（双重保险）
                var parentDir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // 使用 Avalonia 资源 Uri
                var resourceUri = new Uri("avares://WEGOCulture/Assets/questions.db");

                if (AssetLoader.Exists(resourceUri))
                {
                    using var assetStream = AssetLoader.Open(resourceUri);
                    using var fileStream = new FileStream(_dbPath!, FileMode.Create, FileAccess.Write);
                    assetStream.CopyTo(fileStream);
                }
                else
                {
                    throw new FileNotFoundException($"未在资源中找到 questions.db，请检查 avares 路径是否正确: {resourceUri}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"无法复制数据库文件: {ex.Message}", ex);
            }
        }

        private static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_dbPath))
                {
                    InitializeDatabase();
                }
                return $"Data Source={_dbPath};";
            }
        }

        public static List<Question> SearchQuestions(string keyword)
        {
            var list = new List<Question>();

            try
            {
                const string sql = @"
                    SELECT question_type, question, option_a, option_b, option_c, option_d, answer, analysis 
                    FROM questions 
                    WHERE question LIKE @keyword 
                    ORDER BY question;";

                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadQuestion(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"搜索失败: {ex.Message}");
                throw;
            }

            return list;
        }

        public static List<Question> GetAllQuestions()
        {
            var list = new List<Question>();

            try
            {
                const string sql = "SELECT question_type, question, option_a, option_b, option_c, option_d, answer, analysis FROM questions ORDER BY question;";

                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadQuestion(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取所有题目失败: {ex.Message}");
                throw;
            }

            return list;
        }

        // 提取通用的读取方法，减少代码冗余
        private static Question ReadQuestion(SqliteDataReader reader)
        {
            return new Question
            {
                QuestionType = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                QuestionText = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                OptionA = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                OptionB = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                OptionC = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                OptionD = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Answer = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Analysis = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
            };
        }
    }
}