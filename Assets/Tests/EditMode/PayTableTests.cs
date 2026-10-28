using NUnit.Framework;
using UnityEditor;

namespace SlotGame.Tests
{
    public class PayTableTests
    {
        private SlotConfig config;

        [SetUp]
        public void LoadConfig()
        {
            config = AssetDatabase.LoadAssetAtPath<SlotConfig>("Assets/Data/ClassicSlotConfig.asset");
            config.BuildFullPayTable();
        }

        private int Id(string name)
        {
            return System.Array.FindIndex(config.symbols, s => s.name == name);
        }

        [Test]
        public void TheMachineHasFiveReelsAndEveryPaylinePatternSpansThem()
        {
            Assert.AreEqual(5, config.reels.Length);
            foreach (PayLine line in config.payTable) Assert.AreEqual(5, line.symbols.Length);
        }

        [Test]
        public void ThreeOfAKindHasTwoWildPositionsAndSoThreeVariants()
        {
            int a = Id("A");
            var line = new PayLine(new[] { a, a, a, -1, -1 }, 5);

            var variants = line.BuildWildVariants(config, 5);

            Assert.AreEqual(3, variants.Count);
            Assert.IsFalse(variants.Exists(v => v.symbols[0] == config.wildId), "wild must not replace the first symbol");
            Assert.IsTrue(variants.Exists(v => v.symbols[1] == config.wildId && v.symbols[2] == config.wildId));
            Assert.IsTrue(variants.TrueForAll(v => v.symbols[3] == -1 && v.symbols[4] == -1 && v.pay == 5));
        }

        [Test]
        public void FiveOfAKindHasFifteenWildVariants()
        {
            int a = Id("A");
            var line = new PayLine(new[] { a, a, a, a, a }, 50);
            Assert.AreEqual(15, line.BuildWildVariants(config, 5).Count); // 2^4 - 1 subsets of positions 2..5
        }

        [Test]
        public void SymbolsThatOptOutOfWildGetNoVariants()
        {
            int freeSpin = Id("FreeSpin");
            var line = new PayLine(new[] { freeSpin, freeSpin, freeSpin, freeSpin, freeSpin }, 0, 10);
            Assert.AreEqual(0, line.BuildWildVariants(config, 5).Count);
        }

        [Test]
        public void FullPayTableMatchesTheExpectedSize()
        {
            // ten normal symbols pay 3, 4 and 5 of a kind: 3 + 7 + 15 variants each, plus the two special lines
            int expected = 10 * 3 + 10 * (3 + 7 + 15) + 2;
            Assert.AreEqual(32, config.payTable.Count); // 10 symbols x (3, 4, 5 of a kind) + free spins + wild
            Assert.AreEqual(expected, config.FullPayTable.Count);
        }

        [Test]
        public void ReelStripsOnlyUseExistingSymbols()
        {
            foreach (ReelStrip strip in config.reels)
                foreach (int id in strip.order)
                    Assert.That(id, Is.InRange(0, config.symbols.Length - 1));
        }

        [Test]
        public void FiveScattersAreReachableWithOneScatterPerReel()
        {
            foreach (ReelStrip strip in config.reels)
                Assert.IsTrue(System.Array.IndexOf(strip.order, config.scatterId) >= 0);
        }

        [Test]
        public void EverySymbolHasSpriteAndBlur()
        {
            foreach (SymbolDef symbol in config.symbols)
            {
                Assert.IsNotNull(symbol.sprite, symbol.name);
                Assert.IsNotNull(symbol.blur, symbol.name);
            }
        }
    }
}
