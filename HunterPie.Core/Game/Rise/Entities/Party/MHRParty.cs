﻿using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Rise.Definitions;
using HunterPie.Core.Logger;
using HunterPie.Core.Native.IPC.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace HunterPie.Core.Game.Rise.Entities.Party
{
    public class MHRParty : IParty, IEventDispatcher
    {
        private readonly Dictionary<int, MHRPartyMember> _partyMembers = new();
        private readonly Dictionary<int, MHRPartyMember> _partyMemberPets = new();
        private readonly Dictionary<string, MHRPartyMember> _partyMemberNameLookup = new();
        
        public const int MAX_PARTY_SIZE = 4;
        public int Size { get; internal set; }
        public int MaxSize => MAX_PARTY_SIZE;

        public List<IPartyMember> Members
        {
            get
            {
                var members = _partyMembers.Values.ToList();
                var pets = _partyMemberPets.Values.ToList();

                members.AddRange(pets);

                return members.ToList<IPartyMember>();
            }
        }

        public event EventHandler<IPartyMember> OnMemberJoin;
        public event EventHandler<IPartyMember> OnMemberLeave;
        
        public void Update(MHRPartyMemberData data)
        {
            if (string.IsNullOrEmpty(data.Name))
                return;


            if (!_partyMemberNameLookup.ContainsKey(data.Name))
                Add(data);

            IUpdatable<MHRPartyMemberData> updatable = _partyMembers[data.Index];
            updatable.Update(data);
        }

        public void Update(EntityDamageData data)
        {
            Dictionary<int, MHRPartyMember>? localData = data.Entity.Type switch
            {
                EntityType.PLAYER => _partyMembers,
                EntityType.PET => _partyMemberPets,
                _ => null
            };

            if (localData is null)
                return;

            if (!localData.ContainsKey(data.Entity.Index))
                return;

            IUpdatable<EntityDamageData> updatable = localData[data.Entity.Index];
            updatable.Update(data);
        }

        public void Add(MHRPartyMemberData data)
        {
            if (_partyMembers.ContainsKey(data.Index))
                Remove(data.Index);

            MHRPartyMember member = new();
            MHRPartyMember memberPet = new();
            _partyMembers.Add(data.Index, member);
            _partyMemberPets.Add(data.Index, member);
            _partyMemberNameLookup.Add(data.Name, member);

            IUpdatable<MHRPartyMemberData> updatable = member;
            IUpdatable<MHRPartyMemberData> updatablePet = memberPet;
            updatable.Update(data);
            updatablePet.Update(data);

            Log.Debug("Added new player to party: id: {0} name: {1} weap: {2}, hash: {3:X}", data.Index, data.Name, data.WeaponId, updatable.GetHashCode());
            
            this.Dispatch(OnMemberJoin, member);
            this.Dispatch(OnMemberJoin, memberPet);
        }

        public void Remove(int memberIndex)
        {
            if (!_partyMembers.ContainsKey(memberIndex))
                return;

            IPartyMember member = _partyMembers[memberIndex];
            IPartyMember memberPet = _partyMemberPets[memberIndex];
            _partyMembers.Remove(memberIndex);
            _partyMemberPets.Remove(memberIndex);
            _partyMemberNameLookup.Remove(member.Name);
            
            Log.Debug("Removed player: id: {0} name: {1} hash: {2:X}", memberIndex, member.Name, member.GetHashCode());

            this.Dispatch(OnMemberLeave, member);
            this.Dispatch(OnMemberLeave, memberPet);
        }

        public void Clear()
        {
            foreach (int index in _partyMembers.Keys.ToArray())
                Remove(index);
        }
    }
}
#nullable restore