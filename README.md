# NCMDecode
使用.NET 10编写的NCM格式解码程序

---
# 使用方法
  编译此项目或者通过Release下载最新的NCMDecode程序

  在Windows上可以双击运行exe 打开图形化界面
  或者使用任意参数打开命令行界面

  Linux及其他仅可使用命令行界面 暂无添加图形界面打算

  可用`NCMDecode -h`打开帮助
```
  用法:
  NCMDecode -i <输入路径> [选项]

参数:
  -i, --input     <路径>    指定输入文件或文件夹路径 (必填)
  -o, --output    <路径>    指定输出文件或文件夹路径 (默认输入文件同级目录, 指定文件路径时覆盖自定义名称)

选项:
  --enable-meta             保存从NCM获取的元数据(封面,作者,标题等)到文件中
                            解密的文件可能原本就存在元数据, 如果不开启则不会覆盖

  --enable-net              允许联网获取NCM中缺失的封面图片, 需要元数据可正常读取

  --custom-name   <名称>    以NCM获取到的元数据重命名文件
                            可用自定义格式: {Title} - 标题 {Artist|_} - 作者,以'_'分隔 {Ext} - 扩展名

  -h, --help                显示此帮助信息

示例:
  # 解码单个文件
  NCMDecode -i "song.ncm"

  # 解码整个文件夹并保存到 D:\Music
  NCMDecode -i "C:\DownloadMusic" -o "D:\Music" --enable-meta --custom-name "{Artist|_} - {Title}.{Ext}"
```

---
# 编译
  Windows `dotnet publish -c Release -r win-x64`
  
  Linux `dotnet publish -c Release -r linux-x64`

---
# 已知问题
  - 由于启用跨平台识别 在Visual Studio可能会加载异常
  - Windows下命令行界面无法正常取消捕获控制台
