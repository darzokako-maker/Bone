using System;
using System.Collections.Generic;
using System.Numerics;
using CS2Cheat.Core.Data;
using CS2Cheat.Data.Entity;
using CS2Cheat.Data.Game;
using CS2Cheat.Graphics;
using ImGuiNET;

namespace CS2Cheat.Features
{
    public static class EspRenderer
    {
        private static readonly (string Start, string End)[] BoneConnections =
        [
            ("head", "neck_0"), ("neck_0", "spine_1"), ("spine_1", "spine_2"), ("spine_2", "pelvis"),
            ("spine_1", "arm_upper_L"), ("arm_upper_L", "arm_lower_L"), ("arm_lower_L", "hand_L"),
            ("spine_1", "arm_upper_R"), ("arm_upper_R", "arm_lower_R"), ("arm_lower_R", "hand_R"),
            ("pelvis", "leg_upper_L"), ("leg_upper_L", "leg_lower_L"), ("leg_lower_L", "ankle_L"),
            ("pelvis", "leg_upper_R"), ("leg_upper_R", "leg_lower_R"), ("leg_lower_R", "ankle_R")
        ];

        public static void Draw(ImDrawListPtr drawList, GameData gameData)
        {
            var player = gameData?.Player;
            if (player == null || gameData.Entities == null) return;

            foreach (var entity in gameData.Entities)
            {
                if (entity == null || !entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;

                // Terörist: Sarı, Anti-Terörist: Mavi
                uint teamColor = entity.Team == Team.Terrorists ? OverlayRenderer.Colors.Yellow : OverlayRenderer.Colors.Blue;

                var boundingBox = GetEntityBoundingBox(player, entity);
                if (boundingBox.HasValue)
                {
                    var (min, max) = boundingBox.Value;

                    // 1. Box Çizimi (Siyah gölgeli dış hat)
                    drawList.AddRect(min - new Vector2(1, 1), max + new Vector2(1, 1), OverlayRenderer.Colors.Black, 0.0f, ImDrawFlags.None, 1.0f);
                    drawList.AddRect(min, max, teamColor, 0.0f, ImDrawFlags.None, 1.5f);

                    // 2. Can Çubuğu (Health Bar)
                    float boxHeight = max.Y - min.Y;
                    float healthHeight = boxHeight * (entity.Health / 100.0f);
                    uint healthColor = entity.Health < 40 ? OverlayRenderer.Colors.Red : (entity.Health < 75 ? OverlayRenderer.Colors.Yellow : OverlayRenderer.Colors.Green);
                    drawList.AddRectFilled(new Vector2(min.X - 6, min.Y), new Vector2(min.X - 2, max.Y), OverlayRenderer.Colors.Black);
                    drawList.AddRectFilled(new Vector2(min.X - 5, max.Y - healthHeight), new Vector2(min.X - 3, max.Y), healthColor);

                    // 3. İsim Yazısı
                    string name = string.IsNullOrEmpty(entity.Name) ? "Enemy" : entity.Name;
                    Vector2 nameSize = ImGui.CalcTextSize(name);
                    DrawOutlinedText(drawList, new Vector2(min.X + ((max.X - min.X) / 2.0f) - (nameSize.X / 2.0f), min.Y - nameSize.Y - 2), OverlayRenderer.Colors.White, name);
                }

                // 4. İskelet Çizimi (Skeleton)
                DrawSkeleton(drawList, player, entity, teamColor);
            }
        }

        private static void DrawSkeleton(ImDrawListPtr drawList, Player player, Entity entity, uint color)
        {
            if (entity.BonePos == null) return;
            var matrix = player.MatrixViewProjectionViewport;

            foreach (var (start, end) in BoneConnections)
            {
                if (!entity.BonePos.TryGetValue(start, out var startW) || !entity.BonePos.TryGetValue(end, out var endW)) continue;

                var startS = matrix.Transform(startW);
                var endS = matrix.Transform(endW);

                if (startS.Z >= 1 || endS.Z >= 1) continue; // Görüş alanı dışındaysa atla

                drawList.AddLine(new Vector2(startS.X, startS.Y), new Vector2(endS.X, endS.Y), color, 1.5f);
            }
        }

        private static (Vector2 Min, Vector2 Max)? GetEntityBoundingBox(Player player, Entity entity)
        {
            if (entity.BonePos == null || entity.BonePos.Count == 0) return null;
            
            var minPos = new Vector2(float.MaxValue, float.MaxValue);
            var maxPos = new Vector2(float.MinValue, float.MinValue);
            var matrix = player.MatrixViewProjectionViewport;
            bool valid = false;

            foreach (var bone in entity.BonePos.Values)
            {
                var trans = matrix.Transform(bone);
                if (trans.Z >= 1) continue;

                valid = true;
                minPos.X = Math.Min(minPos.X, trans.X);
                minPos.Y = Math.Min(minPos.Y, trans.Y);
                maxPos.X = Math.Max(maxPos.X, trans.X);
                maxPos.Y = Math.Max(maxPos.Y, trans.Y);
            }

            if (!valid) return null;
            return (minPos - new Vector2(4, 4), maxPos + new Vector2(4, 4));
        }

        private static void DrawOutlinedText(ImDrawListPtr drawList, Vector2 pos, uint color, string text)
        {
            drawList.AddText(pos + new Vector2(1, 1), OverlayRenderer.Colors.Black, text);
            drawList.AddText(pos, color, text);
        }
    }
}
