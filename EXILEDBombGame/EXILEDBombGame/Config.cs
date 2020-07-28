using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace EXILEDBombGame
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public float DiffuseTime { get; set; } = 10f;
        public float PlantTime { get; set; } = 3f;
        public float RoundTime { get; set; } = 180f;
        public float DistanceFromSite { get; set; } = 10f;
        public float BombTimer { get; set; } = 50f;
        public string BombPlantText { get; set; } = "<color=red>Bomb has been planted!</color>";
        public string BombExplodeText { get; set; } = "<color=red>Bomb has been detonated!</color>";
        public string RoundDrawText { get; set; } = "<color=red>Round Draw!</color>";
        public string BombDiffuseText { get; set; } = "<color=red>Bomb has been diffused!</color>";
        public string RoundInfoText { get; set; } = "Time until draw: %roundtimer%\n<color=blue>NTF: %ntf%</color> | <color=green>CI: %ci%</color>\nBomb planted: %bombplanted%";
        public string BombInfoText { get; set; } = "\n<color=red>Time until detonation: %bombtimer%</color>";
        public string CISpawn { get; set; } = "LCZ_372";
        public string CISpawnText { get; set; } = "Plant the bomb (coin) at the Micro HID Room!";
        public string NTFSpawnText { get; set; } = "Defend the bomb site at the Micro HID Room!";
        public string NTFSpawn { get; set; } = "HCZ_079";
        public string BombsiteSpawn { get; set; } = "HCZ_106";
        public List<ItemType> CIItems { get; set; } = new List<ItemType>()
        {
            ItemType.GrenadeFlash,
            ItemType.GunLogicer,
            ItemType.SCP207,
        };
        public List<ItemType> NTFItems { get; set; } = new List<ItemType>()
        {
            ItemType.GrenadeFlash,
            ItemType.GunLogicer,
            ItemType.SCP207,
        };
    }
}