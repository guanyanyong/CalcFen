using System;
using System.IO;
using Newtonsoft.Json;

namespace CpCodeSelect.Config
{
    public class AppConfig
    {
        public TradingSettings TradingSettings { get; set; }
        
        public static AppConfig Current { get; private set; }

        public FileSettings FileSettings { get; set; }

        static AppConfig()
        {
            LoadConfig();
        }
        
        public static void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            
            if (!File.Exists(configPath))
            {
                // 如果配置文件不存在，创建默认配置
                Current = new AppConfig
                {
                    TradingSettings = new TradingSettings { CycleLength = 6 },
                    FileSettings = new FileSettings()
                };
                SaveConfig();
                return;
            }
            
            try
            {
                string json = File.ReadAllText(configPath);
                Current = JsonConvert.DeserializeObject<AppConfig>(json);
                
                // 确保配置值有效
                if (Current.TradingSettings == null)
                {
                    Current.TradingSettings = new TradingSettings { CycleLength = 6 };
                }
                
                // 验证周期长度是否在合理范围内（必须大于等于3）
                if (Current.TradingSettings.CycleLength < 3)
                {
                    Current.TradingSettings.CycleLength = 6; // 默认为6
                }
            }
            catch (Exception ex)
            {
                // 如果读取配置失败，使用默认值
                Current = new AppConfig
                {
                    TradingSettings = new TradingSettings { CycleLength = 6 }
                };
                Console.WriteLine($"读取配置文件失败: {ex.Message}");
            }
        }
        
        public static void SaveConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            try
            {
                string json = JsonConvert.SerializeObject(Current, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }
    }
    
    public class TradingSettings
    {
        public int CycleLength { get; set; } = 6;
    }
    public class FileSettings
    {
        public string TxffcFilePath { get; set; } 
    }
}