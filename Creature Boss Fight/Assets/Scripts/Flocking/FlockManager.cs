using System.Collections.Generic;
using System.Linq;
using DissolveShader;
using UnityEngine;

namespace Flocking
{
    public class FlockManager : MonoBehaviour
    {
        private List<PeepModel> _peepModels = new();
        private List<DissolveController> dissolveControllers = new();
        private List<PeepHitController> _peepHitControllers = new();

        private void Awake()
        {
            PeepModel[] peepsInFlock = transform.GetComponentsInChildren<PeepModel>();
            _peepModels.AddRange(peepsInFlock);
            PeepHitController[] peepGetHits = transform.GetComponentsInChildren<PeepHitController>();
            _peepHitControllers.AddRange(peepGetHits);
            DissolveController[] peepsDissolve = transform.GetComponentsInChildren<DissolveController>();
            dissolveControllers.AddRange(peepsDissolve);
        }
    
        private void OnEnable()
        {
            for (int i = 0; i < _peepModels.Count; i++)
            {
                dissolveControllers[i].SetVisibility(true);
                if(!_peepModels[i].gameObject.activeSelf) _peepModels[i].gameObject.SetActive(true);
                if(!_peepHitControllers[i].gameObject.activeSelf) _peepHitControllers[i].gameObject.SetActive(true);
            }
        }

        public List<PeepModel> GetPeepModels()
        {
            return _peepModels;
        }

        public bool AreAllPeepsHit() // Returns true if all peeps are hit, false otherwise.
        {
            return _peepHitControllers.All(peepHitController => peepHitController.IsHit());
        }
    }
}
