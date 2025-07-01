using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;
    public int Hour { get; private set; }
    public int Minute { get; private set; }

    float timeCounter = 0f;
    public float minuteLength = 1f; // 1秒=1游戏分钟

    void Awake()
    {
        Instance = this;
        Hour = 6; Minute = 0;
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
