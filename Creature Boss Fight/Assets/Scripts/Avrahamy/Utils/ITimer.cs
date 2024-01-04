namespace Avrahamy {
    public interface ITimer {
        float StartTime { get; set; }
        float EndTime { get; set; }
        float Duration { get; set; }
        float UnscaledDuration { get; }
        float ElapsedTime { get; }
        float RemainingTime { get; }
        bool IsActive { get; }
        bool IsSet { get; }
        float Progress { get; }
        float ClampedProgress { get; }
        float TimeScale { get; set; }
        void Start();
        void Start(float duration);
        void Clear();
    }
}