# 飞牛NAS 部署指南

## 🎯 快速开始（推荐：3分钟搞定）

### 方式一：一键自动部署（最简单）⭐

#### 步骤 1️⃣：SSH 连接到飞牛NAS
```bash
ssh root@[NAS_IP地址]
# 输入密码
```

#### 步骤 2️⃣：下载并运行部署脚本
```bash
cd /tmp
wget https://raw.githubusercontent.com/Dong-learn9/videoduplicatefinder/master/deploy-fnos.sh
chmod +x deploy-fnos.sh
bash deploy-fnos.sh
```

脚本会自动引导您：
- ✅ 检查 Docker 环境
- ✅ 创建必要的目录结构
- ✅ 配置 Web UI 密码
- ✅ 配置媒体目录路径
- ✅ 配置服务端口
- ✅ 启动容器
- ✅ 显示完整访问信息

#### 步骤 3️⃣：打开浏览器访问
```
http://[NAS_IP]:8080
```

输入密码（您在脚本中设置的密码）

---

## 📋 方式二：手动部署

### 步骤 1️⃣：准备工作
```bash
# SSH 连接到 NAS
ssh root@[NAS_IP]

# 创建目录结构
mkdir -p /opt/vdf/data/db
mkdir -p /opt/vdf/data/state
mkdir -p /opt/vdf/logs
chmod -R 777 /opt/vdf
```

### 步骤 2️⃣：下载配置文件
```bash
cd /opt/vdf
wget https://raw.githubusercontent.com/Dong-learn9/videoduplicatefinder/master/docker-compose.fnos.yml -O docker-compose.yml
```

### 步骤 3️⃣：修改配置（重要！）
```bash
nano docker-compose.yml
```

**需要修改的地方：**

1. **修改密码：**
```yaml
environment:
  - VDF_WEB_PASSWORD=YOUR_PASSWORD  # 改为您的密码
```

2. **修改媒体目录：**
```yaml
volumes:
  # 改为您实际的媒体路径
  - /mnt/md0/movies:/media/movies:ro
  - /mnt/md0/tv:/media/tv:ro
```

**常见飞牛NAS路径：**

| 存储类型 | 路径 |
|---------|------|
| RAID 存储（推荐） | `/mnt/md0/` |
| 单盘存储 | `/mnt/mnt0/` |
| USB 存储 | `/mnt/usb0/` |
| USB 存储 | `/mnt/usb1/` |

### 步骤 4️⃣：启动容器
```bash
cd /opt/vdf
docker-compose up -d
```

### 步骤 5️⃣：验证部署
```bash
# 查看容器状态（应显示 Up）
docker-compose ps

# 查看日志和初始密码
docker-compose logs vdf-web
```

---

## 🔧 配置详解

### 环境变量说明

| 变量 | 说明 | 示例 | 必需 |
|------|------|------|------|
| `VDF_WEB_PASSWORD` | Web UI 访问密码 | `123456` | ✅ |
| `ASPNETCORE_URLS` | 服务端口绑定 | `http://+:8080` | ✅ |
| `TZ` | 时区 | `Asia/Shanghai` | ❌ |
| `LANG` | 语言编码 | `zh_CN.UTF-8` | ❌ |

### 媒体目录挂载方式

**方式1：单个目录（推荐用于小规模）**
```yaml
volumes:
  - /mnt/md0/movies:/media:ro
```

**方式2：多个子目录（推荐用于大规模）**
```yaml
volumes:
  - /mnt/md0/movies:/media/movies:ro
  - /mnt/md0/tv:/media/tv:ro
  - /mnt/md0/downloads:/media/downloads:ro
```

**方式3：整个存储池**
```yaml
volumes:
  - /mnt/md0:/media:ro
```

### 端口配置

如果 8080 被占用，修改 `docker-compose.yml`：

```yaml
ports:
  - "8888:8080"  # 使用 8888 端口访问
```

然后访问：`http://[NAS_IP]:8888`

---

## 📊 飞牛NAS 特定操作

### 查询 NAS IP 地址

**方法1：从 SSH 查询**
```bash
hostname -I
```

**方法2：从管理界面**
- 打开飞牛NAS管理页面
- 点击"系统"或"网络设置"
- 查看 IP 地址

### 检查存储位置
```bash
# 查看所有挂载的存储
df -h

# 查看 RAID 状态
cat /proc/mdstat

# 查看具体存储内容
ls -la /mnt/md0/
ls -la /mnt/mnt0/
```

### 检查磁盘空间
```bash
# 详细显示所有分区
df -h

# 查看 /opt/vdf 占用的空间
du -sh /opt/vdf/*

# 查看容器占用的空间
docker system df
```

---

## 🚀 常用操作命令

### 查看服务运行状态
```bash
cd /opt/vdf
docker-compose ps
```

**正常输出示例：**
```
NAME        COMMAND                  STATUS
vdf-web     "dotnet VDF.Web.dll"     Up 5 minutes
```

### 查看实时日志
```bash
cd /opt/vdf
docker-compose logs -f vdf-web

# 查看最后100行
docker-compose logs --tail=100 vdf-web
```

### 重启服务
```bash
cd /opt/vdf
docker-compose restart
```

### 停止服务（保留数据）
```bash
cd /opt/vdf
docker-compose down
```

### 完全删除（包括数据，谨慎！）
```bash
cd /opt/vdf
docker-compose down -v
rm -rf /opt/vdf/data/*
```

### 查看容器日志中的密码
```bash
cd /opt/vdf
docker-compose logs | grep -i password
```

### 更新到最新版本
```bash
cd /opt/vdf
docker-compose pull
docker-compose up -d
```

