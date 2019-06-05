using FAILang.Types.Unevaluated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAILang.Types
{
    public class Map : Function
    {
        private static readonly string[] PARAMS = new string[] { "name" };

        public Dictionary<IType, IType> kvMap;

        public Map(Dictionary<IType, IType> map)
            : base(PARAMS, null, null, false, false)
        {
            kvMap = map;
        }

        public Map(IType[] keys, IType[] values)
            : this(keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v))
        {

        }

        public override IType Evaluate(IType[] args)
        {
            return kvMap.TryGetValue(args[0], out IType result) ? result : Undefined.Instance;
        }
    }

    class UnevaluatedMap : IUnevaluated
    {
        public string TypeName => "UnevaluatedMap";

        public readonly IType[] keys;
        public readonly IType[] values;

        public UnevaluatedMap(IType[] keys, IType[] values)
        {
            this.keys = keys;
            this.values = values;
        }

        public IType Evaluate(Dictionary<string, IType> lookups)
        {
            IType[] evalKeys = new IType[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] is IUnevaluated u)
                {
                    evalKeys[i] = u.Evaluate(lookups);
                    if (evalKeys[i] is Union un)
                    {
                        for (int k = i; k < keys.Length; k++)
                        {
                            evalKeys[k] = keys[k];
                        }
                        IType[] maps = new IType[un.values.Length];
                        for (int j = 0; j < maps.Length; j++)
                        {
                            var unKeys = evalKeys.ToArray();
                            unKeys[i] = un.values[j];
                            maps[j] = new UnevaluatedMap(unKeys, values).Evaluate(lookups);
                        }
                        return new Union(maps);
                    }
                }
                else
                    evalKeys[i] = keys[i];
            }
            return new Map(evalKeys, values);
        }
    }
}
