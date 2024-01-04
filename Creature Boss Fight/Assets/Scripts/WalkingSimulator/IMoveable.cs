using UnityEngine;

namespace WalkingSimulator {
    public interface IMoveable {
        Vector3 Position { get; set; }
        Vector3 Velocity { get; set; }
    }
}