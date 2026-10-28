using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SlotGame.EditorTools
{
    /// <summary>Creates the SlotConfig asset for the classic 3x5 machine: symbols, reel strips and pay table.</summary>
    public static class ClassicConfigFactory
    {
        public const string ConfigPath = "Assets/Data/ClassicSlotConfig.asset";

        // symbol ids, in the order of the SlotConfig.symbols list
        public const int A = 0, J = 1, K = 2, Q = 3, Ten = 4, Seven = 5, Bar1 = 6, Bar2 = 7, Bar3 = 8,
                         Bell = 9, FreeSpin = 10, Scatter = 11, Wild = 12, Jackpot = 13;

        public static SlotConfig CreateOrUpdate()
        {
            EnsureFolder("Assets/Data");
            var config = AssetDatabase.LoadAssetAtPath<SlotConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SlotConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            config.symbols = new[]
            {
                Symbol("A", "A.png", "Symbol Blur A.png", true),
                Symbol("J", "J.png", "Symbol Blur J.png", true),
                Symbol("K", "K.png", "Symbol Blur K.png", true),
                Symbol("Q", "Q.png", "Symbol Blur Q.png", true),
                Symbol("10", "10.png", "Symbol Blur 10.png", true),
                Symbol("7", "7.png", "Symbol Blur 7.png", true),
                Symbol("1Bar", "1Bar.png", "Symbol Blur Bar.png", true),
                Symbol("2Bar", "2Bar.png", "Symbol Blur 2Bar.png", true),
                Symbol("3Bar", "3Bar.png", "Symbol Blur 3Bar.png", true),
                Symbol("Bell", "Bell.png", "Symbol Blur Bell.png", true),
                Symbol("FreeSpin", "FreeSpin.png", "Symbol Blur Free Speen.png", false),
                Symbol("Scatter", "Scatter.png", "Symbol Blur Scatter.png", false),
                Symbol("Wild", "Wild.png", "Symbol Blur Wild.png", false),
                Symbol("Jackpot", "Jackpot.png", "Symbol Blur Jackpot.png", false),
            };
            config.scatterId = Scatter;
            config.wildId = Wild;
            config.useWild = true;
            config.useHold = false; // the classic 3x5 machine has no hold feature
            config.useScatter = true;
            config.wildInFirstPosition = false;

            config.tilesPerReel = 30;
            config.reels = new[]
            {
                Reel(0.1f, A, FreeSpin, J, Wild, K, Q, Ten, Bar1, Seven, Bar2, Bar3, Bell, Scatter, Ten, Jackpot, Bell),
                Reel(0.2f, A, Jackpot, J, K, Q, FreeSpin, Ten, Bar1, Seven, Bar3, Bell, Bar2, Scatter, Ten, Wild, Seven, Bar3),
                Reel(0.3f, A, J, Jackpot, K, Bar3, Q, Ten, Bar1, Seven, Bar2, Bell, Scatter, Wild, Bell, FreeSpin, Ten),
                Reel(0.3f, A, J, K, Q, Ten, Seven, Bar1, Bar2, Bar3, Bell, FreeSpin, Scatter, Jackpot, Wild, Jackpot, Seven, Bell, Ten, Wild),
                Reel(0.4f, A, J, K, Q, Ten, Seven, Wild, Bar1, Bar2, Bar3, Bell, FreeSpin, Scatter, Wild, Jackpot, Seven, Jackpot, Bell, Wild, Bell),
            };

            // three, four or five of a kind from the left; the free spin and wild lines need all five
            config.payTable = new List<PayLine>();
            AddKinds(config, A, 5, 25, 50);
            AddKinds(config, J, 10, 35, 75);
            AddKinds(config, K, 15, 50, 80);
            AddKinds(config, Q, 25, 50, 120);
            AddKinds(config, Ten, 25, 50, 130);
            AddKinds(config, Seven, 35, 60, 140);
            AddKinds(config, Bar1, 35, 65, 145);
            AddKinds(config, Bar2, 40, 70, 150);
            AddKinds(config, Bar3, 70, 100, 160);
            AddKinds(config, Bell, 70, 120, 200);
            config.payTable.Add(Line(FreeSpin, 5, 0, 10));
            config.payTable.Add(Line(Wild, 5, 500));
            config.scatterPayTable = new List<ScatterPay>
            {
                // five scatters anywhere on screen fill the level bar
                new ScatterPay { scattersCount = 5, levelProgress = 100f },
            };

            config.inRotTime = 0.15f; config.inRotAngle = 7f;
            config.outRotTime = 0.15f; config.outRotAngle = 7f;
            config.mainRotateTime = 3f; config.mainRotateTimeRandomize = 10;

            config.startCoins = 500;
            config.maxLineBet = 20;
            config.defaultLineBet = 1;
            config.multiplyWinByLineBet = true;
            config.multiplyFreeSpinsByLineBet = false;

            config.loseSpinLevelProgress = 0.5f;
            config.winSpinLevelProgress = 2f;
            config.bigWinMinCoins = 5000;

            config.jackpotSymbolId = Jackpot;
            config.useMegaJackpot = true;
            config.megaJackpotCount = 7;
            config.megaJackpotStart = 1000;
            config.jackpotIncrement = 1;

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static SymbolDef Symbol(string name, string spriteFile, string blurFile, bool wildSubstitute)
        {
            return new SymbolDef
            {
                name = name,
                sprite = SymbolArt.Get(spriteFile),
                blur = SymbolArt.Get(blurFile),
                wildSubstitute = wildSubstitute,
            };
        }

        private static ReelStrip Reel(float startDelay, params int[] order)
        {
            return new ReelStrip { order = order, spinStartDelay = startDelay, laps = 3, randomStart = true };
        }

        /// <summary><paramref name="count"/> of the same symbol from the left; the remaining reels may show anything.</summary>
        private static PayLine Line(int symbol, int count, int pay, int freeSpins = 0)
        {
            var symbols = new int[5];
            for (int i = 0; i < symbols.Length; i++) symbols[i] = i < count ? symbol : -1;
            return new PayLine(symbols, pay, freeSpins);
        }

        private static void AddKinds(SlotConfig config, int symbol, int pay3, int pay4, int pay5)
        {
            config.payTable.Add(Line(symbol, 3, pay3));
            config.payTable.Add(Line(symbol, 4, pay4));
            config.payTable.Add(Line(symbol, 5, pay5));
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
