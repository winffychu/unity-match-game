using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryMatch.Data
{
    [Serializable]
    public class LevelConfig
    {
        public int levelIndex;
        public int rows;
        public int columns;
        public int maxFlipCount;

        public int TotalCards => rows * columns;
        public int PairCount => TotalCards / 2;
    }

    [Serializable]
    public class CardData
    {
        public int cardId;
        public Sprite frontSprite;
    }

    public enum GameState
    {
        Idle,
        Playing,
        Checking,
        Win,
        Lose
    }

    public class FlipRecord
    {
        public Card card;
        public bool wasMatched;

        public FlipRecord(Card card, bool wasMatched)
        {
            this.card = card;
            this.wasMatched = wasMatched;
        }
    }
}
