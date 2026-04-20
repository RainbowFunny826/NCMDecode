using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
#if WINDOWS
using System.Windows.Forms;
#endif

namespace NCMDecode
{
    public static partial class NcmDecryptor
    {
        private static byte[] Unpad(byte[] d) => d.Length == 0 ? d : d.AsSpan(0, d.Length - d[^1]).ToArray();

        private static byte[] AesDecrypt(byte[] data, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key; aes.Mode = CipherMode.ECB; aes.Padding = PaddingMode.None;
            return aes.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
        }

        public class DecryptState : IDisposable
        {
            public required FileStream Fs { get; set; }
            public required byte[] Ks { get; set; }
            public required string JsonStr { get; set; }
            public required byte[] ImgData { get; set; }
            public long AudioStart { get; set; }
            public void Dispose()
            {
                Fs?.Dispose();
                GC.SuppressFinalize(this);
            }

        }

        public static DecryptState PreLoad(string inputPath)
        {
            // 万年不改的两个密钥
            byte[] coreKey = Convert.FromHexString("687A4852416D736F356B496E62617857");
            byte[] metaKey = Convert.FromHexString("2331346C6A6B5F215C5D2630553C2728");

            var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs, Encoding.UTF8, true);

            // NCM 魔术头 CTENFDAM
            byte[] header = br.ReadBytes(8);
            if (header.Length != 8 || !header.SequenceEqual("CTENFDAM"u8))
                throw new InvalidDataException();

            // 跳过 2 个字节
            fs.Seek(2, SeekOrigin.Current);

            byte[] keyData = br.ReadBytes(br.ReadInt32());
            for (int i = 0; i < keyData.Length; i++) keyData[i] ^= 0x64;

            byte[] decKey = AesDecrypt(keyData, coreKey);
            byte[] pkcsKey = Unpad(decKey);
            byte[] rc4Seed = pkcsKey[17..];

            byte[] kBox = new byte[256];
            for (int i = 0; i < 256; i++) kBox[i] = (byte)i;

            for (int i = 0, c = 0, off = 0; i < 256; i++)
            {
                byte swap = kBox[i];
                c = (swap + c + rc4Seed[off]) & 0xFF;
                (kBox[i], kBox[c]) = (kBox[c], swap);
                if (++off >= rc4Seed.Length) off = 0;
            }

            // NCM 元数据 163 key(Don't modify):
            byte[] metaRaw = br.ReadBytes(br.ReadInt32());
            string jsonStr = "{}"; //读取失败标记
            if (metaRaw.Length > 22)
            {
                for (int i = 0; i < metaRaw.Length; i++) metaRaw[i] ^= 0x63;

                byte[] metaB64 = Convert.FromBase64String(Encoding.ASCII.GetString(metaRaw[22..]));
                byte[] decMeta = AesDecrypt(metaB64, metaKey);
                jsonStr = Encoding.UTF8.GetString(Unpad(decMeta))[6..];
            }

            using var doc = JsonDocument.Parse(jsonStr);

            uint crc32 = br.ReadUInt32();

            byte version = br.ReadByte();
            int coverLength = br.ReadInt32();
            int image1Len = br.ReadInt32();

            byte[] imgData = br.ReadBytes(image1Len);

            long image2Len = coverLength - image1Len;
            if (image2Len > 0) fs.Seek(image2Len, SeekOrigin.Current);

            byte[] ks = new byte[256];
            for (int j = 0; j < 256; j++) ks[j] = kBox[(kBox[j] + kBox[(kBox[j] + j) & 0xFF]) & 0xFF];

            return new DecryptState
            {
                Fs = fs,
                Ks = ks,
                JsonStr = jsonStr,
                ImgData = imgData,
                AudioStart = fs.Position
            };
        }

        // 实际上应该没有用 但是留着吧
        public static byte[] Cover(DecryptState V) => V.ImgData;

        public static string FileType(DecryptState V)
        {
            if (V.JsonStr == "{}")
            {
                var fs = V.Fs;
                fs.Seek(V.AudioStart, SeekOrigin.Begin);
                byte[] buf = new byte[4];
                fs.ReadExactly(buf, 0, 4);
                for (int i = 0; i < 4; i++)
                    buf[i] ^= V.Ks[(i + 1) & 0xFF];
                if (buf.SequenceEqual("ID3"u8)) return "mp3";
                if (buf.SequenceEqual("fLaC"u8)) return "flac";
            }
            using var doc = JsonDocument.Parse(V.JsonStr);
            if (doc.RootElement.TryGetProperty("format", out var format))
                return format.GetString()!;
            return "Unknow";
        }

