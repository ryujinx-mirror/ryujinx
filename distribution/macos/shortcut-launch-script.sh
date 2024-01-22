#!/bin/sh
launch_arch="$(uname -m)"
if [ "$(sysctl -in sysctl.proc_translated)" = "1" ]
then
    launch_arch="arm64"
fi

arch -$launch_arch {0} {1}
