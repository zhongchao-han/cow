using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }
    public int Hour { get; private set; }
    public int Minute { get; private set; }

    float timeCounter = 0f;
    public float minuteLength = 1f; // 1秒=1游戏分钟

    private void Awake()
    {
        // 保证只有一个实例存在
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 场景里重复就销毁多余的
            return;
        }
        Instance = this;
        Hour = 9; Minute = 0;
        // DontDestroyOnLoad(gameObject); // 如果想切场景不丢失，就加上
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        timeCounter += Time.deltaTime;
        if (timeCounter > minuteLength)
        {
            Minute++;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour = (Hour + 1) % 24;
            }
            timeCounter = 0f;
        }
    }
}