        public static string CustomFileName(DecryptState V, string CustomName)
        {
            //"可用自定义格式:\n{Title} - 标题\n{Artist|_} - 作者,以'_'分隔\n{Ext} - 扩展名"
            //{"musicName":"XXX","artist":[["A",1],["B",2]],"format":"mp3"}
            if (V.JsonStr == "{}")
                throw new NullReferenceException();
            using var doc = JsonDocument.Parse(V.JsonStr);
            string result = CustomName.Replace("{Title}", doc.RootElement.GetProperty("musicName").GetString()!)
                                      .Replace("{Ext}", doc.RootElement.GetProperty("format").GetString()!);
            Match match = ArtistRegex().Match(CustomName);
            if (match.Success)
                result = result.Replace(match.Value, string.Join(match.Groups[1].Value, doc.RootElement.GetProperty("artist").EnumerateArray().Select(x => x[0].GetString()!)));

            if (NameRegex().IsMatch(result))
                throw new ArgumentException("生成的文件名包含非法字符。", result);
            else
                return result;

        }

        public static void File(DecryptState V, string outputPath)
        {
            using var bw = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            var fs = V.Fs;
            fs.Seek(V.AudioStart, SeekOrigin.Begin);

            byte[] buf = new byte[0x8000];
            while (fs.Read(buf) is int len && len > 0)
            {
                for (int i = 0; i < len; i++) buf[i] ^= V.Ks[(i + 1) & 0xFF];
                bw.Write(buf, 0, len);
            }
        }

        //Gemini Pro 3.1巨献对ID3V2.3和FLAC的支持
        public static void MetaFile(DecryptState V, string outputPath, byte[]? coverData = null)
        {
            byte[] imgData = coverData ?? V.ImgData;
            if (V.JsonStr == "{}")
            {
                File(V, outputPath);
                return;
            }
            using var doc = JsonDocument.Parse(V.JsonStr);
            string title = doc.RootElement.GetProperty("musicName").GetString()!;
            string artist = string.Join("; ", doc.RootElement.GetProperty("artist").EnumerateArray().Select(x => x[0].GetString()!));
            string album = doc.RootElement.GetProperty("album").GetString()!;
            string format = FileType(V);

            using var bw = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            byte[] buf = new byte[0x8000];
            int totalRead = 0;

            int ReadDec(byte[] b, int count)
            {
                int read = V.Fs.Read(b, 0, count);
                for (int i = 0; i < read; i++) b[i] ^= V.Ks[(totalRead + i + 1) & 0xFF];
                totalRead += read;
                return read;
            }

            V.Fs.Seek(V.AudioStart, SeekOrigin.Begin);

            if (format == "mp3")
            {
                byte[] h = new byte[10];
                ReadDec(h, 10);
                int skip = (h[0] == 'I' && h[1] == 'D' && h[2] == '3') ? ((h[6] << 21) | (h[7] << 14) | (h[8] << 7) | h[9]) + 10 : 0;

                bw.Write("ID3"u8); bw.Write([3, 0, 0, 0, 0, 0, 0]);
                long startPos = bw.Position;
                void WFrame(string id, string val)
                {
                    byte[] b = Encoding.Unicode.GetBytes(val);
                    bw.Write(Encoding.ASCII.GetBytes(id));
                    int l = b.Length + 3;
                    bw.Write([(byte)(l >> 24), (byte)(l >> 16), (byte)(l >> 8), (byte)l, 0, 0, 1, 0xFF, 0xFE]);
                    bw.Write(b);
                }
                WFrame("TIT2", title); WFrame("TPE1", artist); WFrame("TALB", album);

                string mime = imgData.Length > 2 && imgData[0] == 0x89 ? "image/png" : "image/jpeg";
                byte[] mb = Encoding.ASCII.GetBytes(mime);
                int pl = 14 + mb.Length + imgData.Length - 10;
                bw.Write("APIC"u8); bw.Write([(byte)(pl >> 24), (byte)(pl >> 16), (byte)(pl >> 8), (byte)pl, 0, 0, 0]);
                bw.Write(mb); bw.Write([0, 3, 0]); bw.Write(imgData);

                long tagLen = bw.Position - startPos;
                bw.Seek(6, SeekOrigin.Begin);
                bw.Write([(byte)((tagLen >> 21) & 0x7F), (byte)((tagLen >> 14) & 0x7F), (byte)((tagLen >> 7) & 0x7F), (byte)(tagLen & 0x7F)]);
                bw.Seek(0, SeekOrigin.End);

                if (skip > 10) { byte[] d = new byte[skip - 10]; ReadDec(d, d.Length); }
                else if (skip == 0) { V.Fs.Seek(V.AudioStart, SeekOrigin.Begin); totalRead = 0; }
            }
            else if (format == "flac")
            {
                byte[] h = new byte[4]; ReadDec(h, 4);
                bw.Write(h);
                bool last = false;
                while (!last)
                {
                    byte[] bh = new byte[4]; ReadDec(bh, 4);
                    last = (bh[0] & 0x80) != 0;
                    int type = bh[0] & 0x7F;
                    int size = (bh[1] << 16) | (bh[2] << 8) | bh[3];
                    if (type == 0)
                    {
                        bh[0] &= 0x7F;
                        bw.Write(bh);
                        byte[] sd = new byte[size]; ReadDec(sd, size); bw.Write(sd);

                        using var vm = new MemoryStream();
                        using (var vw = new BinaryWriter(vm))
                        {
                            vw.Write(4); vw.Write("Lavf"u8); vw.Write(3);
                            foreach (var s in new[] { $"TITLE={title}", $"ARTIST={artist}", $"ALBUM={album}" })
                            {
                                byte[] b = Encoding.UTF8.GetBytes(s); vw.Write(b.Length); vw.Write(b);
                            }
                        }
                        byte[] vb = vm.ToArray();
                        bw.WriteByte(4); bw.Write([(byte)(vb.Length >> 16), (byte)(vb.Length >> 8), (byte)vb.Length]);
                        bw.Write(vb);

                        string mime = imgData.Length > 2 && imgData[0] == 0x89 ? "image/png" : "image/jpeg";
                        byte[] mb = Encoding.ASCII.GetBytes(mime);
                        int pl = 32 + mb.Length + imgData.Length;
                        bw.WriteByte(0x86);
                        bw.Write([(byte)(pl >> 16), (byte)(pl >> 8), (byte)pl]);
                        bw.Write([0, 0, 0, 3]);
                        bw.Write([(byte)(mb.Length >> 24), (byte)(mb.Length >> 16), (byte)(mb.Length >> 8), (byte)mb.Length]);
                        bw.Write(mb);
                        bw.Write(new byte[20]);
                        bw.Write([(byte)(imgData.Length >> 24), (byte)(imgData.Length >> 16), (byte)(imgData.Length >> 8), (byte)imgData.Length]);
                        bw.Write(imgData);
                    }
                    else { byte[] d = new byte[size]; ReadDec(d, size); }
                }
            }

            int len;
            while ((len = ReadDec(buf, buf.Length)) > 0) bw.Write(buf, 0, len);
        }

