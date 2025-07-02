from PIL import Image
import os

def mirror_frames_by_index(img_path, out_path, frame_indices, rows=2, cols=2):
    """
    将指定帧编号的区域进行水平镜像，并保存新图。
    :param img_path: 输入图片路径
    :param out_path: 输出图片路径
    :param frame_indices: 要镜像的帧编号（从 1 开始）
    :param rows: 行数（默认2）
    :param cols: 列数（默认2）
    """
    img = Image.open(img_path).convert("RGBA")
    total_width, total_height = img.size

    frame_w = total_width // cols
    frame_h = total_height // rows

    for index in frame_indices:
        if index < 1 or index > rows * cols:
            print(f"⚠️ 帧号 {index} 超出范围，跳过")
            continue

        row = (index - 1) // cols
        col = (index - 1) % cols

        x1 = col * frame_w
        y1 = row * frame_h
        x2 = x1 + frame_w
        y2 = y1 + frame_h

        region = img.crop((x1, y1, x2, y2))
        flipped = region.transpose(Image.FLIP_LEFT_RIGHT)
        img.paste(flipped, (x1, y1))

    # 如果文件存在则先删除
    if os.path.exists(out_path):
        os.remove(out_path)

    img.save(out_path)
    print(f"✅ 镜像帧 {frame_indices} 已保存为：{out_path}")

# 示例用法：
mirror_frames_by_index(
    img_path=r"D:\Users\hanerlv\Downloads\ChatGPT Image 2025年7月1日 20_45_37.png",
    out_path=r"D:\Users\hanerlv\Downloads\Cow_Idle.png",
    frame_indices=[2, 4],  # 想翻转的帧编号（从1开始）
    rows=2,
    cols=2
)