---

## 📱 Web UI 使用指南

### 首次访问

1. 打开浏览器，访问 `http://[NAS_IP]:8080`
2. 输入密码（自动生成或您自己设置的）
3. 进入仪表板

### 添加扫描目录

1. 点击 **"Settings"**（设置）
2. 在 **"Search Directories"** 部分添加目录
3. 建议添加：`/media` 或 `/media/movies`
4. 点击 **"Save"** 保存

### 开始扫描

1. 返回主页，点击 **"Scan"** 按钮
2. 等待扫描完成（会显示进度）
3. 扫描完成后自动显示结果

### 查看和管理结果

1. 点击 **"Results"** 查看找到的重复文件
2. 预览重复文件的缩略图
3. 选择要删除的文件
4. 点击"Delete"删除

### 导出结果

1. 在结果页面点击 **"Export Results"**
2. 选择导出格式：JSON、CSV 等
3. 下载文件

---

## 🆘 故障排查

### 问题 1️⃣：无法连接服务

**症状：** 浏览器无法打开 `http://NAS_IP:8080`

**解决方案：**
```bash
# 检查容器是否运行
docker ps | grep vdf-web

# 检查端口是否开放
netstat -tuln | grep 8080

# 检查防火墙规则
iptables -L | grep 8080

# 重启容器
docker-compose restart
```

### 问题 2️⃣：权限错误

**症状：** 日志中显示 "Permission denied"

**解决方案：**
```bash
# 修复权限
chmod -R 777 /opt/vdf
chmod -R 777 /mnt/md0  # 如果是媒体目录权限问题

# 重启容器
docker-compose restart
```

### 问题 3️⃣：磁盘空间不足

**症状：** 扫描失败，显示磁盘满

**解决方案：**
```bash
# 检查磁盘使用
df -h

# 清理 Docker 临时文件
docker system prune -a

# 查看 Docker 占用空间
docker system df
```

### 问题 4️⃣：无法访问媒体文件

**症状：** 扫描时找不到媒体文件

**解决方案：**
```bash
# 确认媒体目录存在
ls -la /mnt/md0/movies

# 检查目录权限
stat /mnt/md0/movies

# 查看 docker-compose.yml 中的挂载配置是否正确
cat /opt/vdf/docker-compose.yml | grep -A5 "volumes:"
```

### 问题 5️⃣：忘记密码

**解决方案：**
```bash
# 查看初始密码
docker-compose logs | grep -i password

# 或修改 docker-compose.yml 设置新密码
nano docker-compose.yml
# 改变 VDF_WEB_PASSWORD 的值

# 重启容器使新密码生效
docker-compose restart
```

---

## 💾 数据备份与恢复

### 备份数据
```bash
# 备份所有配置和数据库
tar -czf vdf-backup-$(date +%Y%m%d-%H%M%S).tar.gz /opt/vdf/data/

# 将备份复制到其他位置
cp vdf-backup-*.tar.gz /mnt/md0/backups/
```

### 恢复数据
```bash
# 停止容器
cd /opt/vdf && docker-compose down

# 恢复备份
tar -xzf vdf-backup-*.tar.gz -C /

# 重启容器
cd /opt/vdf && docker-compose up -d
```

---

## 🔐 安全建议

### 修改默认密码
```yaml
environment:
  - VDF_WEB_PASSWORD=your_strong_password_here
```

### 限制访问 IP（可选但推荐）
```bash
# 仅允许本地网络访问
iptables -A INPUT -p tcp --dport 8080 -s 192.168.1.0/24 -j ACCEPT
iptables -A INPUT -p tcp --dport 8080 -j DROP

# 保存规则
iptables-save > /etc/iptables/rules.v4
```

### 定期更新
```bash
# 检查更新
docker-compose pull

# 如有更新，重启容器
docker-compose up -d
```

---

## 📊 性能优化

### 调整资源限制
编辑 `docker-compose.yml`：

```yaml
deploy:
  resources:
    limits:
      cpus: '8'      # 根据 NAS CPU 核心数调整
      memory: 8G     # 根据 NAS RAM 调整
```

### 提高扫描并行度
在 Web UI 的 Settings 中：
- **Max Degree of Parallelism** - 设置为 CPU 核心数

---

## 📞 获得帮助

如遇到问题，请按以下步骤获得帮助：

1. **查看日志：**
   ```bash
   docker-compose logs -f vdf-web
   ```

2. **查看官方文档：**
   - [项目首页](https://github.com/0x90d/videoduplicatefinder)
   - [中文版仓库](https://github.com/Dong-learn9/videoduplicatefinder)

3. **提交 Issue：**
   - 在 GitHub 中提交详细的问题描述
   - 包含日志信息和配置细节

---

## ✨ 完整功能列表

您现在拥有的是完整的中文版本，包括：

- ✅ **Web UI** - 浏览器界面，完全中文化
- ✅ **CLI 工具** - 命令行工具，自动中文检测
- ✅ **GUI 应用** - 桌面应用（Windows/Linux/macOS）
- ✅ **多格式支持** - 视频、图像识别
- ✅ **智能去重** - 支持转码、格式转换检测
- ✅ **音频指纹** - 部分片段检测
- ✅ **Docker 部署** - 一键部署

---

## 🎉 部署完成！

现在您可以：

1. 🎬 **扫描所有视频和图像**
2. 🔍 **找出完全相同或相似的文件**
3. 🗑️ **安全地删除重复文件**
4. 📊 **导出扫描结果**
5. 🌍 **远程管理 NAS**

祝您使用愉快！
