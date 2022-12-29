using System;
using UnityEngine;

namespace ValheimTooler.Models
{
    public class TPTarget
    {
        public readonly TargetType targetType;
        public readonly ZNet.PlayerInfo playerNet;
        public readonly Player player;
        public readonly ZNetPeer peer;
        public readonly Minimap.PinData minimapPin;

        public string Name
        {
            get
            {
                switch (targetType)
                {
                    case TargetType.PlayerNet:
                        return playerNet.m_name;
                    case TargetType.Player:
                        return player.GetPlayerName();
                    case TargetType.Peer:
                        return peer.m_playerName;
                    case TargetType.MapPin:
                        return $"{minimapPin.m_name} ({minimapPin.m_type})";
                }
                return "";
            }
        }

        public Vector3? Position
        {
            get
            {
                switch (targetType)
                {
                    case TargetType.PlayerNet:
                        if (playerNet.m_publicPosition)
                        {
                            return playerNet.m_position;
                        }
                        return null;
                    case TargetType.Player:
                        return player.transform.position;
                    case TargetType.Peer:
                        return null;
                    case TargetType.MapPin:
                        Vector3 pin_pos = minimapPin.m_pos;

                        pin_pos.y = Mathf.Clamp(pin_pos.y, ZoneSystem.instance.m_waterLevel, ZoneSystem.instance.m_waterLevel);

                        return pin_pos;
                }
                return null;
            }
        }

        public TPTarget(TargetType targetType, object target)
        {
            this.targetType = targetType;

            switch (targetType)
            {
                case TargetType.PlayerNet:
                    playerNet = (ZNet.PlayerInfo)target;
                    break;
                case TargetType.Player:
                    player = (Player)target;
                    break;
                case TargetType.Peer:
                    peer = (ZNetPeer)target;
                    break;
                case TargetType.MapPin:
                    minimapPin = (Minimap.PinData)target;
                    break;
            }
        }

        public override string ToString()
        {
            switch (targetType)
            {
                case TargetType.PlayerNet:
                    return $"Player {playerNet.m_name}";
                case TargetType.Player:
                    return $"Player {player.GetPlayerName()}";
                case TargetType.Peer:
                    return $"Player {peer.m_playerName}";
                case TargetType.MapPin:
                    return $"Map pin: {minimapPin.m_name} ({minimapPin.m_type})";
            }
            return "Unknow";
        }

        public enum TargetType
        {
            PlayerNet,
            Player,
            Peer,
            MapPin
        }
    }
}
