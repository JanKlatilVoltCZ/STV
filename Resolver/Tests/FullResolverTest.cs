using NUnit.Framework;
using Resolver;
using Resolver.Ballots;

namespace Test
{
    [TestFixture]
    public class FullResolverTest
    {
        [Test]
        public void SimpleFullTest()
        {
            var (candidatesManager, ballots, seatsCount) = Parser.StvVoteFrom(lines: new[]
            {
                "1",
                "Hitler,Matka Tereza,Papez",
                "4,Hitler",
                "2,Matka Tereza,Papez",
                "3,Papez,Matka Tereza",

            });
            var vote = new SingleTransferableVote(seatsCount);

            vote.ComputeResults(candidatesManager, ballots);
        }

        [Test]
        public void RacionalFullTest()
        {
            var (candidatesManager, ballots, seatsCount) = Parser.StvVoteFrom(lines: new[]
            {
                "2",
                "A,B,C",
                "2,A,C,B",
                "2,A,B",
                "1,C",
                "1,B",
            });
            var vote = new SingleTransferableVote(seatsCount);

            vote.ComputeResults(candidatesManager, ballots);
            //var vote = new Vote(new SingleTransferableVote(2), "A", "B", "C");

            //vote.AddBallot(2, "A", "B");
            //vote.AddBallot(2, "A", "C", "B");
            //vote.AddBallot(2, "B");
            //vote.AddBallot(1, "C");

            //vote.ComputeResults();
        }
    }
}