        [GeneratedRegex(@"\{Artist\|(.*?)\}")]
        private static partial Regex ArtistRegex();
        [GeneratedRegex(@"[\x00-\x1f<>""\\/|?*:]")]
        private static partial Regex NameRegex();
    }
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [STAThread]
        static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (args.Length > 0)
                {
                    CLIMode(args);
                }
                else
                {
#if WINDOWS
                    GUIMode();
#endif
                }
            }
            else
            {
                if (args.Length == 0) args = ["-h"];
                CLIMode(args);
            }
        }
        static void Help()
        {
            Console.WriteLine("NCMDecode v1.0.0 --By RainbowFunny");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("用法:");
            Console.WriteLine("  NCMDecode -i <输入路径> [选项]");
            Console.WriteLine();
            Console.WriteLine("参数:");
            Console.WriteLine("  -i, --input     <路径>    指定输入文件或文件夹路径 (必填)");
            Console.WriteLine("  -o, --output    <路径>    指定输出文件或文件夹路径 (默认输入文件同级目录, 指定文件路径时覆盖自定义名称)");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  --enable-meta             保存从NCM获取的元数据(封面,作者,标题等)到文件中");
            Console.WriteLine("                            解密的文件可能原本就存在元数据, 如果不开启则不会覆盖");
            Console.WriteLine();
            Console.WriteLine("  --enable-net              允许联网获取NCM中缺失的封面图片, 需要元数据可正常读取");
            Console.WriteLine();
            Console.WriteLine("  --custom-name   <名称>    以NCM获取到的元数据重命名文件");
            Console.WriteLine("                            可用自定义格式: {Title} - 标题 {Artist|_} - 作者,以'_'分隔 {Ext} - 扩展名");
            Console.WriteLine();
            Console.WriteLine("  -h, --help                显示此帮助信息");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  # 解码单个文件");
            Console.WriteLine("  NCMDecode -i \"song.ncm\"");
            Console.WriteLine();
            Console.WriteLine("  # 解码整个文件夹并保存到 D:\\Music");
            Console.WriteLine("  NCMDecode -i \"C:\\DownloadMusic\" -o \"D:\\Music\" --enable-meta --custom-name \"{Artist|_} - {Title}.{Ext}\"");
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------");
        }
        public class CliConfig
        {
            public string? InputPath { get; set; }
            public string? OutputPath { get; set; }
            public bool EnableMeta { get; set; }
            public bool EnableNet { get; set; }
            public string? CustomName { get; set; }
            public bool Help { get; set; }
        }
        static CliConfig ParseArguments(string[] args)
        {
            var config = new CliConfig();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (string.IsNullOrEmpty(arg)) continue;

                if (arg.StartsWith("--"))
                {
                    string key = arg[2..].ToLower();
                    switch (key)
                    {
                        case "help":
                            config.Help = true;
                            break;
                        case "enable-meta":
                            config.EnableMeta = true;
                            break;
                        case "enable-net":
                            config.EnableNet = true;
                            break;
                        case "custom-name":
                            if (i + 1 < args.Length) config.CustomName = args[++i];
                            break;
                        case "input":
                            if (i + 1 < args.Length) config.InputPath = args[++i];
                            break;
                        case "output":
                            if (i + 1 < args.Length) config.OutputPath = args[++i];
                            break;
                    }
                }
                else if (arg.StartsWith('-') && arg.Length > 1)
                {
                    char key = arg[1];
                    switch (char.ToLower(key))
                    {
                        case 'h':
                            config.Help = true;
                            break;
                        case 'i':
                            if (i + 1 < args.Length) config.InputPath = args[++i];
                            break;
                        case 'o':
                            if (i + 1 < args.Length) config.OutputPath = args[++i];
                            break;
                    }
                }
            }
            return config;
        }


        static void CLIMode(string[] args)
        {
            //用来绑定控制台 但是退出时无法释放 不清楚如何解决
#if WINDOWS
            if (!AttachConsole(-1)) AllocConsole();
#endif
            Console.WriteLine();
            var config = ParseArguments(args);

            if (config.Help || string.IsNullOrEmpty(config.InputPath))
            {
                Help();
                return;
            }

            static IEnumerable<string> NcmFiles(string path)
            {
                if (File.Exists(path) && Path.GetExtension(path).Equals(".ncm", StringComparison.CurrentCultureIgnoreCase)) yield return path;
                else if (Directory.Exists(path))
                    foreach (var file in Directory.EnumerateFiles(path, "*.ncm", SearchOption.AllDirectories)) yield return file;
            }

            var files = NcmFiles(config.InputPath).ToList();
            if (files.Count == 0) { Console.WriteLine("未找到有效的.ncm文件"); return; }

            using var httpClient = new HttpClient();
            var CoverDictionary = new Dictionary<string, byte[]>();
            bool isFile = files.Count == 1;
            string outBase = config.OutputPath ?? (isFile ? Path.GetDirectoryName(config.InputPath)! : config.InputPath);

            Console.WriteLine($"开始处理 {files.Count} 个文件...");

            for (int i = 0; i < files.Count; i++)
            {
                string filePath = files[i];
                Console.WriteLine($"[{i + 1}/{files.Count}] -- {Path.GetFileName(filePath)}");

                using var ncmData = NcmDecryptor.PreLoad(filePath);
                byte[] imgData = NcmDecryptor.Cover(ncmData);

                if (imgData.Length > 0) CoverDictionary[filePath] = imgData;
                else if (config.EnableNet)
                {
                    try
                    {
                        if (JsonDocument.Parse(ncmData.JsonStr).RootElement.TryGetProperty("albumPic", out var url))
                            CoverDictionary[filePath] = httpClient.GetByteArrayAsync(url.GetString()).GetAwaiter().GetResult();
                    }
                    catch { }
                }

                string fileName;
                if (!string.IsNullOrEmpty(config.CustomName))
                {
                    try { fileName = NcmDecryptor.CustomFileName(ncmData, config.CustomName); }
                    catch { fileName = Path.GetFileNameWithoutExtension(filePath) + "." + NcmDecryptor.FileType(ncmData); }
                }
                else { fileName = Path.GetFileNameWithoutExtension(filePath) + "." + NcmDecryptor.FileType(ncmData); }

                string outputPath = (isFile && !string.IsNullOrEmpty(config.OutputPath) && Path.HasExtension(config.OutputPath)) ? config.OutputPath! : Path.Combine(outBase, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                byte[]? CoverData = config.EnableNet && CoverDictionary.TryGetValue(filePath, out var img) ? img : null;
                if (config.EnableMeta) NcmDecryptor.MetaFile(ncmData, outputPath, CoverData);
                else NcmDecryptor.File(ncmData, outputPath);
            }

            Console.WriteLine("完成");
            //此处退出后在Windows需要再按一次任意键 其他系统则不需要
            Environment.Exit(0);
        }
#if WINDOWS
        static void GUIMode()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new NCMDecode());
        }
#endif
    }
}
