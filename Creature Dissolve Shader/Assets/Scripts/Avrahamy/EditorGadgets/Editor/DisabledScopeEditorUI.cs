using UnityEngine;
using System;

namespace Avrahamy.EditorGadgets {
    /// <summary>
    /// A disposable wrapper for GUI.enable = false scope, to ensure restoring
    /// the GUI.enabled state.
    /// </summary>
    public class DisabledScopeEditorUI : IDisposable {
        private bool wasEnabled;

        public DisabledScopeEditorUI() {
            wasEnabled = GUI.enabled;
            GUI.enabled = false;
        }

        public void Dispose() {
            GUI.enabled = wasEnabled;
        }
    }
}
