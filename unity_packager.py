import os
import zipfile

# 需要包含的文件扩展名
INCLUDE_EXTS = [
    '.cs', '.uxml', '.uss', '.json', '.asset', '.png', '.jpg', '.jpeg', '.wav', '.mp3'
]
# 需要包含的目录
INCLUDE_DIRS = ['Assets', 'ProjectSettings', 'Packages']

# 排除的目录
EXCLUDE_DIRS = [
    'Library', 'Temp', 'Obj', 'Build', 'Builds', 'Logs', '.vs', '.vscode', '.git'
]

# 打包输出文件名
OUTPUT_ZIP = 'unity_upload_pack.zip'

def should_include_file(filepath):
    # 判断文件是否应该被包含
    ext = os.path.splitext(filepath)[1].lower()
    if ext in INCLUDE_EXTS:
        return True
    # 单独包含部分配置文件
    if filepath.endswith('Packages/manifest.json') or filepath.endswith('ProjectVersion.txt'):
        return True
    return False

def is_excluded_dir(path):
    for ex in EXCLUDE_DIRS:
        if os.path.sep + ex + os.path.sep in path or path.startswith(ex + os.path.sep):
            return True
    return False

def main():
    root = os.getcwd()
    with zipfile.ZipFile(OUTPUT_ZIP, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for inc_dir in INCLUDE_DIRS:
            inc_path = os.path.join(root, inc_dir)
            if not os.path.exists(inc_path):
                continue
            for dirpath, dirnames, filenames in os.walk(inc_path):
                # 过滤掉不需要的目录
                if is_excluded_dir(os.path.relpath(dirpath, root) + os.path.sep):
                    dirnames[:] = []  # 不遍历子目录
                    continue
                for filename in filenames:
                    rel_dir = os.path.relpath(dirpath, root)
                    rel_file = os.path.join(rel_dir, filename)
                    if should_include_file(rel_file):
                        zipf.write(os.path.join(root, rel_file), rel_file)
    print(f'打包完成，文件已保存为 {OUTPUT_ZIP}')

if __name__ == '__main__':
    main()
