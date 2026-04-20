using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCMDecode
{
    public partial class NCMDecode : Form
    {
        public NCMDecode()
        {
            InitializeComponent();
        }

        private static readonly HttpClient httpClient = new();
        private readonly Dictionary<string, byte[]> CoverDictionary = [];

        //画一个占位的X 提示找不到对应的图片
        private static Bitmap CreatePlaceholderImage()
        {
            const int size = 128;
            using var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(245, 245, 245));
            using var pen = new Pen(Color.FromArgb(220, 53, 69), 8) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            int m = 24;
            g.DrawLine(pen, m, m, size - m, size - m);
            g.DrawLine(pen, size - m, m, m, size - m);
            return new Bitmap(bmp);
        }

        private void AddItemToListView(string filePath)
        {
            foreach (ListViewItem item in FileListView.Items)
            {
                if (item.Text == Path.GetFileName(filePath)) return;
            }

            ListViewItem fileItem = new(Path.GetFileName(filePath))!;
            fileItem.Tag = filePath;
            fileItem.SubItems.Add(NcmDecryptor.FileType(NcmDecryptor.PreLoad(filePath)));

            FileListView.Items.Add(fileItem);
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string File in OpenFileDialog.FileNames)
                {
                    AddItemToListView(File);
                }
                SavePath_Click(sender, e);
            }
        }
        private void FileListView_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = FileListView.HitTest(e.Location);
            if (hit.Item == null || e.Button != MouseButtons.Right) return;

            string filePath = hit.Item.Tag!.ToString()!;
            CoverDictionary.Remove(filePath);

            FileListView.Items.Remove(hit.Item);
            CoverImage.Image?.Dispose();
            CoverImage.Image = null;
        }

        private async void FileListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems.Count == 0) return;
            CoverImage.Image?.Dispose();
            CoverImage.Image = null;

            string filePath = FileListView.SelectedItems[0].Tag!.ToString()!;

            using var ncmData = NcmDecryptor.PreLoad(filePath);
            byte[] imgData = NcmDecryptor.Cover(ncmData);

            void Cover(byte[] data)
            {
                using var ms = new MemoryStream(data);
                using var tmp = Image.FromStream(ms);
                CoverImage.Image = new Bitmap(tmp);
            }

            if (imgData.Length > 0) { Cover(imgData); return; }
            if (CoverDictionary.TryGetValue(filePath, out var data)) { Cover(data); return; }

            if (EnableNetConnect.Checked && EnableNetConnect.Enabled)
            {
                if (JsonDocument.Parse(ncmData.JsonStr).RootElement.TryGetProperty("albumPic", out var url))
                {
                    try
                    {
                        var coverData = await httpClient.GetByteArrayAsync(url.GetString());
                        if (coverData.Length > 0) { CoverDictionary[filePath] = coverData; Cover(coverData); return; }
                    }
                    catch { }
                }
            }
            CoverImage.Image = CreatePlaceholderImage();
        }
        private void FileListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;

        }
        private void FileListView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data!.GetData(DataFormats.FileDrop)!;

            if (files == null || files.Length == 0) return;

            foreach (string filePath in files)
            {
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".ncm")
                {
                    AddItemToListView(filePath);
                }
            }
        }

        private void SavePath_Click(object sender, EventArgs e)
        {
            if (FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SavePathLabel.Text = FolderBrowserDialog.SelectedPath;
            }
        }

        private async void CreateFile_Click(object sender, EventArgs e)
        {
            CreateFile.Enabled = false;
            if (EnableNetConnect.Checked) await EnableNetConnectAsync();
            ProgressBar.Value = 0;
            int count = FileListView.Items.Count;
            for (int i = 0; i < count; i++)
            {
                ProgressBar.Value = i * 100 / count;
                string tag = FileListView.Items[i].Tag!.ToString()!;
                using var V = NcmDecryptor.PreLoad(tag);

                string fileName;
                if (EnableFileName.Checked)
                {
                    try { fileName = NcmDecryptor.CustomFileName(V, FileNameBox.Text); }
                    catch { fileName = Path.GetFileNameWithoutExtension(tag) + "." + NcmDecryptor.FileType(V); }
                }
                else { fileName = Path.GetFileNameWithoutExtension(tag) + "." + NcmDecryptor.FileType(V); }

                var filePath = Path.Combine(SavePathLabel.Text, fileName);
                byte[]? CoverData = EnableNetConnect.Checked && CoverDictionary.TryGetValue(tag, out var img) ? img : null;
                if (EnableMeta.Checked) NcmDecryptor.MetaFile(V, filePath, CoverData);
                else NcmDecryptor.File(V, filePath);

            }
            if (count > 0) ProgressBar.Value = 100;
            CreateFile.Enabled = true;
        }

        private void EnableFileName_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableFileName.Checked)
                FileNameBox.Enabled = true;
            else
                FileNameBox.Enabled = false;
        }

        private async Task EnableNetConnectAsync()
        {
            ProgressBar.Value = 0;

            int count = FileListView.Items.Count;
            for (int i = 0; i < count; i++)
            {
                string filePath = FileListView.Items[i].Tag!.ToString()!;
                if (CoverDictionary.ContainsKey(filePath)) { ProgressBar.Value = i * 100 / count; continue; }

                using var ncmData = NcmDecryptor.PreLoad(filePath);
                byte[] imgData = NcmDecryptor.Cover(ncmData);
                if (imgData.Length > 0) { CoverDictionary[filePath] = imgData; ProgressBar.Value = i * 100 / count; continue; }

                if (EnableNetConnect.Checked)
                {
                    if (JsonDocument.Parse(ncmData.JsonStr).RootElement.TryGetProperty("albumPic", out var url))
                    {
                        try { CoverDictionary[filePath] = await httpClient.GetByteArrayAsync(url.GetString()); }
                        catch { }
                    }
                }
            }
            if (count > 0) ProgressBar.Value = 100;
        }
        private async void EnableNetConnect_CheckedChanged(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(EnableNetConnect, "当前已经开始下载 等待进度条完成即可\n若有下载失败可再次开启或者直接选中列表等待下载");
            EnableNetConnect.Enabled = false;
            CreateFile.Enabled = false;
            await EnableNetConnectAsync();
            EnableNetConnect.Enabled = true;
            CreateFile.Enabled = true;
            ToolTip.SetToolTip(EnableNetConnect, "允许联网获取NCM中缺失的封面图片 需要元数据可正常读取\n其他功能待引入");

        }

        private void NCMDecode_Resize(object sender, EventArgs e)
        {
            FileListView.Columns[0].Width = FileListView.ClientSize.Width - 100;
        }

        private void NCMDecode_Load(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(FileListView, "选中列表可预览封面\n右键选中可移除");
            ToolTip.SetToolTip(EnableMeta, "保存从NCM获取的元数据(封面,作者,标题等)到文件中\n解密的文件可能原本就存在元数据, 如果不开启则不会覆盖");
            ToolTip.SetToolTip(EnableFileName, "以NCM获取到的元数据重命名文件");
            ToolTip.SetToolTip(EnableNetConnect, "允许联网获取NCM中缺失的封面图片 需要元数据可正常读取\n其他功能待引入");
            ToolTip.SetToolTip(CreateFile, "生成列表中的所有文件");
            ToolTip.SetToolTip(FileNameBox, "可用自定义格式:\n{Title} - 标题\n{Artist|_} - 作者,以'_'分隔\n{Ext} - 扩展名");
            ToolTip.SetToolTip(ProgressBar, "实际上, 这是一个进度条!\n程序: RainbowFunny");
            NCMDecode_Resize(sender, e);
        }
    }
}
