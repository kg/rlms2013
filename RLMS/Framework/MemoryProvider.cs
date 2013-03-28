using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLMS.Framework.Framework {
    public class MemoryProvider {
        interface IValueContainer {
        }

        private class ValueContainer<T> : IValueContainer {
            public T Value;
        }

        public struct Slot<T> {
            private readonly MemoryProvider Provider;
            private readonly T DefaultValue;
            public readonly string Key;

            public bool TryGet (out T value) {
                IValueContainer temp;
                var result = Provider.Values.TryGetValue(Key, out temp);

                if (result)
                    value = ((ValueContainer<T>)temp).Value;
                else
                    value = DefaultValue;

                return result;
            }

            public void Set (T value) {
                IValueContainer temp;
                if (!Provider.Values.TryGetValue(Key, out temp))
                    Provider.Values[Key] = new ValueContainer<T> { Value = value };
                else
                    ((ValueContainer<T>)temp).Value = value;
            }

            public Slot (MemoryProvider provider, string key, T defaultValue) {
                Provider = provider;
                Key = key;
                DefaultValue = defaultValue;
            }

            public static implicit operator T (Slot<T> @this) {
                T temp;
                @this.TryGet(out temp);
                return temp;
            }
        }

        private readonly Dictionary<string, IValueContainer> Values = new Dictionary<string, IValueContainer>();

        public Slot<T> GetSlot<T> (string key, T defaultValue) {
            return new Slot<T>(this, key, defaultValue);
        }
    }
}
