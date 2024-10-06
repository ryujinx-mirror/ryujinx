#!/bin/sh
set -e
set -x

ROOTDIR="$(readlink -f "$(dirname "$0")")"/../../../
cd "$ROOTDIR"

BUILDDIR=${BUILDDIR:-publish}
OUTDIR=${OUTDIR:-publish_appimage}

rm -rf AppDir
mkdir -p AppDir/usr/bin/bin

# Ensure necessary bins are set as executable
chmod +x "$BUILDDIR"/Ryujinx*

# Add symlinks for the AppImage
ln -s "$ROOTDIR"/distribution/linux/Ryujinx.desktop AppDir/Ryujinx.desktop
ln -s "$ROOTDIR"/distribution/linux/appimage/AppRun AppDir/AppRun
ln -s "$ROOTDIR"/distribution/misc/Logo.svg AppDir/Ryujinx.svg

cp -r "$BUILDDIR"/* AppDir/usr/bin/

mkdir -p "$OUTDIR"

appimagetool --comp zstd --mksquashfs-opt -Xcompression-level --mksquashfs-opt 21 \
    -u "gh-releases-zsync|$GITHUB_REPOSITORY_OWNER|Ryujinx|latest|*.AppImage.zsync" \
    AppDir "$OUTDIR"/Ryujinx.AppImage

# ??
mv ./*.AppImage.zsync "$OUTDIR"
