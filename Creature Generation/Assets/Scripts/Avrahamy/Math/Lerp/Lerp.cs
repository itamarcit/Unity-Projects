namespace Avrahamy.Math {
    public abstract class Lerp<T> {
        protected T from;
        protected T to;
        protected readonly TimeLerp timeLerp = new TimeLerp();

        public bool HasReachedTarget {
            get {
                return timeLerp.HasReachedTarget;
            }
        }

        protected Lerp() {
            // Empty on purpose.
        }

        /// <summary>
        /// Don't use this at field initializers as it will crash Unity.
        /// </summary>
        /// <param name="unscaled">If set to <c>true</c> use unscaled time.</param>
        protected Lerp(T from, T to, float duration, bool unscaled = true) {
            Reset(from, to, duration, unscaled);
        }

        public void Stop() {
            timeLerp.Stop();
        }

        public void Reset(T from, T to, float duration, bool unscaled = true) {
            this.from = from;
            this.to = to;
            timeLerp.Reset(duration, unscaled);
        }

        public abstract T GetValue();
    }
}
