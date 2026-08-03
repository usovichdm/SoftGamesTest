using NUnit.Framework;
using SoftGames.Gameplay.PhoenixFlame;

namespace SoftGames.Tests.EditMode
{
    public sealed class FireColorCycleTests
    {
        [Test]
        public void Advance_CyclesOrangeGreenBlue()
        {
            var cycle = new FireColorCycle(FireColorId.Orange);

            Assert.AreEqual(FireColorId.Green, cycle.Advance());
            Assert.AreEqual(FireColorId.Blue, cycle.Advance());
            Assert.AreEqual(FireColorId.Orange, cycle.Advance());
            Assert.AreEqual(FireColorId.Green, cycle.Advance());
        }

        [Test]
        public void Current_StartsAtConfiguredColor()
        {
            var cycle = new FireColorCycle(FireColorId.Blue);
            Assert.AreEqual(FireColorId.Blue, cycle.Current);
        }
    }
}
