using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace EXILEDBombGame
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int StartMoney { get; set; } = 100;
        public int KillMoney { get; set; } = 300;
        public int BombInteractMoney { get; set; } = 700;
        public int RoundWinMoney { get; set; } = 500;
        public int RoundLoseMoney { get; set; } = 200;
        public int RoundsBeforeReset { get; set; } = 5;
        public int PlayerMaxHP { get; set; } = 100;
        public float DiffuseTime { get; set; } = 10f;
        public float PlantTime { get; set; } = 3f;
        public float RoundTime { get; set; } = 300f;
        public float DistanceFromSite { get; set; } = 10f;
        public float BombTimer { get; set; } = 50f;
        public float BuyTimer { get; set; } = 20f;
        public string BombPlantText { get; set; } = "<color=red>Bomb has been planted!</color>";
        public string BombExplodeText { get; set; } = "<color=red>Bomb has been detonated!</color>";
        //public string RoundDrawText { get; set; } = "<color=red>Round Draw!</color>";
        public string RoundCIWin { get; set; } = "<color=red>CI win!</color>";
        public string RoundNTFWin { get; set; } = "<color=red>NTF win!</color>";
        public string BombDiffuseText { get; set; } = "<color=red>Bomb has been diffused!</color>";
        public string RoundInfoText { get; set; } = "Time until round end: %roundtimer%\n<color=blue>NTF: %ntf%</color> | <color=green>CI: %ci%</color>\n%bombplanted%";
        public string BombInfoText { get; set; } = "\n<color=red>Time until detonation: %bombtimer%</color>";
        public string CISpawn { get; set; } = "HCZ_457";
        public string CISpawnText { get; set; } = "Plant the bomb (CI Keycard) at the Micro HID Room or Server Room!";
        public string NTFSpawnText { get; set; } = "Defend the bomb site at the Micro HID Room or Server Room!";
        public string NTFSpawn { get; set; } = "HCZ_079";
        public List<string> BombsiteSpawn { get; set; } = new List<string>()
        {
            "HCZ_Hid",
            "HCZ_Server",
        };
        public string BuyTimeExpireText { get; set; } = "Buy time expired.";
        public string BuyTimeTitleText { get; set; } = "Your money: %money%\nShop:\n";
        public string BuyTimeCompleteText { get; set; } = "Purchase complete.";
        public string BuyTimeFailText { get; set; } = "Not enough Money.";
        public List<ItemType> CIItems { get; set; } = new List<ItemType>()
        {
            ItemType.GunCOM15,
            ItemType.Medkit,
        };
        public List<ItemType> NTFItems { get; set; } = new List<ItemType>()
        {
            ItemType.GunCOM15,
            ItemType.Medkit,
        };

        public RoleType CIRole { get; set; } = RoleType.ChaosInsurgency;
        public RoleType NTFRole { get; set; } = RoleType.NtfLieutenant;

        public List<ShopItem> CIBuyables { get; set; } = new List<ShopItem>()
        {
            new ShopItem()
            {
                Price = 1300,
                Item = ItemType.GunLogicer,
                DisplayName = "Logicer",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 2400,
                Item = ItemType.GunUSP,
                DisplayName = "USP",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 500,
                Item = ItemType.GunMP7,
                DisplayName = "MP7",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 400,
                Item = ItemType.GrenadeFrag,
                DisplayName = "Frag. Grenade",
            },
        };
        public List<ShopItem> NTFBuyables { get; set; } = new List<ShopItem>()
        {
            new ShopItem()
            {
                Price = 1200,
                Item = ItemType.GunE11SR,
                DisplayName = "E11 Rifle",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 2400,
                Item = ItemType.GunUSP,
                DisplayName = "USP",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 500,
                Item = ItemType.GunMP7,
                DisplayName = "MP7",
                Barrel = 0,
                Sight = 0,
                Other = 0,
            },
            new ShopItem()
            {
                Price = 400,
                Item = ItemType.SCP018,
                DisplayName = "SCP-018",
            },
        };
    }

    public class ShopItem
    {
        public int Price { get; set; } = 0;
        public ItemType Item { get; set; } = ItemType.Flashlight;
        public string DisplayName { get; set; } = "Flashlight";
        public int Sight { get; set; } = -1;
        public int Barrel { get; set; } = -1;
        public int Other { get; set; } = -1;
    }
}