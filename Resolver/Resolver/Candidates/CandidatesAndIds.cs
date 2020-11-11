using Resolver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Resolver
{
    public class CandidatesAndIds
    {
        public const int InvalidId = -1;

        #region Format
        public string FormatId(int id) => id.ToString(idFormat);
        private readonly string idFormat;
        #endregion

        private readonly Dictionary<string, Candidate> candidatesByName = new Dictionary<string, Candidate>();
        private readonly Dictionary<int, Candidate> candidatesById = new Dictionary<int, Candidate>();

        public CandidatesAndIds(params string[] names)
        {
            using var logger = new Logger(nameof(CandidatesAndIds));
            logger.Log("Loading candidates for election:");

            idFormat = GetIdFormat(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                if (candidatesByName.ContainsKey(names[i]))
                    throw new Exception($"Cannot have 2 candidates named {names[i]}");

                var candidate = new Candidate(names[i], i);
                logger.Log($"-{candidate.Id.ToString(idFormat)}:{candidate.Name}");

                candidatesByName.Add(names[i], candidate);
                candidatesById.Add(i, candidate);
            }



            //leading zeros for Ids
            static string GetIdFormat(int candidatesCount) => string.Join("", Enumerable.Repeat("0", ((int)Math.Log10(candidatesCount)) + 1));
        }

        public (int min, int max) GetIdsRange() => (0, candidatesById.Count - 1);

        public bool IsValidId(int id) => candidatesById.ContainsKey(id);

        public bool TryGetId(string name, out int id)
        {
            if (candidatesByName.TryGetValue(name, out var candidate))
            {
                id = candidate.Id;
                return true;
            }

            id = -1;
            return false;
        }

        public string GetName(int id) => candidatesById[id].Name;
    }
}
