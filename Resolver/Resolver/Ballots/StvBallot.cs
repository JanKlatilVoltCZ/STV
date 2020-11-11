using System;
using System.Collections.Generic;

namespace Resolver.Ballots
{
    public sealed class StvBallot
    {
        /// <summary>
        /// First = First choice
        /// Last  = Last Choice
        /// never null
        /// </summary>
        public IReadOnlyList<int> OrderedCandidatesIds { get; }
        public int Count { get; private set; }

        public StvBallot(IReadOnlyList<int> orderedCandidatesIds = null)
        {
            OrderedCandidatesIds = orderedCandidatesIds ?? new int[0];

            for (int i = 0; i < OrderedCandidatesIds.Count; i++)
                for (int j = 0; j < OrderedCandidatesIds.Count; j++)
                    if (i != j && OrderedCandidatesIds[i] == OrderedCandidatesIds[j])
                        throw new ArgumentException($"Candidate Id {OrderedCandidatesIds[i]} 2x on ballot");
        }

        public void Add(int count)
        {
            if (0 < count && count < int.MaxValue)
                Count = checked(Count + count);
            else
                throw new ArgumentException($"Count ({count}) must be from interval [1; {int.MaxValue})");
        }

        public bool HasSame(IReadOnlyList<int> orderedCandidatesIds)
        {
            if (orderedCandidatesIds == null)
            {
                if (OrderedCandidatesIds.Count == 0)
                    return true;
                else
                    return false;
            }

            if (OrderedCandidatesIds.Count != orderedCandidatesIds.Count)
                return false;

            for (int j = 0; j < OrderedCandidatesIds.Count; j++)
                if (OrderedCandidatesIds[j] != orderedCandidatesIds[j])
                    return false;

            return true;
        }

        //public void AddWeight(double weight)
        //{
        //    if (weight <= 0 || weight >= double.MaxValue || double.IsNaN(weight))
        //    {
        //        throw new ArgumentException($"Weight ({weight}) must be from interval (0; MaxValue)");
        //    }
        //    else
        //    {
        //        var value = Weight + weight;

        //        if (value - weight < double.Epsilon && Weight > 0)
        //            throw new ArgumentException($"Weight: Cant't add {weight} to {Weight}, too big change.");
        //        if (value - Weight < double.Epsilon && Weight > 0)
        //            throw new ArgumentException($"Weight: Cant't add {weight} to {Weight}, too small change.");
        //        if (double.IsInfinity(Weight))
        //            throw new ArgumentException($"Weight of ballot overflowed.");

        //        Weight = value;
        //    }
        //}
    }
}
