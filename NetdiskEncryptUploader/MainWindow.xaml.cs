using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using SevenZip;

namespace NetdiskEncryptUploader
{
    public static partial class Varible
    {
        public static Config config = new Config();
    }

    public static partial class Func
    {
        /// <summary>
        /// 计算文件SHA1
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetFileSha1(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            HashAlgorithm hashAlg = SHA1.Create();
            string result = BitConverter.ToString(hashAlg.ComputeHash(fileStream)).Replace("-", "");
            fileStream.Close();
            return result;
        }

        /// <summary>
        /// 获取目录及子目录中的文件
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns></returns>
        public static string[] GetDirFile(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fileInfo = dirInfo.GetFiles();
            List<string> fileList = new List<string>();
            //目录中的文件
            foreach (var file in fileInfo)
            {
                fileList.Add(file.FullName);
            }
            //目录中的子目录
            DirectoryInfo[] subdirInfo = dirInfo.GetDirectories();
            foreach (var subdir in subdirInfo)
            {
                //子目录中的文件
                FileInfo[] subFileInfo = subdir.GetFiles();
                foreach (var file in subFileInfo)
                {
                    fileList.Add(file.FullName);
                }
            }
            return fileList.ToArray();
        }

        /// <summary>
        /// 根据压缩等级数值 返回压缩等级名称
        /// </summary>
        /// <param name="lv">压缩等级(0-5)</param>
        /// <returns></returns>
        public static string GetCompressLvName(int lv)
        {
            switch (Varible.config.compressionLevel)
            {
                case 0:
                    return "None";
                case 1:
                    return "Fast";
                case 2:
                    return "Low";
                case 3:
                    return "Normal";
                case 4:
                    return "High";
                case 5:
                    return "Ultra";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 运行程序
        /// </summary>
        /// <param name="file">程序文件路径</param>
        /// <param name="arg">参数</param>
        /// <param name="isCreateWindow">是否显示窗口</param>
        /// <param name="isShellExecute">是否使用ShellExecute</param>
        public static void Execuate(string file, string arg, bool isCreateWindow, bool isShellExecute)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = file;
                process.StartInfo.Arguments = arg;
                process.StartInfo.CreateNoWindow = isCreateWindow;
                process.StartInfo.UseShellExecute = isShellExecute;
                process.Start();
            }
            catch (Exception e)
            {
                WriteLog(LogLevel.ERROR, "Fail to execute: " + e.Message);
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="lv">日志等级(0-4)</param>
        /// <param name="msg">内容</param>
        public static void WriteLog(int lv, string msg)
        {
            //检测Log开关 在配置中改为留空则为关
            if (Varible.config.logPath == "")
            {
                return;
            }
            //检测要输出的最低Log等级 若lv小于最低要输出的Log等级则退出
            if(lv < Varible.config.minLogLevel)
            {
                return;
            }
            //检测Log文件是否存在 不存在则创建
            if(!File.Exists(Varible.config.logPath))
            {
                FileStream fs = File.Create(Varible.config.logPath);
                fs.Close();
            }
            try
            {
                using (StreamWriter logWriter = new StreamWriter(Varible.config.logPath, true)) //使用append
                {
                    switch (lv)
                    {
                        case 0:
                            logWriter.Write("[DEBUG]");
                            break;
                        case 1:
                            logWriter.Write("[INFO]");
                            break;
                        case 2:
                            logWriter.Write("[WARN]");
                            break;
                        case 3:
                            logWriter.Write("[ERROR]");
                            break;
                        case 4:
                            logWriter.Write("[FATAL]");
                            break;
                    }
                    logWriter.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="lv">日志等级</param>
        /// <param name="msg">内容</param>
        public static void WriteLog(LogLevel lv, string msg)
        {
            WriteLog((int)lv, msg);
        }

    }
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static BackgroundWorker worker = new BackgroundWorker();
        DoWorkEventArgs args = new DoWorkEventArgs(worker);
        bool isRun = false;
        bool isPasswdShow = false;

        public MainWindow()
        {
            //检测有无配置文件 若无则询问是否创建
            if(!File.Exists("config.json"))
            {
                MessageBoxResult result = MessageBox.Show("config.json not found\nCreate now?", "NetdiskEncryptUploader", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    try
                    {
                        FileStream configFileStream = File.Create("config.json");
                        var options = new JsonSerializerOptions { WriteIndented = true };   //对JSON输出进行优质打印
                        string jsonString = JsonSerializer.Serialize(Varible.config, options);  //序列化config生成JSON
                        configFileStream.Write(System.Text.Encoding.UTF8.GetBytes(jsonString));
                        configFileStream.Close();
                        MessageBox.Show("Success\nComplete the config and restart the program", "NetdiskEncryptUploader", MessageBoxButton.OK, MessageBoxImage.Information);
                        Environment.Exit(1);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("[Error] Fail to generate config.txt\n\nInfo:\n" + e.Message, "NetdiskEncryptUploader", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    Environment.Exit(-1);
                }
            }   

            //读取配置文件
            try
            {
                StreamReader configReader = new StreamReader("config.json");
                string jsonString = configReader.ReadToEnd();
                configReader.Close();
                Varible.config = JsonSerializer.Deserialize<Config>(jsonString);    //JSON反序列化 存储到config
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: fail to load config.txt\n\nInfo:\n" + e.Message, "NetdiskEncryptUploader", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }

            InitializeComponent();

            //初始化UI
            rect_IconRun.Visibility = Visibility.Hidden;
            rect_IconComplete.Visibility = Visibility.Hidden;
            rect_IconCancelled.Visibility = Visibility.Hidden;
            rect_IconWarn.Visibility = Visibility.Hidden;
            rect_IconCompleteForClear.Visibility = Visibility.Hidden;
            chkbox_Hash.IsChecked = Varible.config.isHashEnabled ? true : false;
            chkbox_Encrypt.IsChecked = Varible.config.isEncryptEnabled ? true : false;
            slider_CompressLv.Value = (double)Varible.config.compressionLevel;
            UpdateInfo();

            //初始化BackgroundWorker
            worker.DoWork += (s, e) =>
            {
                ProcessItem(tblk_Progress, rect_IconWarn);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                isRun = false;
                rect_IconRun.Visibility = Visibility.Hidden;
                switch(args.Cancel)
                {
                    case true:
                        rect_IconCancelled.Visibility = Visibility.Visible;
                        Func.WriteLog(LogLevel.INFO, "CANCELLED");
                        break;
                    case false:
                        rect_IconComplete.Visibility = Visibility.Visible;
                        Func.WriteLog(LogLevel.INFO, "COMPLETE");
                        break;
                }
            };
            worker.WorkerSupportsCancellation = true;

            Func.WriteLog(LogLevel.DEBUG, "LAUNCH - " + Varible.config.ToString());
        }

        //控件事件
        private void slider_Del_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100) 
            {
                Directory.Delete(Varible.config.outputDir, true);
                Directory.CreateDirectory(Varible.config.outputDir);
                slider_Del.Value = 0;
                rect_IconCompleteForClear.Visibility = Visibility.Visible;
                Func.WriteLog(LogLevel.INFO, "Output cleared");
            }
            else if (e.NewValue != 0)
            {
                rect_IconCompleteForClear.Visibility = Visibility.Hidden;
            }
        }
        private void slider_CompressLv_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Varible.config.compressionLevel = (int)e.NewValue;
            tblk_Info.Text = "Input: " + Varible.config.inputList + "\nOutput: " + Varible.config.outputDir + "\nPassword: ";
            tblk_Info.Text += isPasswdShow ? Varible.config.passwd : "***";
            tblk_Info.Text += "\nCompression Level: " + Varible.config.compressionLevel.ToString() + " / ";
            switch (Varible.config.compressionLevel)
            {
                case 0:
                    tblk_Info.Text += "None";
                    break;
                case 1:
                    tblk_Info.Text += "Fast";
                    break;
                case 2:
                    tblk_Info.Text += "Low";
                    break;
                case 3:
                    tblk_Info.Text += "Normal";
                    break;
                case 4:
                    tblk_Info.Text += "High";
                    break;
                case 5:
                    tblk_Info.Text += "Ultra";
                    break;
            }
        }
        private void chkbox_Hash_Checked(object sender, RoutedEventArgs e)
        {
            Varible.config.isHashEnabled = true;
        }

        private void chkbox_Hash_Unchecked(object sender, RoutedEventArgs e)
        {
            Varible.config.isHashEnabled = false;
        }

        private void chkbox_Encrypt_Checked(object sender, RoutedEventArgs e)
        {
            Varible.config.isEncryptEnabled = true;
        }

        private void chkbox_Encrypt_Unchecked(object sender, RoutedEventArgs e)
        {
            Varible.config.isEncryptEnabled = false;
        }
        private void Expander_HideSuccessSign(object sender, RoutedEventArgs e)
        {
            rect_IconCompleteForClear.Visibility = Visibility.Hidden;
        }
        private void btn_OpenOutput_Click(object sender, RoutedEventArgs e)
        {
            //检测目录是否存在
            if (!Directory.Exists(Varible.config.outputDir))
            {
                MessageBoxResult result = MessageBox.Show(Varible.config.outputDir + " not found\nCreate now?", "NetdiskEncryptUploader", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Directory.CreateDirectory(Varible.config.outputDir);
                        break;
                    case MessageBoxResult.No:
                        return;
                }
            }
            Func.Execuate("explorer.exe", Varible.config.outputDir, true, false);
        }
        private void btn_Upload_Click(object sender, RoutedEventArgs e)
        {
            Func.Execuate(Varible.config.uploadProgram, Varible.config.uploadArg, true, false);
            Func.WriteLog(LogLevel.INFO, "Uploaded");
        }
        private void btn_ViewPasswd_Click(object sender, RoutedEventArgs e)
        {
            switch (isPasswdShow)
            {
                case false:
                    isPasswdShow = true;
                    btn_ViewPasswd.Content = "Hide Password";
                    UpdateInfo();
                    break;
                case true:
                    isPasswdShow = false;
                    btn_ViewPasswd.Content = "Show Password";
                    UpdateInfo();
                    break;
            }
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if(isRun)
            {
                isRun = false;
                worker.CancelAsync();
                return;
            }
            
            //显示运行标志 复位其他标志
            rect_IconRun.Visibility = Visibility.Visible;
            rect_IconComplete.Visibility = Visibility.Hidden;
            rect_IconCancelled.Visibility = Visibility.Hidden;
            rect_IconWarn.Visibility = Visibility.Hidden;
            isRun = true;

            //异步运行
            worker.RunWorkerAsync();
        }

        //统计信息
        static int countTotalItem = 0;
        static int countSuccessItem = 0;
        static int countFailItem = 0;

        static int countTotalFile = 0;
        static int countSkipFile = 0;
        static int countSuccessFile = 0;
        static int countFailFile = 0;

        //目录长度 用于目录类型从完整路径中分离出相对路径
        static int dirLength = 0;

        public void ProcessItem(TextBlock textProgress, Image rectWarn)
        {
            Func.WriteLog(LogLevel.INFO, "START - " + Varible.config.ToString());

            //清空计数
            countTotalItem = 0;
            countSuccessItem = 0;
            countFailItem = 0;
            countTotalFile = 0;
            countSkipFile = 0;
            countSuccessFile = 0;
            countFailFile = 0;

            //复位取消
            args.Cancel = false;

            //检测文件列表是否存在 并打开文件列表
            if(!File.Exists(Varible.config.inputList))
            {
                MessageBox.Show("[Error] Input List not exist", "NetdiskEncryptUploader", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }
            StreamReader listReader = new StreamReader(Varible.config.inputList);

            //Hash检查启用
            //Hash数据存放在Hash文件夹中 每个文件的Hash为"*.*-Hash.txt"
            //在开始前创建Hash文件夹
            if (Varible.config.isHashEnabled)
            {
                if(!Directory.Exists("Hash"))
                {
                    Directory.CreateDirectory("Hash");
                }
            }

            //读入文件列表
            while (!listReader.EndOfStream)
            {
                string path = listReader.ReadLine();

                char type = 'U';    //文件类型
                int result = -2;    //记录处理文件的方法的返回值
                bool resultItem = true; //当前项目是否成功

                //更新统计信息(1)
                UpdateStatistic(textProgress, path);

                //检查是否取消 1.在读入文件列表中的项目时
                if(worker.CancellationPending)
                {
                    args.Cancel = true;
                    return;
                }

                //判断输入路径是文件/目录
                if (File.Exists(path))  //文件
                {
                    type = 'F';
                }
                else if (Directory.Exists(path))    //目录
                {
                    type = 'D';
                }
                else    //不存在
                {
                    //显示警告
                    rectWarn.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        rectWarn.Visibility = Visibility.Visible;
                    }));
                    //增加计数
                    countTotalItem++;
                    countFailItem++;
                    Func.WriteLog(LogLevel.WARN, "Item \"" + path + "\" not found");
                    continue;
                }

                switch (type)
                {
                    case 'D':
                        if (Directory.Exists(path))
                        {
                            string[] files = Func.GetDirFile(path);
                            foreach(var file in files)
                            {
                                //更新统计信息(2)
                                UpdateStatistic(textProgress, file);

                                string _file = file;

                                //检测是否强制使用相对路径
                                if (Varible.config.isForceRelativePath)
                                {
                                    //将路径转换为完整路径再操作
                                    _file = Path.GetFullPath(file);
                                    //获取目录长度 以分离出后面的部分
                                    dirLength = Path.GetFullPath(path).Length;  //目录长度可直接作为SubString的StartIndex 从\的后一个字符开始
                                }

                                //执行
                                result = ProcessSingleFile(_file);
                                //判断是否已取消
                                if (worker.CancellationPending)
                                {
                                    args.Cancel = true;
                                    return;
                                }
                                //增加文件计数 并在失败时更改该项目结果
                                countTotalFile++;
                                switch (result)
                                {
                                    case 0: //错误
                                        countFailFile++;
                                        resultItem = false;    //将项目结果设为false
                                        break;
                                    case 1: //成功
                                        countSuccessFile++;
                                        break;
                                    case -1: //跳过
                                        countSkipFile++;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Func.WriteLog(LogLevel.WARN, "Item \"" + path + "\" not found");
                        }
                        break;
                    case 'F':
                        //检查是否强制使用绝对路径
                        if (Varible.config.isForceAbsolutePath)
                        {
                            path = Path.GetFullPath(path);
                        }

                        result = ProcessSingleFile(path);

                        //增加文件计数
                        countTotalFile++;
                        switch (result)
                        {
                            case 0: //错误
                                countFailFile++;
                                resultItem = false;    //将项目结果设为false
                                break;
                            case 1: //成功
                                countSuccessFile++;
                                break;
                            case -1: //跳过
                                countSkipFile++;
                                break;
                        }
                        break;
                }

                //增加项目计数 并判断项目是否成功
                countTotalItem++;
                switch(resultItem)
                {
                    case false: //错误
                        countFailItem++;
                        break;
                    case true: //成功
                        countSuccessItem++;
                        break;
                }

                //更新统计信息与警告标志(3)
                UpdateStatistic(textProgress, path);
                if (!resultItem)
                {
                    rectWarn.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        rectWarn.Visibility = Visibility.Visible;
                    }));
                }
            }
            //关闭文件列表
            listReader.Close();
        }

        //处理文件
        public int ProcessSingleFile(string path)
        {
            //检查是否取消 2.在处理文件时
            if (worker.CancellationPending)
            {
                args.Cancel = true;
                return 2;
            }

            //若文件不存在则报错(若在此方法前检查过是否存在 此处不应出现文件不存在现象)
            if (!File.Exists(path))
            {
                Func.WriteLog(LogLevel.ERROR, "File \"" + path + "\" not found in executing function");
                return 0;
            }

            //是否是新增或修改的文件 此此假定为是 即需要进行处理
            bool isModified = true;

            //创建不含:和\等字符的文件名_path 用于生成文件名
            //检测是否强制使用相对路径 若是则分割path
            string _path = (Varible.config.isForceRelativePath ? path.Substring(dirLength) : path);
            _path = _path.Replace(@"\", Varible.config.replacementForBackslashInPath);
            _path = _path.Replace(":", Varible.config.replacementForColonInPath);
            string hash_path = @"Hash\" + _path + "_Hash.txt";
            string compress_path = Varible.config.outputDir + @"\" + _path + ".7z";

            //Hash
            if (Varible.config.isHashEnabled)
            {
                //计算Hash
                string hash = Func.GetFileSha1(path);

                //该文件的Hash文件不存在 则创建并计算Hash 并标记为需要处理
                if (!File.Exists(hash_path))
                {
                    FileStream hashFileStream = File.Create(hash_path);
                    hashFileStream.Write(System.Text.Encoding.UTF8.GetBytes(Func.GetFileSha1(path)));
                    hashFileStream.Close();
                    Func.WriteLog(LogLevel.DEBUG, "File \"" + path + "\" added");
                }
                //该文件的Hash文件存在 则比较Hash
                else
                {
                    StreamReader hashReader = new StreamReader(hash_path);
                    //Hash相同 且有Done标记 则标记为已存在的文件 不处理
                    if (hash + " Done" == hashReader.ReadLine())   //如果是新文件则读入的是空字符串 也与Hash不同
                    {
                        isModified = false;
                        Func.WriteLog(LogLevel.DEBUG, "File \"" + path + "\" skipped");
                        hashReader.Close();
                        //返回-1
                        return -1;
                    }
                    //Hash不同则记录新Hash
                    else
                    {
                        hashReader.Close();
                        try
                        {
                            using (StreamWriter hashWriter = new StreamWriter(hash_path, false))    //不使用Append
                            {
                                hashWriter.Write(hash);
                            }
                            Func.WriteLog(LogLevel.DEBUG, "File \"" + path + "\" updated");
                        }
                        catch (Exception e)
                        {
                            Func.WriteLog(LogLevel.ERROR, "Fail to write hash: " + e.Message);
                            return 1;
                        }
                    }
                }
            }
            //加密压缩
            if (Varible.config.isEncryptEnabled && isModified)
            {
                try
                {
                    //检测输出目录是否存在 不存在则创建
                    if (!Directory.Exists(Varible.config.outputDir))
                    {
                        Directory.CreateDirectory(Varible.config.outputDir);
                    }
                    
                    //使用SevenZipSharp
                    SevenZipBase.SetLibraryPath("7z.dll");  //使用SevenZipSharp需要指定7z.dll路径
                    SevenZipCompressor compressor = new SevenZipCompressor();
                    compressor.CompressionLevel = (CompressionLevel)Varible.config.compressionLevel;
                    compressor.EncryptHeaders = true;
                    compressor.CompressionFinished += (sender, eventArgs) =>
                    {
                        try
                        {
                            //如果压缩成功 则在Hash文件末尾添加Done标记
                            using (StreamWriter hashAppendWriter = new StreamWriter(hash_path, true))   //使用append
                            {
                                hashAppendWriter.Write(" Done");
                            }
                        }
                        catch (Exception e)
                        {
                            Func.WriteLog(LogLevel.ERROR, "Fail to append Done sign to hash: " + e.Message);
                        }
                        Func.WriteLog(LogLevel.DEBUG, "File \"" + path + "\" compression completed");
                    };
                    //开始压缩
                    compressor.CompressFilesEncrypted(compress_path, Varible.config.passwd, path);
                }
                catch (Exception e)
                {
                    Func.WriteLog(LogLevel.ERROR, "File \"" + path + "\" compression error: " + e.Message);
                    //压缩过程出错 返回0
                    return 0;
                }
            }
            //成功 返回1
            return 1;
        }
        public void UpdateInfo()
        {
            tblk_Info.Text = "Input: " + Varible.config.inputList
                           + "\nOutput: " + Varible.config.outputDir
                           + "\nPassword: " + (isPasswdShow ? Varible.config.passwd.ToString() : "***")
                           + "\nCompression Level: " + Varible.config.compressionLevel.ToString() + " / " + Func.GetCompressLvName(Varible.config.compressionLevel);
        }
        public void UpdateStatistic(TextBlock textProgress, string path)
        {
            //显示统计信息(避免"调用线程无法访问此对象，因为另一个线程拥有该对象"的错误)
            textProgress.Dispatcher.BeginInvoke(new Action(() =>
            {
                textProgress.Text = "Current: " + path
                                  + "\nItem Processed: " + countTotalItem
                                  + "\nItem Success: " + countSuccessItem
                                  + "\nItem Fail: " + countFailItem
                                  + "\nFile Processed: " + countTotalFile
                                  + "\nFile Skipped: " + countSkipFile
                                  + "\nFile Success: " + countSuccessFile
                                  + "\nFile Fail: " + countFailFile;
            }));
        }
    }

    public partial class Config
    {
        public string inputList { get; set; } = "input.txt";

        public string outputDir { get; set; } = "output";

        public string passwd { get; set; } = "123456";

        public bool isHashEnabled { get; set; } = true;

        public bool isEncryptEnabled { get; set; } = true;

        public string replacementForColonInPath { get; set; } = "$";

        public string replacementForBackslashInPath { get; set; } = "--";

        public bool isForceAbsolutePath { get; set; } = false;

        public bool isForceRelativePath { get; set; } = false;

        public int compressionLevel { get; set; } = 5;

        public bool isUploadShown { get; set; } = false;

        public string uploadProgram { get; set; } = "";

        public string uploadArg { get; set; } = "";

        public string logPath { get; set; } = "log.log";

        public int minLogLevel { get; set; } = 1;

        public override string ToString()
        {
            return "Config:\ninputList=" + this.inputList
                 + "\noutputDir=" + this.outputDir
                 + "\npasswd=" + this.passwd
                 + "\nisHashEnabled=" + this.isHashEnabled.ToString()
                 + "\nisEncryptEnabled=" + this.isEncryptEnabled.ToString()
                 + "\nreplacementForColonInPath=" + this.replacementForColonInPath
                 + "\nreplacementForBackslashInPath=" + this.replacementForBackslashInPath
                 + "\nisForceAbsolutePath=" + this.isForceAbsolutePath.ToString()
                 + "\nisForceRelativePath=" + this.isForceRelativePath.ToString()
                 + "\ncompressionLevel=" + this.compressionLevel.ToString()
                 + "\nisUploadShown=" + this.isUploadShown.ToString()
                 + "\nuploadProgram=" + this.uploadProgram
                 + "\nuploadArg=" + this.uploadArg
                 + "\nlogPath=" + this.logPath
                 + "\nminLogLevel=" + this.minLogLevel.ToString();
        }
    }

    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }
}