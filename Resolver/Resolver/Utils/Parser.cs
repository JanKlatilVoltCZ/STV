using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Resolver.Ballots
{
    public class Parser
    {
        public static (CandidatesAndIds candidatesManager, IReadOnlyList<StvBallot> ballots, int seatsCount) StvVoteFrom(string filePath, char fieldsSeparator = ',')
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} doesn't exists.");

            var lines = File.ReadAllLines(filePath);
            return StvVoteFrom(fieldsSeparator, lines);
        }

        public static (CandidatesAndIds candidatesManager, IReadOnlyList<StvBallot> ballots, int seatsCount) StvVoteFrom(char fieldsSeparator = ',', params string[] lines)
        {
            if (!int.TryParse(lines[0], out var seatsCount))
                throw new Exception($"Invalid seats count on line'{lines[0]}'");

            var candidatesManager = new CandidatesAndIds(lines[1].Split(fieldsSeparator));
            var counter = new StvBallotsCounter();

            for (int i = 2; i < lines.Length; i++)
            {
                var splits = lines[i].Split(fieldsSeparator);

                if (!int.TryParse(splits[0], out var multiplicity))
                    throw new Exception($"Invalid multiplicity on line'{lines[i]}'");

                counter.AddBallot(multiplicity, splits.Skip(1).Select(candidateName =>
                {
                    if (!candidatesManager.TryGetId(candidateName, out var id))
                        throw new Exception($"Unknown candidate {candidateName} on line'{lines[i]}'");

                    return id;
                }).ToArray());
            }

            return (candidatesManager, counter.GetBallots(), seatsCount);
        }
    }
}
