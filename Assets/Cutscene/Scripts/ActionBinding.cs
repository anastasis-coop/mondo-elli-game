using System.Collections;

namespace Cutscene
{
    public abstract class ActionBinding : Binding
    {
        public abstract IEnumerator Execute(CutscenePlayer player);
    }
}