namespace Avrahamy.Audio {
    public class GetAudioSourceMessage {
        private static readonly GetAudioSourceMessage instance = new GetAudioSourceMessage();
        private readonly int track;
        private ExtendedAudioSource source;

        public int Track {
            get {
                return track;
            }
        }

        public ExtendedAudioSource AudioSource {
            get {
                return source;
            }
            set {
                source = value;
            }
        }

        public static GetAudioSourceMessage Instance {
            get {
                instance.source = null;
                return instance;
            }
        }

        public GetAudioSourceMessage() {}

        public GetAudioSourceMessage(int track) {
            this.track = track;
        }
    }
}
