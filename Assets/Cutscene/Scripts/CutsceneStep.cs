using System.Collections.Generic;
using UnityEngine;

namespace Cutscene
{
    public interface ICutsceneStep
    {
        public void Prepare();
        public IEnumerator<ICutsceneStep> Execute();

        public IEnumerator<ICutsceneStep> PrepareAndExecute()
        {
            Prepare();
            return Execute();
        }
    }

    public abstract class CutsceneStep : MonoBehaviour, ICutsceneStep
    {
        public abstract void Prepare();
        public abstract IEnumerator<ICutsceneStep> Execute();
    }
}