# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Use China apt mirror for build stage
RUN sed -i 's|deb.debian.org|mirrors.aliyun.com|g' /etc/apt/sources.list.d/debian.sources 2>/dev/null || \
    sed -i 's|deb.debian.org|mirrors.aliyun.com|g' /etc/apt/sources.list 2>/dev/null || true

WORKDIR /src
COPY ["VDF.Core/VDF.Core.csproj", "VDF.Core/"]
COPY ["VDF.Web/VDF.Web.csproj", "VDF.Web/"]
COPY ["NuGet.Config", "."]
RUN dotnet restore "VDF.Web/VDF.Web.csproj"
COPY VDF.Core/ VDF.Core/
COPY VDF.Web/ VDF.Web/
RUN dotnet publish "VDF.Web/VDF.Web.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Use China apt mirror for runtime stage
RUN sed -i 's|deb.debian.org|mirrors.aliyun.com|g' /etc/apt/sources.list.d/debian.sources 2>/dev/null || \
    sed -i 's|deb.debian.org|mirrors.aliyun.com|g' /etc/apt/sources.list 2>/dev/null || true

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl xz-utils ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install VAAPI libraries and Intel GPU drivers from Debian Trixie (testing).
# The fnOS host runs kernel 6.18 which is too new for the bookworm libva/iHD
# (2.17/23.1.1).  Trixie provides libva 2.22 + intel-media-va-driver 24.3
# which properly supports Alder Lake-N (N100/N150) on modern kernels.
# This only affects the container — the host OS is not modified.
RUN echo "deb http://mirrors.aliyun.com/debian testing main" > /etc/apt/sources.list.d/trixie.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
    -t testing libva2 libva-drm2 intel-media-va-driver mesa-va-drivers \
    && rm -rf /var/lib/apt/lists/* /etc/apt/sources.list.d/trixie.list

# Install BtbN FFmpeg GPL shared build — the shared library versions (.so)
# match the FFmpeg.AutoGen 8.0.0 binding expectations (e.g. libavcodec.so.62).
# This enables native FFmpeg binding support in Docker (no version mismatch).
#
# Option A: Place the pre-downloaded tar.xz in VDF.Web/ffmpeg-local.tar.xz
#            (faster for China network — download from:
#             https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-linux64-gpl-shared.tar.xz )
# Option B: Let the build download it automatically (requires GitHub access)
ARG TARGETARCH

# Try local file first, fall back to download
COPY VDF.Web/ffmpeg-local.tar.xz* /tmp/
RUN mkdir -p /app/bin /app/lib \
    && if [ -f /tmp/ffmpeg-local.tar.xz ]; then \
    echo "Using local FFmpeg archive"; \
    cp /tmp/ffmpeg-local.tar.xz /tmp/ffmpeg.tar.xz; \
    else \
    case "${TARGETARCH}" in \
    amd64)  FFMPEG_ARCH=linux64 ;; \
    arm64)  FFMPEG_ARCH=linuxarm64 ;; \
    *)      echo "Unsupported architecture: ${TARGETARCH}" && exit 1 ;; \
    esac; \
    echo "Downloading FFmpeg master-latest for ${FFMPEG_ARCH}..."; \
    curl -fSL -o /tmp/ffmpeg.tar.xz \
    "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-${FFMPEG_ARCH}-gpl-shared.tar.xz"; \
    fi \
    && tar -xJf /tmp/ffmpeg.tar.xz -C /tmp \
    && FFMPEG_DIR=$(find /tmp -maxdepth 1 -type d -name "ffmpeg-*" | head -1) \
    && if [ -z "$FFMPEG_DIR" ]; then echo "ERROR: FFmpeg archive extraction failed" && exit 1; fi \
    && cp "${FFMPEG_DIR}/bin/ffmpeg" /app/bin/ \
    && cp "${FFMPEG_DIR}/bin/ffprobe" /app/bin/ \
    && cp "${FFMPEG_DIR}/lib/"*.so.* /app/lib/ \
    && chmod +x /app/bin/ffmpeg /app/bin/ffprobe \
    && rm -rf /tmp/ffmpeg.tar.xz ${FFMPEG_DIR}

# Ensure the runtime linker can find the BtbN shared libraries
ENV LD_LIBRARY_PATH=/app/lib:${LD_LIBRARY_PATH}

# Add /app/bin to PATH so FFToolsUtils.GetPath() can find ffmpeg/ffprobe
# (CoreUtils.CurrentFolder may point to the dotnet host dir, not /app)
ENV PATH=/app/bin:${PATH}

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VDF.Web.dll"]
