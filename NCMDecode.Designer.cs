using System;
using System.Drawing;
using System.Windows.Forms;

namespace NCMDecode
{
    partial class NCMDecode
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            OpenFileDialog = new OpenFileDialog();
            OpenFile = new Button();
            SavePath = new Button();
            ProgressBar = new ProgressBar();
            FileListView = new ListView();
            ListViewFileName = new ColumnHeader();
            ListViewFileType = new ColumnHeader();
            EnableMeta = new CheckBox();
            CreateFile = new Button();
            FolderBrowserDialog = new FolderBrowserDialog();
            SavePathLabel = new Label();
            TableLayoutPanel = new TableLayoutPanel();
            FileNameTableLayoutPanel = new TableLayoutPanel();
            FileNameLabel = new Label();
            FileNameBox = new TextBox();
            ButtonTableLayoutPanel = new TableLayoutPanel();
            EnableFileName = new CheckBox();
            EnableNetConnect = new CheckBox();
            ImageTableLayoutPanel = new TableLayoutPanel();
            CoverImage = new PictureBox();
            ToolTip = new ToolTip(components);
            TableLayoutPanel.SuspendLayout();
            FileNameTableLayoutPanel.SuspendLayout();
            ButtonTableLayoutPanel.SuspendLayout();
            ImageTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CoverImage).BeginInit();
            SuspendLayout();
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.Filter = "NCM文件(*.ncm)|*.ncm";
            OpenFileDialog.Multiselect = true;
            OpenFileDialog.Title = "选择NCM文件";
            // 
            // OpenFile
            // 
            OpenFile.Dock = DockStyle.Fill;
            OpenFile.Location = new Point(3, 3);
            OpenFile.Name = "OpenFile";
            OpenFile.Size = new Size(196, 31);
            OpenFile.TabIndex = 0;
            OpenFile.Text = "打开文件";
            OpenFile.UseVisualStyleBackColor = true;
            OpenFile.Click += OpenFile_Click;
            // 
            // SavePath
            // 
            SavePath.Dock = DockStyle.Fill;
            SavePath.Location = new Point(3, 40);
            SavePath.Name = "SavePath";
            SavePath.Size = new Size(196, 31);
            SavePath.TabIndex = 1;
            SavePath.Text = "保存路径";
            SavePath.UseVisualStyleBackColor = true;
            SavePath.Click += SavePath_Click;
            // 
            // ProgressBar
            // 
            ProgressBar.Dock = DockStyle.Bottom;
            ProgressBar.Location = new Point(0, 407);
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(832, 17);
            ProgressBar.TabIndex = 3;
            // 
            // FileListView
            // 
            FileListView.AllowDrop = true;
            FileListView.Columns.AddRange(new ColumnHeader[] { ListViewFileName, ListViewFileType });
            FileListView.Dock = DockStyle.Fill;
            FileListView.GridLines = true;
            FileListView.Location = new Point(3, 3);
            FileListView.Name = "FileListView";
            TableLayoutPanel.SetRowSpan(FileListView, 2);
            FileListView.Size = new Size(618, 360);
            FileListView.TabIndex = 5;
            FileListView.UseCompatibleStateImageBehavior = false;
            FileListView.View = View.Details;
            FileListView.SelectedIndexChanged += FileListView_SelectedIndexChanged;
            FileListView.DragDrop += FileListView_DragDrop;
            FileListView.DragEnter += FileListView_DragEnter;
            FileListView.MouseClick += FileListView_MouseClick;
            // 
            // ListViewFileName
            // 
            ListViewFileName.Text = "文件名";
            // 
            // ListViewFileType
            // 
            ListViewFileType.Text = "文件类型";
            ListViewFileType.Width = 100;
            // 
            // EnableMeta
            // 
            EnableMeta.AutoSize = true;
            EnableMeta.Checked = true;
            EnableMeta.CheckState = CheckState.Checked;
            EnableMeta.Dock = DockStyle.Fill;
            EnableMeta.Location = new Point(3, 77);
            EnableMeta.Name = "EnableMeta";
            EnableMeta.Size = new Size(196, 28);
            EnableMeta.TabIndex = 6;
            EnableMeta.Text = "保存元数据";
            EnableMeta.UseVisualStyleBackColor = true;
            // 
            // CreateFile
            // 
            CreateFile.Dock = DockStyle.Fill;
            CreateFile.Location = new Point(627, 369);
            CreateFile.Name = "CreateFile";
            CreateFile.Size = new Size(202, 35);
            CreateFile.TabIndex = 10;
            CreateFile.Text = "开始转换";
            CreateFile.UseVisualStyleBackColor = true;
            CreateFile.Click += CreateFile_Click;
            // 
            // FolderBrowserDialog
            // 
            FolderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            // 
            // SavePathLabel
            // 
            SavePathLabel.Location = new Point(0, 0);
            SavePathLabel.Name = "SavePathLabel";
            SavePathLabel.Size = new Size(0, 0);
            SavePathLabel.TabIndex = 0;
            SavePathLabel.Visible = false;
            // 
            // TableLayoutPanel
            // 
            TableLayoutPanel.ColumnCount = 2;
            TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            TableLayoutPanel.Controls.Add(FileListView, 0, 0);
            TableLayoutPanel.Controls.Add(CreateFile, 1, 2);
            TableLayoutPanel.Controls.Add(FileNameTableLayoutPanel, 0, 2);
            TableLayoutPanel.Controls.Add(ButtonTableLayoutPanel, 1, 0);
            TableLayoutPanel.Controls.Add(ImageTableLayoutPanel, 1, 1);
            TableLayoutPanel.Dock = DockStyle.Fill;
            TableLayoutPanel.Location = new Point(0, 0);
            TableLayoutPanel.Name = "TableLayoutPanel";
            TableLayoutPanel.RowCount = 3;
            TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TableLayoutPanel.Size = new Size(832, 407);
            TableLayoutPanel.TabIndex = 11;
            // 
            // FileNameTableLayoutPanel
            // 
            FileNameTableLayoutPanel.ColumnCount = 2;
            FileNameTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            FileNameTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            FileNameTableLayoutPanel.Controls.Add(FileNameLabel, 0, 0);
            FileNameTableLayoutPanel.Controls.Add(FileNameBox, 1, 0);
            FileNameTableLayoutPanel.Dock = DockStyle.Fill;
            FileNameTableLayoutPanel.Location = new Point(3, 369);
            FileNameTableLayoutPanel.Name = "FileNameTableLayoutPanel";
            FileNameTableLayoutPanel.RowCount = 1;
            FileNameTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            FileNameTableLayoutPanel.Size = new Size(618, 35);
            FileNameTableLayoutPanel.TabIndex = 11;
            // 
            // FileNameLabel
            // 
            FileNameLabel.AutoSize = true;
            FileNameLabel.Dock = DockStyle.Top;
            FileNameLabel.Location = new Point(0, 0);
            FileNameLabel.Margin = new Padding(0);
            FileNameLabel.Name = "FileNameLabel";
            FileNameLabel.Padding = new Padding(0, 6, 0, 0);
            FileNameLabel.Size = new Size(100, 30);
            FileNameLabel.TabIndex = 8;
            FileNameLabel.Text = "文件名格式";
            FileNameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FileNameBox
            // 
            FileNameBox.Dock = DockStyle.Fill;
            FileNameBox.Enabled = false;
            FileNameBox.Location = new Point(103, 3);
            FileNameBox.Name = "FileNameBox";
            FileNameBox.Size = new Size(831, 30);
            FileNameBox.TabIndex = 9;
            FileNameBox.Text = "{Artist|_} - {Title}.{Ext}";
            // 
            // ButtonTableLayoutPanel
            // 
            ButtonTableLayoutPanel.ColumnCount = 1;
            ButtonTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ButtonTableLayoutPanel.Controls.Add(EnableMeta, 0, 2);
            ButtonTableLayoutPanel.Controls.Add(EnableFileName, 0, 3);
            ButtonTableLayoutPanel.Controls.Add(SavePath, 0, 1);
            ButtonTableLayoutPanel.Controls.Add(OpenFile, 0, 0);
            ButtonTableLayoutPanel.Controls.Add(EnableNetConnect, 0, 4);
            ButtonTableLayoutPanel.Dock = DockStyle.Fill;
            ButtonTableLayoutPanel.Location = new Point(627, 3);
            ButtonTableLayoutPanel.Name = "ButtonTableLayoutPanel";
            ButtonTableLayoutPanel.RowCount = 5;
            ButtonTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            ButtonTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            ButtonTableLayoutPanel.RowStyles.Add(new RowStyle());
            ButtonTableLayoutPanel.RowStyles.Add(new RowStyle());
            ButtonTableLayoutPanel.RowStyles.Add(new RowStyle());
            ButtonTableLayoutPanel.Size = new Size(202, 177);
            ButtonTableLayoutPanel.TabIndex = 12;
            // 
            // EnableFileName
            // 
            EnableFileName.AutoSize = true;
            EnableFileName.Dock = DockStyle.Fill;
            EnableFileName.Location = new Point(3, 111);
            EnableFileName.Name = "EnableFileName";
            EnableFileName.Size = new Size(196, 28);
            EnableFileName.TabIndex = 1;
            EnableFileName.Text = "自定义文件名";
            EnableFileName.UseVisualStyleBackColor = true;
            EnableFileName.CheckedChanged += EnableFileName_CheckedChanged;
            // 
            // EnableNetConnect
            // 
            EnableNetConnect.AutoSize = true;
            EnableNetConnect.Location = new Point(3, 145);
            EnableNetConnect.Name = "EnableNetConnect";
            EnableNetConnect.Size = new Size(144, 28);
            EnableNetConnect.TabIndex = 7;
            EnableNetConnect.Text = "启用联网下载";
            EnableNetConnect.UseVisualStyleBackColor = true;
            EnableNetConnect.CheckedChanged += EnableNetConnect_CheckedChanged;
            // 
            // ImageTableLayoutPanel
            // 
            ImageTableLayoutPanel.ColumnCount = 1;
            ImageTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ImageTableLayoutPanel.Controls.Add(CoverImage, 0, 0);
            ImageTableLayoutPanel.Dock = DockStyle.Fill;
            ImageTableLayoutPanel.Location = new Point(627, 186);
            ImageTableLayoutPanel.Name = "ImageTableLayoutPanel";
            ImageTableLayoutPanel.RowCount = 2;
            ImageTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ImageTableLayoutPanel.RowStyles.Add(new RowStyle());
            ImageTableLayoutPanel.Size = new Size(202, 177);
            ImageTableLayoutPanel.TabIndex = 13;
            // 
            // CoverImage
            // 
            CoverImage.Dock = DockStyle.Fill;
            CoverImage.Location = new Point(3, 3);
            CoverImage.Name = "CoverImage";
            CoverImage.Size = new Size(196, 171);
            CoverImage.SizeMode = PictureBoxSizeMode.Zoom;
            CoverImage.TabIndex = 0;
            CoverImage.TabStop = false;
            // 
            // NCMDecode
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(832, 424);
            Controls.Add(TableLayoutPanel);
            Controls.Add(SavePathLabel);
            Controls.Add(ProgressBar);
            Name = "NCMDecode";
            Text = "NCMDecode";
            Load += NCMDecode_Load;
            Resize += NCMDecode_Resize;
            TableLayoutPanel.ResumeLayout(false);
            FileNameTableLayoutPanel.ResumeLayout(false);
            FileNameTableLayoutPanel.PerformLayout();
            ButtonTableLayoutPanel.ResumeLayout(false);
            ButtonTableLayoutPanel.PerformLayout();
            ImageTableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)CoverImage).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private OpenFileDialog OpenFileDialog;
        private Button OpenFile;
        private Button SavePath;
        private ProgressBar ProgressBar;
        private ListView FileListView;
        private CheckBox EnableMeta;
        private ColumnHeader ListViewFileName;
        private Button CreateFile;
        private FolderBrowserDialog FolderBrowserDialog;
        private Label SavePathLabel;
        private TableLayoutPanel TableLayoutPanel;
        private TableLayoutPanel FileNameTableLayoutPanel;
        private Label FileNameLabel;
        private TextBox FileNameBox;
        private CheckBox EnableFileName;
        private TableLayoutPanel ButtonTableLayoutPanel;
        private TableLayoutPanel ImageTableLayoutPanel;
        private PictureBox CoverImage;
        private ColumnHeader ListViewFileType;
        private ToolTip ToolTip;
        private CheckBox EnableNetConnect;
    }
}
