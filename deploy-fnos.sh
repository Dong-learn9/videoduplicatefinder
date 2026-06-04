#!/bin/bash

# 飞牛NAS自动部署脚本
# 用法: bash deploy-fnos.sh

set -e

echo "========================================"
echo "  视频重复查找器 - 飞牛NAS部署脚本"
echo "========================================"
echo ""

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 检查Docker是否安装
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker 未安装${NC}"
    echo "请先在飞牛NAS中安装 Docker"
    exit 1
fi

echo -e "${GREEN}✓ Docker 已安装${NC}"

# 创建必要的目录
echo ""
echo "📁 创建目录结构..."
mkdir -p /opt/vdf/data/db
mkdir -p /opt/vdf/data/state
mkdir -p /opt/vdf/logs
chmod -R 777 /opt/vdf

echo -e "${GREEN}✓ 目录创建完成${NC}"

# 提示用户配置
echo ""
echo "⚙️  请配置以下信息："
echo ""

read -p "请输入 Web UI 密码 (默认: 123456): " WEB_PASSWORD
WEB_PASSWORD=${WEB_PASSWORD:-123456}

echo ""
echo "📍 媒体目录配置："
echo "常见飞牛NAS路径："
echo "  /mnt/md0/     - RAID 存储（推荐）"
echo "  /mnt/mnt0/    - 单盘存储"
echo "  /mnt/usbX/    - USB 存储"
echo ""
read -p "请输入媒体根目录 (默认: /mnt/md0): " MEDIA_PATH
MEDIA_PATH=${MEDIA_PATH:-/mnt/md0}

if [ ! -d "$MEDIA_PATH" ]; then
    echo -e "${YELLOW}⚠️  警告：目录 $MEDIA_PATH 不存在${NC}"
    read -p "是否继续? (y/n): " CONTINUE
    if [ "$CONTINUE" != "y" ]; then
        exit 1
    fi
fi

echo ""
read -p "请输入 Web 服务端口 (默认: 8080): " WEB_PORT
WEB_PORT=${WEB_PORT:-8080}

# 生成 docker-compose.yml
echo ""
echo "📝 生成 docker-compose.yml..."

cat > /opt/vdf/docker-compose.yml << EOF
version: '3.8'

services:
  vdf-web:
    image: ghcr.io/0x90d/vdf-web:latest
    container_name: vdf-web
    restart: always
    
    ports:
      - "${WEB_PORT}:8080"
    
    environment:
      - VDF_WEB_PASSWORD=${WEB_PASSWORD}
      - ASPNETCORE_URLS=http://+:8080
      - ASPNETCORE_ENVIRONMENT=Production
      - LANG=zh_CN.UTF-8
      - LC_ALL=zh_CN.UTF-8
      - TZ=Asia/Shanghai
    
    volumes:
      - vdf-db:/root/.config/VDF
      - vdf-state:/root/.local/state/VDF
      - ${MEDIA_PATH}:/media:ro
    
    deploy:
      resources:
        limits:
          cpus: '4'
          memory: 4G
        reservations:
          cpus: '2'
          memory: 2G
    
    logging:
      driver: "json-file"
      options:
        max-size: "100m"
        max-file: "5"
    
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

volumes:
  vdf-db:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /opt/vdf/data/db
  
  vdf-state:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /opt/vdf/data/state

networks:
  default:
    name: vdf-network
    driver: bridge
EOF

echo -e "${GREEN}✓ docker-compose.yml 生成完成${NC}"

# 启动容器
echo ""
echo "🚀 启动容器..."
cd /opt/vdf

if docker-compose up -d; then
    echo -e "${GREEN}✓ 容器启动成功${NC}"
else
    echo -e "${RED}❌ 容器启动失败${NC}"
    exit 1
fi

# 等待容器启动
echo ""
echo "⏳ 等待服务启动..."
sleep 5

# 检查容器状态
if docker-compose ps | grep -q "vdf-web.*Up"; then
    echo -e "${GREEN}✓ 服务运行中${NC}"
else
    echo -e "${RED}❌ 服务未启动${NC}"
    docker-compose logs
    exit 1
fi

# 显示访问信息
echo ""
echo "========================================"
echo -e "${GREEN}✓ 部署完成！${NC}"
echo "========================================"
echo ""
echo "📱 访问信息："
echo "  URL: http://[NAS_IP]:${WEB_PORT}"
echo "  密码: ${WEB_PASSWORD}"
echo ""
echo "📁 媒体路径: ${MEDIA_PATH}"
echo "📊 数据存储: /opt/vdf/data"
echo "📝 日志文件: /opt/vdf/logs"
echo ""
echo "🔧 常用命令:"
echo "  查看日志: cd /opt/vdf && docker-compose logs -f vdf-web"
echo "  停止服务: cd /opt/vdf && docker-compose down"
echo "  重启服务: cd /opt/vdf && docker-compose restart"
echo "  删除数据: cd /opt/vdf && docker-compose down -v"
echo ""
echo "========================================"
