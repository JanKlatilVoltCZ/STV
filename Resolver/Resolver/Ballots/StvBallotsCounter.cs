using System.Collections.Generic;

namespace Resolver.Ballots
{
    public class StvBallotsCounter
    {
        private readonly List<StvBallot> ballots = new List<StvBallot>();
        public IReadOnlyList<StvBallot> GetBallots() => ballots.AsReadOnly();

        public void AddBallot(int multiplicity, params int[] orderedCandidatesIds)
        {
            if (!TryGetBallotWith(orderedCandidatesIds, out StvBallot ballot))
            {
                ballot = new StvBallot(orderedCandidatesIds);
                ballots.Add(ballot);
            }

            ballot.Add(multiplicity);
        }

        private bool TryGetBallotWith(int[] orderedCandidatesIds, out StvBallot ballot)
        {
            for (int i = 0; i < ballots.Count; i++)
                if (ballots[i].HasSame(orderedCandidatesIds))
                {
                    ballot = ballots[i];
                    return true;
                }

            ballot = null;
            return false;
        }
    }
}
