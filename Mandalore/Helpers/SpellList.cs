using System.Diagnostics;
using System.Windows;
using System.Windows.Ink;
using JetBrains.Annotations;

namespace Mandalore.Helpers
{
    [UsedImplicitly]
    abstract class SpellList : Mandalore
    {

        #region DeathKnight Spells

        public const int
            AntiMagicShell = 48707,
            Asphyxiate = 221562,
            BloodBoil = 50842,
            Bonestorm = 194844,
            Consumption  = 205223,
            DancingRuneWeapon = 49028,
            DeathandDecay = 43265,
            DeathGrip = 49576,
            DeathsCaress = 195292,
            DeathStrike = 49998,
            GorefiendsGrasp = 108199,
            HeartStrike = 206930,
            Marrowrend = 195182,
            MindFreeze = 47528,
            VampiricBlood = 55233,
            WraithWalk = 212552,


DarkSuccor = 178819,
SummonGargoyle = 49206,
SoulReaper = 130736,
ScourgeStrike = 55090,
Runeforging = 53428,
RaiseDead = 46584,
RaiseAlly = 61999,
PathofFrost = 3714,
Outbreak = 77575,
IceboundFortitude = 48792,
FesteringStrike = 85948,
DeathGate = 50977,
DeathCoil = 47541,
DarkTransformation = 63560,
DarkCommand = 56222,
ControlUndead = 111673,
ChainsofIce = 45524,
ArmyoftheDead = 42650,
Apocalypse = 220143

            ;


        #endregion

        public const int
            Ascendance = 114052,
            AstralShift = 108271,
            Boulderfist = 201897,
            CrashLightning = 187874,
            DoomWinds = 204945,
            EarthenSpike = 188089,
            EarthgrabTotem = 51485,
            FeralLunge = 196884,
            FeralSpirit = 51533,
            Flametongue = 193796,
            Frostbrand = 196834,
            FuryofAir = 197211,
            HealingSurge = 188070,
            LavaLash = 60103,
            LightningBolt = 188196,
            LightningSurgeTotem = 192058,
            Rockbiter = 193786,
            Stormstrike = 17364,
            Sundering = 197214,
            Windsong = 201898,
            WindShear = 57994,
            Windstrike = 115357


            ;


        #region Paladin Spells (holy and ret so far, not all talents either)

        public const int
            Fishing = 131474,
            GreaterBlessingofWisdom = 203539,
            GreaterBlessingoMight = 203528,
            GreaterBlessingofKings = 203538,
            SwordofLight = 53503,
            Retribution = 183435,
            HeartoftheCrusader = 32223,
            WakeofAshes = 205273,
            TemplarsVerdict = 85256,
            ShieldofVengeance = 184662,
            Redemption = 7328,
            Rebuke = 96231,
            LayonHands = 633,
            JusticarsVengeance = 215661,
            Judgment = 20271,
            HandofReckoning = 62124,
            HandofHindrance = 183218,
            HammerofJustice = 853,
            FlashofLight = 19750,
            DivineStorm = 53385,
            DivineSteed = 190784,
            DivineShield = 642,
            CrusaderStrike = 35395,
            CleanseToxins = 213644,
            BlessingofProtection = 1022,
            BlessingofFreedom = 1044,
            BladeofJustice = 184575,
            AvengingWrath = 31884,
            Zeal = 217020,
            BladeofWrath = 202270,
            DivineHammer = 198034,
            Consecration = 26573,
            TyrsDeliverance = 200652,
            LightoftheMartyr = 183998,
            LightofDawn = 85222,
            HolyShock = 20473,
            HolyLight = 82326,
            DivineProtection = 498,
            Cleanse = 4987,
            BlessingofSacrifice = 6940,
            BeaconofLight = 53563,
            AvengingWrathHoly = 31842,
            AuraMastery = 31821,
            Absolution = 212056,
            GarrisonAbility = 161691,
            CombatAlly = 211390,
            WeaponSkills = 76294,
            TheQuickandtheDead = 83950,
            TheHumanSpirit = 20598,
            MountUp = 78633,
            Languages = 79738,
            HastyHearth = 83944,
            GuildMail = 83951,
            FlightMastersLicense = 90267,
            ExpertRiding = 34090,
            DraenorPathfinder = 191645,
            Diplomacy = 20599,
            ColdWeatherFlying = 54197,
            ArmorSkills = 76271,
            ApprenticeRiding = 33388,
            ReviveBattlePets = 125439,
            MobileBanking = 83958,
            EveryManforHimself = 59752,
            AutoAttack = 6603;

        #endregion

        #region DH Skills

        public const int
            ConsumeMagic = 183752,
            DemonSpikes = 203720,
            EmpowerWards = 218256,
            FelBlade = 213241,
            FelDevastation = 212084,
            FelEruption = 211811,
            FieryBrand = 204021,
            Fracture = 209795,
            ImmolationAura = 178740,
            Imprison = 217832,
            InfernalStrike = 189110,

            Shear = 203783,
            SigilofFlame = 204596,
            SigilofMisery = 207684,
            SigilofSilence = 202137,
            SoulCarver = 207407,
            SoulCleave = 228477,
            SpiritBomb = 218679,



//Vengeance
ShatteredSouls = 204254,
DoubleJump = 196055,
DemonicWards = 203513,
Blur = 198589,
ThrowGlaive = 185123,
SpectralSight = 188501,
ChaosStrike = 162794,
BladeDance = 188499,
DemonsBite = 162243,
Metamorphosis = 191427,
FelRush = 195072,
EyeBeam = 198013,
Glide = 131347,
ChaosNova = 179057,
Felblade = 232893,
VengefulRetreat = 198793,
ArcaneTorrent = 202719,

//Havoc
FuryoftheIllidari = 201467,
FelBarrage = 211053,
Darkness = 196718
            


            ;


        #endregion

        public const int
            SummonVoidwalker = 697,
            SummonSuccubus = 712,
            SummonImp = 688,
            SummonFelhunter = 691,
            SoulLeech = 108370,
            ShadowLock = 171138,
            SecretsoftheNecrolyte = 205183,
            UnstableAffliction = 30108,
            UnendingResolve = 104773,
            UnendingBreath = 5697,
            SummonInfernal = 1122,
            SummonDoomguard = 18540,
            Soulstone = 20707,
            SiphonLife = 63106,
            SeedofCorruption = 27243,
            RitualofSummoning = 698,
            ReapSouls = 216698,
            PhantomSingularity = 205179,
            MortalCoil = 6789,
            LifeTap = 1454,
            HealthFunnel = 755,
            Haunt = 48181,
            Fear = 5782,
            EyeofKilrogg = 126,
            EnslaveDemon = 1098,
            ShadowBolt = 232670,
            DemonicGateway = 111771,
            DrainLife = 689,
            DrainSoul = 198590,
            CreateSoulwell = 29893,
            CreateHealthstone = 6201,
            Corruption = 172,
            CommandDemon = 119898,
            Banish = 710,
            Agony = 980,
            Shoot = 5019,
            RocketJump = 69070,
            RocketBarrage = 69041,
            PackHobgoblin = 69046;





    }
}
