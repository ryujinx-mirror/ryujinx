import argparse
from io import BytesIO
import tarfile

parser = argparse.ArgumentParser(
    description="Add the main binary to a tar and force it to be executable"
)
parser.add_argument("input_tar_file", help="input tar file")
parser.add_argument("main_binary_path", help="Main executable path")
parser.add_argument("main_binary_tar_path", help="Main executable tar path")

args = parser.parse_args()
input_tar_file = args.input_tar_file
main_binary_path = args.main_binary_path
main_binary_tar_path = args.main_binary_tar_path

with open(main_binary_path, "rb") as f:
    with tarfile.open(input_tar_file, "a") as tar:
        data = f.read()
        tar_info = tarfile.TarInfo(main_binary_tar_path)
        tar_info.mode = 0o755
        tar_info.size = len(data)

        tar.addfile(tar_info, BytesIO(data))
