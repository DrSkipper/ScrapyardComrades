using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundData : ScriptableObject
{
    public List<AudioClip> ClipsByEnumIndex;
    public List<float> VolumeByEnumIndex;
    public List<float> PitchByEnumIndex;
    public List<int> CooldownsByEnumIndex;

    public enum Key
    {
        /**
         * Level 1 Hero
         */
        HeroLV1_Slide = 0,
        HeroLV1_SlideRecovery,
        HeroLV1_Jump1,
        HeroLV1_Jump2,
        HeroLV1_Jump3,
        HeroLV1_Jump4,
        HeroLV1_Jump5,
        HeroLV1_Falling,
        HeroLV1_Land,
        HeroLV1_LedgeGrab,
        HeroLV1_TouchWall,
        HeroLV1_WallSlide,
        HeroLV1_WallJump,
        HeroLV1_Jab1,
        HeroLV1_Jab2,
        HeroLV1_JabImpact,
        HeroLV1_Uppercut,
        HeroLV1_UppercutImpact,
        HeroLV1_Aerial,
        HeroLV1_AerialImpact,
        HeroLV1_Death,

        /**
         * Level 2 Hero
         */
        HeroLV2_Slide = 30,
        HeroLV2_SlideRecovery,
        HeroLV2_Jump1,
        HeroLV2_Jump2,
        HeroLV2_Jump3,
        HeroLV2_Jump4,
        HeroLV2_Jump5,
        HeroLV2_Falling,
        HeroLV2_Land,
        HeroLV2_LedgeGrab,
        HeroLV2_TouchWall,
        HeroLV2_WallSlide,
        HeroLV2_WallJump,
        HeroLV2_Jab1,
        HeroLV2_Jab2,
        HeroLV2_JabImpact,
        HeroLV2_Kick,
        HeroLV2_KickImpact,
        HeroLV2_Uppercut,
        HeroLV2_UppercutImpact,
        HeroLV2_Aerial,
        HeroLV2_AerialImpact,
        HeroLV2_Death,

        /**
         * Level 3 Hero
         */
        HeroLV3_Slide = 60,
        HeroLV3_SlideRecovery,
        HeroLV3_Jump1,
        HeroLV3_Jump2,
        HeroLV3_Jump3,
        HeroLV3_Jump4,
        HeroLV3_Jump5,
        HeroLV3_Falling,
        HeroLV3_Land,
        HeroLV3_LedgeGrab,
        HeroLV3_TouchWall,
        HeroLV3_WallSlide,
        HeroLV3_WallJump,
        HeroLV3_Jab1,
        HeroLV3_Jab2,
        HeroLV3_JabImpact,
        HeroLV3_Kick,
        HeroLV3_KickImpact,
        HeroLV3_Uppercut,
        HeroLV3_UppercutImpact,
        HeroLV3_Aerial,
        HeroLV3_AerialImpact,
        HeroLV3_Death,

        /**
         * Hero Level 4
         */
        HeroLV4_Dash = 90,
        HeroLV4_DashImpact,
        HeroLV4_Jump1,
        HeroLV4_Jump2,
        HeroLV4_Jump3,
        HeroLV4_Falling,
        HeroLV4_Land,
        HeroLV4_LedgeGrab,
        HeroLV4_TouchWall,
        HeroLV4_WallSlide,
        HeroLV4_WallJump,
        HeroLV4_Punch1,
        HeroLV4_Punch2,
        HeroLV4_PunchImpact,
        HeroLV4_Uppercut,
        HeroLV4_UppercutImpact,
        HeroLV4_GroundPoundSwing,
        HeroLV4_GroundPoundShockwave,
        HeroLV4_GroundPoundImpact,
        HeroLV4_Death,

        /**
         * Hero Level 5
         */
        HeroLV5_Charge = 120,
        HeroLV5_ChargeImpact,
        HeroLV5_Jump,
        HeroLV5_Falling,
        HeroLV5_Land,
        HeroLV5_TouchWall,
        HeroLV5_Punch1,
        HeroLV5_Punch2,
        HeroLV5_PunchImpact,
        HeroLV5_Sweep,
        HeroLV5_SweepImpact,
        HeroLV5_GroundPoundSwing,
        HeroLV5_GroundPoundShockwave,
        HeroLV5_GroundPoundImpact,
        HeroLV5_Death,

        /**
         * Enemy Level 1
         */
        EnemyLV1_Attract = 150,
        EnemyLV1_Land,
        EnemyLV1_Footsteps1,
        EnemyLV1_Footsteps2,
        EnemyLV1_Punch1,
        EnemyLV1_Punch2,
        EnemyLV1_PunchImpact,
        EnemyLV1_Death,

        /**
         * Enemy Level 2
         */
        EnemyLV2_Attract = 170,
        EnemyLV2_Jump,
        EnemyLV2_Land,
        EnemyLV2_Footsteps1,
        EnemyLV2_Footsteps2,
        EnemyLV2_Punch1,
        EnemyLV2_Punch2,
        EnemyLV2_PunchImpact,
        EnemyLV2_Death,
        EnemyLV2_JumpKick,
        EnemyLV2_JumpKickImpact,
        EnemyLV2_Throw,

        /**
         * Enemy Guard
         */
        EnemyGuard_Attract = 190,
        EnemyGuard_Block,
        EnemyGuard_Charge,
        EnemyGuard_ChargeImpact,
        EnemyGuard_ShieldBash,
        EnemyGuard_ShieldBashImpact,
        EnemyGuard_BatonSwing,
        EnemyGuard_BatonSwingImpact,
        EnemyGuard_OverheadSwing,
        EnemyGuard_OverheadSwingImpact,
        EnemyGuard_Footstep1,
        EnemyGuard_Footstep2,
        EnemyGuard_Death,

        /**
         * Enemy Mutant
         */
        EnemyMutant_Attract = 210,
        EnemyMutant_Charge,
        EnemyMutant_ChargeRoar,
        EnemyMutant_ChargeImpact,
        EnemyMutant_Footstep1,
        EnemyMutant_Footstep2,
        EnemyMutant_Punch1,
        EnemyMutant_Punch2,
        EnemyMutant_PunchImpact,
        EnemyMutant_Uppercut,
        EnemyMutant_UppercutImpact,
        EnemyMutant_GroundPoundSwing,
        EnemyMutant_GroundPoundShockwave,
        EnemyMutant_GroundPoundImpact,
        EnemyMutant_Death,
        EnemyMutant_Idle,

        /**
         * Syringe
         */
        Syringe_Break = 230,
        Syringe_HitImpact,

        /**
         * Stasis
         */
        Stasis_Bubbles = 235,
        Stasis_GlassCrack1,
        Stasis_GlassCrack2,
        Stasis_GlassCrack3,
        Stasis_GlassShatter,

        /**
         * Turret
         */
        Turret_Powerup = 245,
        Turret_Powerdown,
        Turret_Move,
        Turret_Target,
        Turret_Untarget,
        Turret_Fire,
        Turret_AttackedClink,
        Turret_Damaged,

        /**
         * Missile
         */
        Missile_Flying = 255,
        Missile_Explosion,
        Missile_Impact,

        /**
         * Mines
         */
        Mine_Explosion1 = 260,
        Mine_Explosion2,
        Mine_Explosion3,
        Mine_Regen,

        /**
         * Moving Platforms
         */
        MovingPlatform_Stop = 265,
        MovingPlatform_Start,
        MovingPlatform_Moving,

        /**
         * Block
         */
        Block_Hit = 270,
        Block_Land,
        Block_Explode,

        /**
         * Switch
         */
        Switch_PressedOn = 275,
        Switch_PressedOff,

        /**
         * Fan
         */
        Fan_PowerOn = 280,
        Fan_PowerOff,
        Fan_Blowing,

        /**
         * Conveyor
         */
        Conveyor_PowerOn = 285,
        Conveyor_PowerOff,
        Conveyor_Whirr,

        /**
         * Doors
         */
        Door_Open1 = 290,
        Door_Open2,
        Door_Close1,
        Door_Close2,

        /**
         * Misc
         */
        Misc_CoinAppear = 300,
        Misc_CoinPickup,
        Misc_ConsumeHeart,
        Misc_HeartAppear,
        Misc_HeartFloating,
        Misc_HealthBeep,
        Misc_AttritionBeep,
        Misc_Mutation1,
        Misc_Mutation2,
        Misc_Mutation3,
        Misc_Mutation4,
        Misc_Mutation5,
        Misc_EnableCheckpoint,
        Misc_CheckpointBubbling,

        /**
         * Event
         */
        Event_Siren = 320,
        Event_Carcrash,
        Event_BurningAmbulance,
        Event_FootShuffleIntro,
        Event_TrainRunning,
        Event_TrainWhistle,
        Event_TrainImpact,

        /**
         * Environment
         */
        Enviro_BirdChirp = 330,
        Enviro_BirdFlying,
        Enviro_StreetLightFlicker,
        Enviro_TrashFire,
        Enviro_NeonFlicker,
        Enviro_PaperBlowing,

        /**
         * UI
         */
        UI_PressStartChime = 340,
        UI_HighlightMove,
        UI_Select,

        /**
         * End
         */
        TOTAL
    }
}
