# 视频重复查找器

视频重复查找器是一款跨平台软件，用于根据相似度在硬盘上查找重复的视频和图像文件。与其他重复文件查找器不同，该软件还能找到通过转码或格式转换产生的重复文件。

# 功能特性

- 🌍 跨平台支持（Windows、Linux、macOS）
- ⚡ 快速扫描速度
- 🚀 超快速重新扫描
- 📹 可选通过 FFmpeg 本地函数加快速度
- 🔍 基于相似度查找重复视频/图像（支持感知哈希 pHash）
- 🎬 部分片段检测 — 查找较短视频是否为较长视频的部分片段（音频指纹识别）
- 🖥️ 桌面 GUI（Windows、Linux、macOS）
- 💻 无头 CLI 用于脚本和自动化
- 🌐 Web UI 用于远程/无头/NAS 使用
- 🐳 Docker 镜像便于自建部署

# 部分片段检测

VDF 可以检测较短的视频是否为较长视频的部分片段 — 例如，电影中的一个场景或从较长录制中保存的片段。即使没有视觉重叠，这也能工作。

它在**正常视觉重复扫描后作为可选的第二阶段**运行，使用音频指纹识别管道（Chromaprint 风格的色度提取 + 滑动窗口 Hamming 相似度匹配）。

### 启用方式

在**设置 → 部分片段检测**中，勾选**启用部分片段检测**并调整：

| 设置 | 默认值 | 说明 |
|---------|---------|-------------|
| 最小片段/源时长比 (%) | 10 | 片段时长相对于源时长的最小百分比。比此值更短的片段将被忽略。 |
| 最小音频相似度 (%) | 80 | 滑动窗口指纹匹配被接受的最小平均 Hamming 相似度。 |

> **注意：** 部分片段检测需要两个文件都有音频轨道。没有音频的视频将被跳过。

---

# 下载

