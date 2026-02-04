using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CpCodeSelect.Core
{
    public class FileWatcher
    {
        private FileSystemWatcher watcher;
        private LotteryProcessor processor;
        private string filePath;
        private System.Threading.Timer debounceTimer; // 防抖定时器
        private bool isMonitoring = false;

        public event Action<string> OnFileChanged;
        public event Action<string> OnError;
        public event Action<string> OnStatusChanged;

        public FileWatcher(LotteryProcessor lotteryProcessor)
        {
            processor = lotteryProcessor;
            // 初始化防抖定时器（但不启动，等待OnChanged调用Change方法启动）
            debounceTimer = new System.Threading.Timer(ProcessFileChange, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        ~FileWatcher()
        {
            // 析构函数中释放资源
            if (debounceTimer != null)
            {
                debounceTimer.Dispose();
            }
        }

        public void StartMonitoring(string fileToWatch)
        {
            if (string.IsNullOrEmpty(fileToWatch) || !File.Exists(fileToWatch))
            {
                OnError?.Invoke($"文件不存在: {fileToWatch}");
                return;
            }

            filePath = fileToWatch;
            
            // 创建文件监控器
            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(filePath);
            watcher.Filter = Path.GetFileName(filePath);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;

            // 注册事件处理器
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;

            // 启动监控
            watcher.EnableRaisingEvents = true;
            isMonitoring = true;
            
            OnStatusChanged?.Invoke($"开始监控文件: {filePath}");
        }

        public void StopMonitoring()
        {
            isMonitoring = false;
            
            if (debounceTimer != null)
            {
                debounceTimer.Dispose();
                debounceTimer = null;
            }

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }
            
            OnStatusChanged?.Invoke("文件监控已停止");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!isMonitoring) return;
            
            // 使用防抖机制，延迟处理文件变化，避免重复触发
            debounceTimer?.Change(1000, Timeout.Infinite); // 1秒防抖
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!isMonitoring) return;
            
            // 如果文件被重命名但仍是我们关注的文件名
            if (e.FullPath == filePath || e.OldFullPath == filePath)
            {
                debounceTimer?.Change(1000, Timeout.Infinite); // 1秒防抖
            }
        }

        private void ProcessFileChange(object state)
        {
            if (!isMonitoring) return;
            
            // 在后台线程上执行文件处理
            Task.Run(() => ProcessFileChanges());
        }

        private void ProcessFileChanges()
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    OnError?.Invoke("监控的文件不存在");
                    return;
                }

                OnStatusChanged?.Invoke($"检测到文件变化，正在处理新数据...");
                
                // 执行增量数据处理
                processor.ExecuteIncrementalData(filePath);

                OnFileChanged?.Invoke($"文件 {filePath} 发生变化，已处理新增数据");
                OnStatusChanged?.Invoke($"文件监控: {filePath} - 成功处理最新数据");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"处理文件变化时发生错误: {ex.Message}");
            }
        }

        public bool IsMonitoring => isMonitoring;
        
        public string MonitoredFilePath => filePath;
    }
}