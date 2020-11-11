using Resolver.Ballots;
using Resolver.Utils;
using Resolver.Voting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Resolver
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Droop_quota
    /// https://en.wikipedia.org/wiki/Counting_single_transferable_votes
    /// </summary>
    public class SingleTransferableVote
    {
        private readonly int seats;
        public SingleTransferableVote(int seats) => this.seats = seats;

        public void ComputeResults(CandidatesAndIds candidatesAndIds, IReadOnlyList<StvBallot> ballots)
        {
            using var logger = new Logger(nameof(SingleTransferableVote));
            logger.Log("Computing Results by STV method with Droop Quota and Gregory Surplus allocation");

            var candidatesStatus = new CandidatesStatus(candidatesAndIds);
            var stvBallotsWithWeight = ballots.Select(i => new StvBallotWithWeight(i)).ToArray();

            var quota = ComputeQuota(ballots.Sum(i => i.Count));
            logger.Log($"Quota is {quota}");

            var round = 1;
            while (candidatesStatus.WinnersCount < seats)
            {
                logger.Log(string.Empty);
                logger.Log("###################");
                logger.Log($"Starting round {round}");

                var groupedBallots = GroupBallotsByTopCandidateId(stvBallotsWithWeight, candidatesStatus);
                var groupedVotes = GroupVotesByTopCandidateId(groupedBallots, candidatesAndIds, logger);

                if (TryGetWinners(groupedVotes, quota, out var winners))
                {
                    foreach (var winner in winners)
                    {
                        logger.Log($"Winner Found: {candidatesAndIds.FormatId(winner.candidateId)}:{candidatesAndIds.GetName(winner.candidateId)} with {winner.receivedVotes} votes");
                        candidatesStatus.DeclareWinner(winner.candidateId);
                        foreach (var ballot in groupedBallots[winner.candidateId])
                            ballot.Weight *= (winner.receivedVotes - quota) / winner.receivedVotes;
                    }
                }
                else if (TryGetCandidatesWithoutHope(stvBallotsWithWeight, candidatesStatus, quota, out var candidatesWithoutHope))
                {
                    foreach (var candidateWithoutHope in candidatesWithoutHope)
                    {
                        logger.Log($"Candidate Without Hope Found: {candidatesAndIds.FormatId(candidateWithoutHope.candidateId)}:{candidatesAndIds.GetName(candidateWithoutHope.candidateId)} with maximum {candidateWithoutHope.possibleVotes} votes");
                        candidatesStatus.DeclareLooser(candidateWithoutHope.candidateId);
                    }
                }
                else
                {
                    var (candidateId, receivedVotes) = FindLooser(groupedVotes, candidatesAndIds);
                    logger.Log($"Worst Candidate Eliminated: {candidatesAndIds.FormatId(candidateId)}:{candidatesAndIds.GetName(candidateId)} with {receivedVotes} votes");
                    candidatesStatus.DeclareLooser(candidateId);
                }

                if (candidatesStatus.UndecidedCount == 0)
                    break;

                round++;
            }

            var (min, max) = candidatesAndIds.GetIdsRange();
            for (int i = min; i <= max; i++)
                if (candidatesAndIds.IsValidId(i) && candidatesStatus.IsUndecided(i))
                    candidatesStatus.DeclareLooser(i);

        }

        private Dictionary<int, double> GroupVotesByTopCandidateId(Dictionary<int, List<StvBallotWithWeight>> groupedBallots, CandidatesAndIds candidatesAndIds, Logger logger)
        {
            logger.Log(string.Empty);
            logger.Log($"Received Votes:");

            var groupedVotes = groupedBallots.ToDictionary(i => i.Key, i => i.Value.Select(i => i.Weight).Sum());
            foreach (var candidateWithVotes in groupedVotes)
                logger.Log($"{candidatesAndIds.FormatId(candidateWithVotes.Key)}:{candidatesAndIds.GetName(candidateWithVotes.Key)} with {candidateWithVotes.Value} votes");

            logger.Log(string.Empty);

            return groupedVotes;
        }

        private (int candidateId, double receivedVotes) FindLooser(Dictionary<int, double> groupedVotes, CandidatesAndIds candidatesAndIds)
        {
            double min = groupedVotes.Select(i => i.Value).Min();

            var candidatesWithMin = groupedVotes.Where(i => i.Value == min).ToArray();

            if (candidatesWithMin.Length == 1)
                return (candidatesWithMin[0].Key, min);

            Console.WriteLine("!!!!! Multiple candidates with same votes found, one need to be eliminated .. Write Candidate ID to eliminate");
            foreach (var candidate in candidatesWithMin)
            {
                Console.WriteLine($"!!!!! {candidatesAndIds.FormatId(candidate.Key)}: {candidatesAndIds.GetName(candidate.Key)}");
            }

            while (true)
            {
                var input = Console.ReadLine();
                if (int.TryParse(input, out var id))
                    foreach (var candidate in candidatesWithMin)
                        if (candidate.Key == id)
                            return (id, min);

                Console.WriteLine($"!!!!! Id '{input}' was not an option, please repeate");
            }
        }

        private bool TryGetCandidatesWithoutHope(StvBallotWithWeight[] stvBallotsWithWeight, CandidatesStatus candidatesStatus, int quota, out List<(int candidateId, double possibleVotes)> candidatesWithoutHope)
        {
            var possibleVotes = new Dictionary<int, double>();

            foreach (var canditeIdWithBallots in stvBallotsWithWeight)
                foreach (var candidateId in canditeIdWithBallots.GetAllUndecidedCandidatesIds(candidatesStatus))
                {
                    if (!possibleVotes.TryGetValue(candidateId, out var votes))
                        possibleVotes.Add(candidateId, 0);

                    possibleVotes[candidateId] += canditeIdWithBallots.Weight;
                }

            candidatesWithoutHope = possibleVotes.Where(i => i.Value < quota).Select(i => (i.Key, i.Value)).ToList();
            return candidatesWithoutHope.Count > 0;
        }

        private bool TryGetWinners(Dictionary<int, double> groupedVotes, int quota, out List<(int candidateId, double receivedVotes)> winners)
        {
            winners = new List<(int, double)>();

            foreach (var canditeIdWithBallots in groupedVotes)
                if (canditeIdWithBallots.Value >= quota)
                    winners.Add((canditeIdWithBallots.Key, canditeIdWithBallots.Value));

            return winners.Count > 0;
        }

        private Dictionary<int, List<StvBallotWithWeight>> GroupBallotsByTopCandidateId(StvBallotWithWeight[] ballots, CandidatesStatus candidates)
        {
            var groupedBallots = new Dictionary<int, List<StvBallotWithWeight>>();
            foreach (var ballot in ballots)
            {
                var topCandidateId = ballot.GetTopCandidateId(candidates);

                if (topCandidateId == CandidatesAndIds.InvalidId || !candidates.IsUndecided(topCandidateId))
                    continue;

                if (!groupedBallots.TryGetValue(topCandidateId, out var ballotsWithSameTopCandidate))
                {
                    ballotsWithSameTopCandidate = new List<StvBallotWithWeight>();
                    groupedBallots.Add(topCandidateId, ballotsWithSameTopCandidate);
                }

                ballotsWithSameTopCandidate.Add(ballot);
            }
            return groupedBallots;
        }

        private int ComputeQuota(int votesCount) => ((int)(votesCount / (seats + (double)1))) + 1;
    }
}