[每日构建](https://github.com/0x90d/videoduplicatefinder/releases/tag/3.0.x) — 附件在每次提交时自动重建和替换。

各平台可用的软件包：
- `GUI-<platform>` — 桌面应用程序
- `CLI-<platform>` — 命令行工具
- `Web-<platform>` — 自包含的 Web 服务器

---

# 桌面 GUI

### 系统要求

FFmpeg 和 FFprobe 是必需的。VDF 在首次启动时会尝试自动下载。
本地 FFmpeg 绑定需要 FFmpeg 8.x 共享库（不是 master 分支）。

#### Windows
从 https://ffmpeg.org/download.html 下载最新的 FFmpeg GPL 共享包。
将 `ffmpeg.exe` 和 `ffprobe.exe` 解压到与 `VDF.GUI.exe` 相同的文件夹、名为 `bin` 的子文件夹中，或确保它们在您的 `PATH` 中。

#### Linux
```bash
sudo apt-get update && sudo apt-get install ffmpeg
```
然后运行：
```bash
chmod +x VDF.GUI
./VDF.GUI
```

**可选：添加到应用菜单**

Linux 存档包含 `videoduplicatefinder.desktop` 和 `icon.png`。要向桌面环境（GNOME、KDE、XFCE 等）注册应用程序：

```bash
# 编辑 Exec= 和 Icon= 路径以匹配您解压存档的位置，例如：
sed -i "s|/opt/videoduplicatefinder|$(pwd)|g" videoduplicatefinder.desktop

# 为当前用户安装
mkdir -p ~/.local/share/applications
cp videoduplicatefinder.desktop ~/.local/share/applications/
```

应用程序将在应用启动器中显示，带有其图标。

#### macOS
```bash
brew install ffmpeg
```
解压存档 — 其中包含 `Video Duplicate Finder.app`。双击即可启动。

如果 macOS 显示 "无法打开，因为无法验证开发者"，右键单击 `.app` 并选择**打开**，然后确认。您只需执行一次。

如果 macOS 仍然拒绝启动该包（例如在 macOS 14+ / Tahoe 上显示 "库加载被系统策略禁止"），清除隔离标志并重新对包中的每个二进制文件进行签名：
```bash
xattr -cr "Video Duplicate Finder.app"
codesign --force --deep --sign - "Video Duplicate Finder.app"
```

---

# 命令行界面 (CLI)

CLI 对脚本编写、计划任务和没有显示的无头服务器很有用。

### 系统要求

与 GUI 相同：FFmpeg 和 FFprobe 必须在您的 `PATH` 中或与 `vdf-cli` 二进制文件在同一目录中。

### 安装

从[发布页面](https://github.com/0x90d/videoduplicatefinder/releases/tag/3.0.x)下载 `CLI-<platform>` 并解压。

在 Linux/macOS 上，使二进制文件可执行：
```bash
chmod +x vdf-cli
```

### 使用

#### 一步扫描和比较
```bash
vdf-cli scan-and-compare --include /path/to/media
```

#### 扫描多个目录，将结果保存为 JSON
```bash
vdf-cli scan-and-compare \
  --include /mnt/movies \
  --include /mnt/series \
  --exclude /mnt/movies/extras \
  --format json \
  --output results.json
```

#### 常用选项
| 标志 | 说明 | 默认值 |
|------|-------------|---------|
| `--include <path>` | 要扫描的目录（可重复） | 必需 |
| `--exclude <path>` | 要排除的目录（可重复） | — |
| `--threshold <n>` | 哈希差异阈值 | 5 |
| `--percent <n>` | 报告的最小相似度 % | 96 |
| `--parallelism <n>` | 并行哈希线程数 | 1 |
| `--include-images` | 也扫描图像文件 | 关闭 |
| `--use-phash` | 使用感知哈希 | 关闭 |
| `--partial-clip-detection` | 启用部分片段检测（音频指纹） | 关闭 |
| `--partial-clip-min-ratio <n>` | 最小片段/源时长比 (0.0–1.0) | 0.10 |
| `--partial-clip-similarity <n>` | 最小音频指纹相似度 (0.0–1.0) | 0.80 |
| `--format json\|text\|csv` | 输出格式 | text |
| `--output <file>` | 将结果写入文件而不是 stdout | stdout |
| `--settings <file>` | 从 JSON 文件加载完整设置 | — |

#### 自动标记和删除重复文件
```bash
# 模拟运行 — 显示将删除的内容，不做任何更改（默认）
vdf-cli scan-and-compare --include /mnt/media --action lowest-quality --dry-run

# 移动到回收站（更安全）
vdf-cli scan-and-compare --include /mnt/media --action lowest-quality --delete

# 永久删除（谨慎使用）
vdf-cli scan-and-compare --include /mnt/media --action lowest-quality --delete-permanent
```

可用的 `--action` 策略：

| 策略 | 保留 |
|----------|-------|
| `lowest-quality` | 每组最高比特率/分辨率 |
| `smallest-file` | 每组最大文件 |
| `shortest-duration` | 每组最长时长 |
| `worst-resolution` | 每组最高分辨率 |
| `100-percent-only` | 仅作用于 100% 相同的组 |

> **注意：** 不建议自动删除。始终先用 `--dry-run` 查看结果。

---

# Web UI

Web UI 作为本地 Web 服务器运行，通过浏览器访问。它专为无头机器、NAS 设备和远程管理而设计。

> **安全提示：** Web UI 受密码保护，但仅供本地/Docker 使用。不要将其暴露到互联网。

### 身份认证

首次启动时，会生成一个随机密码并打印到控制台：

```
============================================
  Web UI 密码:  aB3xK9mQ7p
============================================
```

在浏览器中输入此密码登录。"记住我"Cookie 将使您保持登录 30 天。

**Docker 用户：** 运行 `docker logs vdf-web` 查看密码。

| 环境变量 | 说明 |
|---------------------|-------------|
| `VDF_WEB_PASSWORD` | 设置您自己的密码而不是自动生成的密码 |
| `VDF_WEB_AUTH=false` | 完全禁用身份认证 |

### 系统要求

FFmpeg 和 FFprobe 是必需的。在 Docker 外运行时，VDF.Web 将在首次启动时尝试自动下载它们。您也可以通过系统包管理器手动安装。

### 安装（自包含的存档）

从[发布页面](https://github.com/0x90d/videoduplicatefinder/releases/tag/3.0.x)下载 `Web-<platform>` 并解压。

在 Linux/macOS 上：
```bash
chmod +x VDF.Web
./VDF.Web
```

在 Windows 上：
```
VDF.Web.exe
```

然后在浏览器中打开 **http://localhost:5000** 并输入控制台中显示的密码。

更改端口：
```bash
ASPNETCORE_URLS=http://+:8080 ./VDF.Web
```

设置和扫描数据库保存到：
- Windows: `%APPDATA%\VDF\`
- Linux: `~/.config/VDF/`
- macOS: `~/Library/Preferences/VDF/`

---

# Docker (Web UI)

Docker 是在 NAS、家庭服务器或任何 Linux 机器上运行 Web UI 的最简单方法。镜像中包含 FFmpeg — 无需单独安装。

### 系统要求

- [Docker](https://docs.docker.com/get-docker/) 已安装

### 快速开始

```bash
docker run -d \
  --name vdf-web \
  -p 8080:8080 \
  -v vdf-db:/root/.config/VDF \
  -v vdf-state:/root/.local/state/VDF \
  -v /path/to/your/media:/media:ro \
  ghcr.io/0x90d/vdf-web:latest
```

然后在浏览器中打开 **http://localhost:8080**。
检查密码：`docker logs vdf-web` 并输入登录。
在 Web UI 中，添加 `/media`（或您挂载的任何路径）作为扫描目录。

设置您自己的密码：
```bash
docker run -d \
  --name vdf-web \
  -p 8080:8080 \
  -e VDF_WEB_PASSWORD=mysecretpassword \
  -v vdf-db:/root/.config/VDF \
  -v vdf-state:/root/.local/state/VDF \
  -v /path/to/your/media:/media:ro \
  ghcr.io/0x90d/vdf-web:latest
```

### docker compose（推荐用于永久安装）

1. 从此存储库下载 [`docker-compose.yml`](docker-compose.yml)。

2. 编辑文件并添加您的媒体卷挂载。可选设置您自己的密码：
```yaml
environment:
  - VDF_WEB_PASSWORD=mysecretpassword    # 可选 — 否则检查 docker logs
volumes:
  - /mnt/nas/movies:/mnt/nas/movies:ro
  - /mnt/nas/series:/mnt/nas/series:ro
```

3. 启动服务：
```bash
docker compose up -d
```

4. 在浏览器中打开 **http://localhost:8080** 并输入密码（如果未设置，检查 `docker logs`）。

5. 更新到最新镜像：
```bash
docker compose pull && docker compose up -d
```

### 卷参考

| 卷 | 用途 |
|--------|---------|
| `/root/.config/VDF` | 设置（`web-settings.json`）和登录凭证 — 挂载一个命名卷以便配置在容器更新时保持 |
| `/root/.local/state/VDF` | 扫描数据库（`ScannedFiles.db`） — 挂载一个命名卷以便哈希文件数据在容器更新时保持 |
| 您的媒体路径 | 挂载您要扫描的每个媒体目录。建议使用只读（`:ro`）。 |

### 注意

- 容器镜像为 `linux/amd64` 和 `linux/arm64`（树莓派 / NAS ARM 板）构建。
- 镜像发布到 [GitHub 容器注册表](https://github.com/0x90d/videoduplicatefinder/pkgs/container/vdf-web)，在每次提交时自动更新。

---

# 截图（过时）
<img src="https://user-images.githubusercontent.com/46010672/129763067-8855a538-4a4f-4831-ac42-938eae9343bd.png" width="510">

# 许可证
视频重复查找器根据 AGPLv3 许可证授权。

# 鸣谢 / 第三方库
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [ActiPro Avalonia Controls (Free Edition)](https://github.com/Actipro/Avalonia-Controls)
- [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen)
- [protobuf-net](https://github.com/protobuf-net/protobuf-net)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [AcoustID.NET by wo80](https://github.com/wo80/AcoustID.NET) — 用于部分片段检测的音频指纹识别管道（Chromaprint 风格的色度提取、FIR 平滑和指纹编码）

# 编译要求
- .NET 9.x
- 推荐 Visual Studio 2022 或更新版本

# 贡献指南
- 为每个添加或修复创建一个 pull request — 不要将它们合并为一个 PR
- 除非涉及现有 issue，否则在 pull request 中说明其作用
- 对于较大的 PR，先开启 issue 进行讨论
