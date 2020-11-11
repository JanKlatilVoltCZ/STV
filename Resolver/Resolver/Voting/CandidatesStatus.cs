using System;

namespace Resolver.Voting
{
    public class CandidatesStatus
    {
        public int TotalCount { get; private set; }
        public int WinnersCount { get; private set; }
        public int LoosersCount { get; private set; }
        public int UndecidedCount { get; private set; }

        private enum State { Undecided, Winner, Loser, Invalid }
        private readonly State[] candidatesStates;

        public CandidatesStatus(CandidatesAndIds candidatesAndIds)
        {
            var (min, max) = candidatesAndIds.GetIdsRange();

            if (min < 0)
                throw new ArgumentException($"Id must be at least 0 (found {min})");

            candidatesStates = new State[max + 1];
            for (int i = 0; i < candidatesStates.Length; i++)
            {
                if (candidatesAndIds.IsValidId(i))
                {
                    candidatesStates[i] = State.Undecided;
                    TotalCount++;
                    UndecidedCount++;
                }
                else
                {
                    candidatesStates[i] = State.Invalid;
                }
            }
        }

        internal bool IsUndecided(int id) => candidatesStates[id] == State.Undecided;

        internal void DeclareWinner(int id)
        {
            UndecidedCount--;
            WinnersCount++;
            candidatesStates[id] = State.Winner;
        }

        internal void DeclareLooser(int id)
        {
            UndecidedCount--;
            LoosersCount++;
            candidatesStates[id] = State.Loser;
        }
    }
}
