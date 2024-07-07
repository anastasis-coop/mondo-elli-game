using System.Collections.Generic;
using UnityEngine;

namespace Cutscene
{
    public class CutsceneActivationStep : CutsceneStep
    {
        [SerializeField]
        private bool _activate = true;

        public override void Prepare() => gameObject.SetActive(!_activate);

        public override IEnumerator<ICutsceneStep> Execute()
        {
            gameObject.SetActive(_activate);
            yield break;
        }
    }
}