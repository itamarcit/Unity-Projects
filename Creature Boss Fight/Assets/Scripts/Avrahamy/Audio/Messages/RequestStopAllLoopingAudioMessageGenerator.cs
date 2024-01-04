using UnityEngine;
using Avrahamy.Messages;

namespace Avrahamy.Audio {
    [CreateAssetMenu(menuName = "Avrahamy/Audio/Messages/Request Stop All Looping Audio", fileName = "RequestStopAllLoopingAudioMessage")]
    public class RequestStopAllLoopingAudioMessageGenerator : MessageGeneratorNoParams<RequestStopAllLoopingAudioMessage> {}
}