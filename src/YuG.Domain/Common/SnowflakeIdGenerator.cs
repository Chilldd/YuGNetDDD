namespace YuG.Domain.Common;

/// <summary>
/// 雪花ID生成器（线程安全），用于生成分布式唯一 ID
/// </summary>
public class SnowflakeIdGenerator
{
    private readonly long _workerId;
    private long _lastTimestamp = -1L;
    private long _sequence;
    private readonly object _lock = new();

    // 自定义 Epoch（2024-01-01），允许使用到 2084 年
    private const long TwEpoch = 1704067200000L;

    private const int WorkerIdBits = 10;
    private const int SequenceBits = 12;
    private const int WorkerIdShift = SequenceBits;
    private const int TimestampShift = SequenceBits + WorkerIdBits;
    private const long SequenceMask = (1L << SequenceBits) - 1;
    private const long MaxWorkerId = (1L << WorkerIdBits) - 1;

    /// <summary>
    /// 初始化雪花ID生成器
    /// </summary>
    /// <param name="workerId">工作节点 ID（0-1023）</param>
    public SnowflakeIdGenerator(long workerId)
    {
        if (workerId < 0 || workerId > MaxWorkerId)
            throw new ArgumentException($"WorkerId 必须在 0 到 {MaxWorkerId} 之间");
        _workerId = workerId;
    }

    /// <summary>
    /// 生成下一个唯一 ID
    /// </summary>
    /// <returns>64 位长整型唯一 ID</returns>
    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = GetTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("检测到系统时钟回拨");

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    // 当前毫秒序列号耗尽，等待下一毫秒
                    while (timestamp <= _lastTimestamp)
                        timestamp = GetTimestamp();
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return ((timestamp - TwEpoch) << TimestampShift)
                   | (_workerId << WorkerIdShift)
                   | _sequence;
        }
    }

    private static long GetTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
