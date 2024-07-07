using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Cutscene
{
    [CreateAssetMenu(fileName = "New Move Whiteboard Binding", menuName = "Cutscenes/Move Whiteboard Binding")]
    public class MoveWhiteboardBinding : ActionBinding
    {
        [SerializeField]
        private bool _up;

        public override IEnumerator Execute(CutscenePlayer player)
        {
            yield return player.ShowWhiteboard(!_up).WaitForCompletion();
        }
    }
}