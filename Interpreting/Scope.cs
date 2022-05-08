using Point.Lexing;

namespace Point.Interpreting
{
    public class Scope
    {
        public bool HasParent => Parent != null;
        public Scope Global => HasParent ? Parent.Global : this;
        public readonly Scope Parent;
        private readonly Dictionary<string, object> Values = new();

        public Scope()
        {
            Parent = null;
        }
        public Scope(Scope parent)
        {
            Parent = parent;
        }

        public void Define(string key, object value)
        {
            Values[key] = value;
        }
        public void Set(Token key, object value)
        {
            string keyName = key.Raw;

                // If it exists locally
                if (Has(keyName, false))
                {
                    Values[keyName] = value;
                }
                else
                {
                    // If it exists anywhere
                    if (Has(keyName, true))
                    {
                        // Recall
                        Parent.Set(key, value);
                    }
                    else
                    {
                        // We need to create it
                        Values[keyName] = value;
                    }
                }
        }
        public void Dispose(string key)
        {
            // If it exists locally
            if (Has(key, false))
            {
                Values.Remove(key);
            }
            else
            {
                // If it exists anywhere
                if (Has(key, true))
                {
                    // Recall
                    Parent.Dispose(key);
                }
                else
                {
                    // We need to report this
                    throw new RuntimeError("", "Specified key was not present, unable to be disposed");
                }
            }
        }
        public bool Has(string key, bool canCheckParent = true)
        {
            bool has = Values.ContainsKey(key);

            // Can't be found locally, has a parent to check and allowed to check parent
            if (!has && HasParent && canCheckParent)
                has = Parent.Has(key);
            return has;
        }

        public object Get(Token key)
        {
            string keyName = key.Raw;

            // If can be found locally
            if (Values.ContainsKey(keyName))
                return Values[keyName];

            // If can't be found locally and has a parent to check
            if (HasParent) return Parent.Get(key);

            //throw new RuntimeError(key, $"Undefined variable '{keyName}'"); // return null instead
            return null;
        }

        //public void SetAt(int dist, Token name, object value)
        //{
        //    Ancestor(dist).Values[name.Raw] = value;
        //}
        //public object GetAt(int dist, string name)
        //{
        //    return Ancestor(dist).Values[name];
        //}

        //public Scope Ancestor(int dist)
        //{
        //    Scope scope = this;
        //    for (int i = 0; i < dist; i++)
        //    {
        //        scope = scope.Parent;
        //    }

        //    return scope;
        //}

        public Dictionary<string, object> All()
            => Values;
    }
}
