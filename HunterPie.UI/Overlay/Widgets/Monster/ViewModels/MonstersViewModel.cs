﻿using HunterPie.Core.Architecture;
using System;
using System.Collections.ObjectModel;

namespace HunterPie.UI.Overlay.Widgets.Monster.ViewModels
{
    public class MonstersViewModel : Bindable
    {
        const int MINIMUM_MONSTER_COUNT = 3;

        private int _visibleMonsters;
        private int _monstersCount;
        private readonly ObservableCollection<BossMonsterViewModel> _monsters = new();

        public ObservableCollection<BossMonsterViewModel> Monsters => _monsters;
        public int VisibleMonsters { get => _visibleMonsters; set { SetValue(ref _visibleMonsters, value); } }
        public int MonstersCount { get => _monstersCount; set { SetValue(ref _monstersCount, Math.Max(MINIMUM_MONSTER_COUNT, value)); } }
    }
}
