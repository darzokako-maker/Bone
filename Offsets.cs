using System;
using System.Collections.Generic;

namespace CS2Cheat.Data.Game
{
    public static class Offsets
    {
        // 2026 Tarihli Ana Adresler (offsets.hpp)
        public static readonly IntPtr dwEntityList = 0x24E76A0;       
        public static readonly IntPtr dwLocalPlayerPawn = 0x2341698;  
        public static readonly IntPtr dwLocalPlayerController = 0x2320720; 
        public static readonly IntPtr dwViewMatrix = 0x23476E0;       

        // 2026 Tarihli Sınıf İçi Ofsetler (client_dll.hpp)
        public static readonly int m_iHealth = 0x34C;                
        public static readonly int m_iTeamNum = 0x3EB;                
        public static readonly int m_vOldOrigin = 0x1390;             
        public static readonly int m_pGameSceneNode = 0x328;          
        public static readonly int m_pClippingWeapon = 0x13A0;        
        public static readonly int m_bDormant = 0xEF;                 
        public static readonly int m_iszPlayerName = 0x638;           
        public static readonly int m_hPlayerPawn = 0x90C;             
        public static readonly int m_bIsScoped = 0x23A4;              
        public static readonly int m_flFlashDuration = 0x1360;         
        public static readonly int m_entitySpottedState = 0x23B0;     
        public static readonly int m_AimPunchCache = 0x1538;          
        public static readonly int m_AimPunchAngle = 0x156C;          
        public static readonly int m_modelState = 0x170;              
        public static readonly int m_AttributeManager = 0x1110;        
        public static readonly int m_Item = 0x50;                     
        public static readonly int m_iItemDefinitionIndex = 0x1E;     

        // İskelet Çizim Kemik İndeksleri
        public static readonly IReadOnlyDictionary<string, int> Bones = new Dictionary<string, int>
        {
            { "head", 6 }, { "neck_0", 5 }, { "spine_1", 4 }, { "spine_2", 2 }, { "pelvis", 0 },
            { "arm_upper_L", 8 }, { "arm_lower_L", 9 }, { "hand_L", 10 },
            { "arm_upper_R", 13 }, { "arm_lower_R", 14 }, { "hand_R", 15 },
            { "leg_upper_L", 22 }, { "leg_lower_L", 23 }, { "ankle_L", 24 },
            { "leg_upper_R", 25 }, { "leg_lower_R", 26 }, { "ankle_R", 27 }
        };
    }
}

