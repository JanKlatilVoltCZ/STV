using NUnit.Framework;
using Resolver.Ballots;
using System;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class StvBallotsTests
    {
        [Test]
        public void EmptyBallotsTest()
        {
            var a = new StvBallot();
            Assert.IsNotNull(a.OrderedCandidatesIds);
            Assert.IsTrue(a.HasSame(null));
            Assert.IsTrue(a.HasSame(new int[0]));
        }

        [Test]
        public void InvalidBallotsTest()
        {
            Assert.Throws<ArgumentException>(() => new StvBallot(new[] { 1, 2, 1 }));
        }

        [Test]
        public void CompareTest()
        {
            var a = new StvBallot(new[] { 1, 2 });

            Assert.IsFalse(a.HasSame(null));
            Assert.IsFalse(a.HasSame(new int[] { }));
            Assert.IsFalse(a.HasSame(new int[] { 1 }));
            Assert.IsFalse(a.HasSame(new int[] { 1, 2, 3 }));
            Assert.IsFalse(a.HasSame(new int[] { 1, 3 }));
            Assert.IsTrue(a.HasSame(new int[] { 1, 2 }));
        }

        [Test]
        public void CountTest()
        {
            var a = new StvBallot();

            Assert.Throws<ArgumentException>(() => a.Add(-1));
            Assert.Throws<ArgumentException>(() => a.Add(0));
            Assert.Throws<ArgumentException>(() => a.Add(int.MinValue));
            Assert.Throws<ArgumentException>(() => a.Add(int.MaxValue));
            a.Add((int)(int.MaxValue * 2d / 3d));
            Assert.Throws<OverflowException>(() => a.Add((int)(int.MaxValue * 2d / 3d)));



            //Assert.Throws<ArgumentException>(() => a.AddWeight(double.NaN));
            //Assert.Throws<ArgumentException>(() => a.AddWeight(double.MaxValue));
            //Assert.Throws<ArgumentException>(() => a.AddWeight(double.PositiveInfinity));
            //Assert.Throws<ArgumentException>(() => a.AddWeight(0));
            //Assert.Throws<ArgumentException>(() => a.AddWeight(-1));

            //a.AddWeight(10);
            //Assert.Throws<ArgumentException>(() => a.AddWeight(double.MaxValue * (1d / 3d)));

            //var b = new StvBallot();
            //b.AddWeight(double.MaxValue * (1d / 3d));
            //Assert.Throws<ArgumentException>(() => b.AddWeight(10));

            //b.AddWeight(double.MaxValue * (1d / 3d));
            //Assert.Throws<ArgumentException>(() => a.AddWeight(double.MaxValue * (2d / 3d)));
        }

        [Test]
        public void BallotsCounterTest()
        {
            var counter = new StvBallotsCounter();
            counter.AddBallot(1);
            counter.AddBallot(1);
            counter.AddBallot(1, 1);
            counter.AddBallot(1, 1);
            counter.AddBallot(1, 2);
            var ballots = counter.GetBallots();

            Assert.IsTrue(ballots.Count == 3);
            Assert.IsTrue(ballots.Sum(i => i.Count) == 5);
            Assert.IsTrue(ballots.Where(i => i.HasSame(null)).Sum(i => i.Count) == 2);
            Assert.IsTrue(ballots.Where(i => i.HasSame(new[] { 1 })).Sum(i => i.Count) == 2);
            Assert.IsTrue(ballots.Where(i => i.HasSame(new[] { 2 })).Sum(i => i.Count) == 1);
        }
    }
}
