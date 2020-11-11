using Resolver.Ballots;
using System.Collections.Generic;

namespace Resolver.Voting
{
    public class StvBallotWithWeight
    {
        private readonly StvBallot ballot;
        public double Weight { get; set; }

        public StvBallotWithWeight(StvBallot ballot)
        {
            this.ballot = ballot;
            Weight = ballot.Count;
        }

        internal int GetTopCandidateId(CandidatesStatus candidates4Vote)
        {
            for (int i = 0; i < ballot.OrderedCandidatesIds.Count; i++)
                if (candidates4Vote.IsUndecided(ballot.OrderedCandidatesIds[i]))
                    return ballot.OrderedCandidatesIds[i];

            return CandidatesAndIds.InvalidId;
        }

        internal IEnumerable<int> GetAllUndecidedCandidatesIds(CandidatesStatus candidates4Vote)
        {
            for (int i = 0; i < ballot.OrderedCandidatesIds.Count; i++)
                if (candidates4Vote.IsUndecided(ballot.OrderedCandidatesIds[i]))
                    yield return ballot.OrderedCandidatesIds[i];
        }
    }
}
