using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    [CreateAssetMenu]
    public class ExerciseList : ScriptableObject, IList<Exercise>
    {
        [SerializeField]
        private Exercise[] entries;

        public Exercise this[int index] { get => ((IList<Exercise>)entries)[index]; set => ((IList<Exercise>)entries)[index] = value; }

        public int Count => ((ICollection<Exercise>)entries).Count;

        public bool IsReadOnly => ((ICollection<Exercise>)entries).IsReadOnly;

        public void Add(Exercise item)
        {
            ((ICollection<Exercise>)entries).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Exercise>)entries).Clear();
        }

        public bool Contains(Exercise item)
        {
            return ((ICollection<Exercise>)entries).Contains(item);
        }

        public void CopyTo(Exercise[] array, int arrayIndex)
        {
            ((ICollection<Exercise>)entries).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Exercise> GetEnumerator()
        {
            return ((IEnumerable<Exercise>)entries).GetEnumerator();
        }

        public int IndexOf(Exercise item)
        {
            return ((IList<Exercise>)entries).IndexOf(item);
        }

        public void Insert(int index, Exercise item)
        {
            ((IList<Exercise>)entries).Insert(index, item);
        }

        public bool Remove(Exercise item)
        {
            return ((ICollection<Exercise>)entries).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Exercise>)entries).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }
    }

    [System.Serializable]
    public class LocalizedExerciseList : LocalizedAsset<ExerciseList>
    {

    }
}