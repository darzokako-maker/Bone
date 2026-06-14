using System;
using System.Collections.Generic;
using System.Numerics;
using CS2Cheat.Core.Data;       // Team enumu için doğru ad alanı
using CS2Cheat.Data.Entity;     // Player ve Entity sınıfları için doğru ad alanı
using CS2Cheat.Data.Game;       // GameData sınıfı için doğru ad alanı
using ImGuiNET;

namespace CS2Cheat.Features
{
    public static class EspRenderer
    {
        // C# 11 ve eski sürümlerle tam uyumlu dizi tanımı (Hata riskini sıfırlar)
        private static readonly (string Start, string End)[] BoneConnections = new (string Start, string End)[]
        {
            ("head", "neck_0"), ("neck_0", "spine_1"), ("spine_1", "spine_2"), ("spine_2", "pelvis"),
            ("spine_1", "arm_upper_L"), ("arm_upper_L", "arm_lower_L"), ("arm_lower_L", "hand_L"),
            ("spine_1", "arm_upper_R"), ("arm_upper_R", "arm_lower_R"), ("arm_lower_R", "hand_R"),
            ("pelvis", "leg_upper_L"), ("leg_upper_L", "leg_lower_L"), ("leg_lower_L", "ankle_L"),
            ("pelvis", "leg_upper_R"), ("leg_upper_R", "leg_lower_R"), ("leg_lower_R", "ankle_R")
        };

        public static void Draw(ImDrawListPtr drawList, GameData gameData)
        {
            var player = gameData?.Player;
            if (player == null || gameData.Entities == null) return;

            foreach (var entity in gameData.Entities)
            {
                // IsAlive kontrolü ve kendimizi çizmeyi engelleme
                if (entity == null || !entity.IsAlive() || entity.AddressBase == player.AddressBase) continue;

                // ImGui için Evrensel Renk Tanımlamaları (Harici sınıf bağımlılığını kaldırır)
                uint teamColor = entity.Team == Team.Terrorists 
                    ? ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 0.0f, 1.0f))   // Sarı (T)
                    : ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f));  // Mavi (CT)

                var boundingBox = GetEntityBoundingBox(player, entity);
                if (boundingBox.HasValue)
                {
                    var (min, max) = boundingBox.Value;

                    // 1. Box ESP Çizimi (Siyah kenarlıklı)
                    drawList.AddRect(min - new Vector2(1, 1), max + new Vector2(1, 1), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), 0.0f, ImDrawFlags.None, 1.0f);
                    drawList.AddRect(min, max, teamColor, 0.0f, ImDrawFlags.None, 1.5f);

                    // 2. Can Çubuğu (Health Bar)
                    float boxHeight = max.Y - min.Y;
                    float healthHeight = boxHeight * (entity.Health / 100.0f);
                    
                    uint healthColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Yeşil
                    if (entity.Health < 40) healthColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Kırmızı
                    else if (entity.Health < 75) healthColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 0.0f, 1.0f)); // Sarı

                    drawList.AddRectFilled(new Vector2(min.X - 6, min.Y), new Vector2(min.X - 2, max.Y), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)));
                    drawList.AddRectFilled(new Vector2(min.X - 5, max.Y - healthHeight), new Vector2(min.X - 3, max.Y), healthColor);

                    // 3. Oyuncu İsmi Çizimi
                    string name = string.IsNullOrEmpty(entity.Name) ? "Enemy" : entity.Name;
                    Vector2 nameSize = ImGui.CalcTextSize(name);
                    DrawOutlinedText(drawList, new Vector2(min.X + ((max.X - min.X) / 2.0f) - (nameSize.X / 2.0f), min.Y - nameSize.Y - 2), ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), name);
                }

                // 4. İskelet (Skeleton) Çizimi
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

                if (startS.Z >= 1 || endS.Z >= 1) continue;

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
            drawList.AddText(pos + new Vector2(1, 1), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), text);
            drawList.AddText(pos, color, text);
        }
    }
}
