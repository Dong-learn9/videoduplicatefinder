#!/bin/bash
# VDF-Web Docker 部署验证脚本
# 在飞牛NAS上执行: bash verify.sh

set -e

COMPOSE_FILE="docker-compose.fnos.local.yml"
PROJECT_DIR="/vol1/1000/Docker/vdf-web"

echo "========================================="
echo "  VDF-Web Docker 部署验证"
echo "========================================="
echo ""

# 1. 创建数据目录
echo "[1/6] 创建数据目录..."
sudo mkdir -p "$PROJECT_DIR/data/db" "$PROJECT_DIR/data/state" "$PROJECT_DIR/data/logs"
echo "  完成"
echo ""

# 2. 检查 GPU 设备
echo "[2/6] 检查 Intel 集显设备..."
if [ -e /dev/dri/renderD128 ]; then
    echo "  OK: /dev/dri/renderD128 存在"
    ls -la /dev/dri/
else
    echo "  警告: /dev/dri/renderD128 不存在，GPU 加速不可用"
fi
echo ""

# 3. 构建镜像
echo "[3/6] 构建 Docker 镜像（首次构建较慢，请耐心等待）..."
cd "$PROJECT_DIR"
docker compose -f "$COMPOSE_FILE" build --progress=plain 2>&1
echo "  构建完成"
echo ""

# 4. 启动容器
echo "[4/6] 启动容器..."
docker compose -f "$COMPOSE_FILE" up -d 2>&1
echo "  容器已启动"
echo ""

# 等待容器启动
echo "  等待容器初始化..."
sleep 5
echo ""

# 5. 容器内验证
echo "[5/6] 容器内环境验证..."
echo ""

echo "  --- FFmpeg 版本 ---"
docker exec vdf-web /app/bin/ffmpeg -version 2>&1 | head -3
echo ""

echo "  --- FFmpeg 硬件加速支持 ---"
docker exec vdf-web /app/bin/ffmpeg -hwaccels 2>&1
echo ""

echo "  --- VAAPI 设备检测 ---"
docker exec vdf-web ls -la /dev/dri/ 2>&1 || echo "  /dev/dri 未挂载"
echo ""

echo "  --- 共享库文件（Native Binding 所需）---"
docker exec vdf-web ls -la /app/lib/ 2>&1
echo ""

echo "  --- 共享库版本与 FFmpeg.AutoGen 绑定匹配检查 ---"
# FFmpeg.AutoGen 8.0.0 期望: libavcodec.so.62, libavformat.so.62, libavutil.so.59
for lib in libavcodec.so.62 libavformat.so.62 libavutil.so.59 libswscale.so.9 libswresample.so.5; do
    if docker exec vdf-web test -f "/app/lib/$lib" 2>/dev/null; then
        echo "  OK: $lib"
    else
        echo "  缺失: $lib"
    fi
done
echo ""

echo "  --- LD_LIBRARY_PATH ---"
docker exec vdf-web printenv LD_LIBRARY_PATH 2>&1
echo ""

echo "  --- Web UI 健康检查 ---"
sleep 3
if curl -sf http://localhost:8080/ > /dev/null 2>&1; then
    echo "  OK: Web UI 可访问"
else
    echo "  等待 Web UI 启动..."
    sleep 10
    if curl -sf http://localhost:8080/ > /dev/null 2>&1; then
        echo "  OK: Web UI 可访问"
    else
        echo "  警告: Web UI 暂不可访问，请检查日志: docker logs vdf-web"
    fi
fi
echo ""

# 6. 总结
echo "[6/6] 验证总结"
echo "========================================="
echo ""
echo "  Web UI 地址: http://$(hostname -I | awk '{print $1}'):8080"
echo ""
echo "  首次登录密码: 查看日志获取"
echo "    docker logs vdf-web 2>&1 | grep password"
echo ""
echo "  推荐设置（登录后在 Settings 页面配置）:"
echo "    Hardware acceleration: vaapi"
echo "    Use native FFmpeg binding: 勾选"
echo ""
echo "  常用命令:"
echo "    查看日志: docker logs -f vdf-web"
echo "    停止:     docker compose -f $COMPOSE_FILE down"
echo "    重建:     docker compose -f $COMPOSE_FILE up -d --build"
echo "========================================="
