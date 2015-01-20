using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Utils
{
    public class ObjectList<T> where T : GameObject
    {
        public int Length { get; private set; }

        private T[] _objects;
        private int[] _indexes;
        private int[] _dead;
        private int[] _alive;

        private int _deadOffset;
        private int _aliveOffset;
        private int _indexesOffset;
        private int _clearSpace;

        public ObjectList(int length)
        {
            Length = length;
            _objects = new T[length];
            _indexes = new int[length];
            for (int i = 0; i < _indexes.Length; i++)
            {
                _indexes[i] = i;
            }
            _dead = new int[length];
            _alive = new int[length];
            _deadOffset = 0;
            _indexesOffset = 0;
            _aliveOffset = 0;
            _clearSpace = length;
        }

        public T this[int index]
        {
            get
            {
                return _objects[_indexes[index]];
            }

            set
            {
                _objects[_indexes[index]] = value;
            }
        }

        public void AddItem(T item)
        {
            if (_objects[_indexes[_indexesOffset]] != null)
            {
                if (_objects[_indexes[_indexesOffset]].Alive && _deadOffset > 0)
                    Reorder();
                if (_indexesOffset > _clearSpace && !_objects[_indexes[_indexesOffset]].Alive)
                    RemoveDead(_indexesOffset);
            }
            
            _objects[_indexes[_indexesOffset]] = item;

            if (_aliveOffset != _alive.Length)
            {
                _alive[_aliveOffset] = _indexesOffset;
                _aliveOffset++;
            }

            _indexesOffset++;
            
            if (_indexesOffset == _indexes.Length)
                _indexesOffset = 0;
        }

        private void RemoveDead(int value)
        {
            bool found = false;
            for (int i = 0; i < _deadOffset; i++)
            {
                if (_dead[i] == value)
                    found = true;
                if (found)
                    if (i != _deadOffset - 1)
                        _dead[i] = _dead[i + 1];
            }

            if (!found)
                throw new Exception("Never found the value");
            
            _deadOffset--;
        }

        private void RemoveAlive(int value)
        {
            bool found = false;
            for (int i = 0; i < _aliveOffset; i++)
            {
                if (_alive[i] == value)
                    found = true;
                if (found)
                    if (i != _aliveOffset - 1)
                        _alive[i] = _alive[i + 1];
            }
            
            if (!found)
                throw new Exception("Never found the value");
            
            _aliveOffset--;
        }

        public void KillRandom()
        {
            Random random = new Random();
            Kill(_alive[random.Next(_aliveOffset)]);
        }

        public void Kill(int id)
        {
            if (_objects[_indexes[id]].Alive)
            {
                _objects[_indexes[id]].Alive = false;
                _dead[_deadOffset] = id;
                _deadOffset++;
                RemoveAlive(id);
            }
        }

        public void Reorder()
        {
            int[] result = new int[_objects.Length];
            int offset = _deadOffset;

            _clearSpace = _deadOffset - 1;

            for (int i = 0; i < _deadOffset; i++)
            {
                result[i] = _indexes[_dead[i]];
            }
            for (int i = 0; i < _aliveOffset; i++, offset++)
            {
                result[offset] = _indexes[_alive[i]];
                _alive[i] = offset;
            }

            _indexesOffset = 0;
            _deadOffset = 0;
            _indexes = result;
        }
    }

    public class GameObject
    {
        public bool Alive { get; set; }
        public GameObject(bool alive)
        {
            Alive = alive;
        }
    }
}
