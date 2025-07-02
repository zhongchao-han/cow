from PIL import Image

def remove_bg_color(img_path, out_path, bg_color=(255,255,255), tolerance=10):
    """
    把图片的指定纯色背景(bg_color)去除变成透明，保存为PNG。
    :param img_path: 原图路径
    :param out_path: 输出PNG路径
    :param bg_color: 背景色，RGB元组
    :param tolerance: 色差容忍度，默认10（允许微小色差）
    """
    img = Image.open(img_path).convert("RGBA")
    datas = img.getdata()
    new_data = []
    for item in datas:
        # 判断RGB差值在容差范围内
        if all(abs(item[i] - bg_color[i]) <= tolerance for i in range(3)):
            # 设置为透明
            new_data.append((255,255,255,0))
        else:
            new_data.append(item)
    img.putdata(new_data)
    img.save(out_path)
    print(f"已保存透明PNG到：{out_path}")

# 示例用法
remove_bg_color(r"D:\Users\hanerlv\Downloads\ChatGPT Image 2025年7月2日 14_38_57.png", 
                r"D:\Users\hanerlv\Downloads\Grass.png", 
                bg_color=(255,255,255), tolerance=10)
