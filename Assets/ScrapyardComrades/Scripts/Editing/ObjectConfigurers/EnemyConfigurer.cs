using UnityEngine;
using System.Collections.Generic;

public class EnemyConfigurer : ObjectConfigurer
{
    private const string NAME = "Enemy";
    private const string VARIANT_TYPE = "variant";
    private const string TARGET_TYPE = "target";
    private const string TARGETS_PLAYER = "player";
    private const string TARGETS_ALL = "all";
    private const string HEALTH_REDUCTION = "hp_reduce";

    [System.Serializable]
    public struct Variant
    {
        public string Name;
        public SCMoveSet MoveSet;
        public SCSpriteAnimation IdleAnimation;
        public SCSpriteAnimation RunAnimation;
        public SCSpriteAnimation JumpAnimation;
        public SCSpriteAnimation FallAnimation;
        public SCSpriteAnimation WallSlideAnimation;
        public SCSpriteAnimation LedgeGrabAnimation;
        public SCSpriteAnimation LedgeGrabBackAnimation;
        public SCSpriteAnimation DuckAnimation;
        public SCSpriteAnimation HitStunAnimation;
        public SCSpriteAnimation DeathHitStunAnimation;
        public SCSpriteAnimation DeathAnimation;
    }

    public Variant[] Variants;
    public EnemyController CharacterController;
    public AttackController AttackController;
    public CharacterVisualizer CharacterVisualizer;
    public SCSpriteAnimator SpriteAnimator;
    public LayerMask AllTargetMask;
    public LayerMask PlayerDamagableMask;
    public LayerMask AllDamagableMask;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            List<string> variantNames = new List<string>();
            if (this.Variants != null)
            {
                for (int i = 0; i < this.Variants.Length; ++i)
                    variantNames.Add(this.Variants[i].Name);
            }

            return new ObjectParamType[] {
                new ObjectParamType(VARIANT_TYPE,
                    variantNames.ToArray()
                ),
                new ObjectParamType(TARGET_TYPE, new string[] {
                    TARGETS_PLAYER,
                    TARGETS_ALL
                }),
                new ObjectParamType(HEALTH_REDUCTION, new string[] {
                    SwitchConfigurer.FALSE,
                    SwitchConfigurer.TRUE
                })
            };
        }
    }

    protected override void ConfigureParameter(string parameterName, string option)
    {
        switch (parameterName)
        {
            default:
                LogInvalidParameter(NAME, parameterName, option);
                break;
            case VARIANT_TYPE:
                configureVariantType(option);
                break;
            case TARGET_TYPE:
                configureTargetType(option);
                break;
            case HEALTH_REDUCTION:
                configureHealthReduction(option);
                break;
        }
    }
    /**
     * Private
     */
    private void configureVariantType(string option)
    {
        bool found = false;
        if (this.Variants != null)
        {
            for (int i = 0; i < this.Variants.Length; ++i)
            {
                Variant v = this.Variants[i];
                if (v.Name == option)
                {
                    found = true;

                    this.CharacterController.MoveSet = v.MoveSet;
                    this.CharacterVisualizer.IdleAnimation = v.IdleAnimation;
                    this.CharacterVisualizer.RunAnimation = v.RunAnimation;
                    this.CharacterVisualizer.JumpAnimation = v.JumpAnimation;
                    this.CharacterVisualizer.FallAnimation = v.FallAnimation;
                    this.CharacterVisualizer.WallSlideAnimation = v.WallSlideAnimation;
                    this.CharacterVisualizer.LedgeGrabAnimation = v.LedgeGrabAnimation;
                    this.CharacterVisualizer.LedgeGrabBackAnimation = v.LedgeGrabBackAnimation;
                    this.CharacterVisualizer.DuckAnimation = v.DuckAnimation;
                    this.CharacterVisualizer.HitStunAnimation = v.HitStunAnimation;
                    this.CharacterVisualizer.DeathHitStunAnimation = v.DeathHitStunAnimation;
                    this.CharacterVisualizer.DeathAnimation = v.DeathAnimation;
                    this.SpriteAnimator.DefaultAnimation = v.IdleAnimation;
                    break;
                }
            }
        }

        if (!found)
            LogInvalidParameter(NAME, VARIANT_TYPE, option);
    }

    private void configureTargetType(string option)
    {
        switch (option)
        {
            default:
                LogInvalidParameter(NAME, TARGET_TYPE, option);
                this.CharacterController.Targets = EnemyController.TargetType.Player;
                this.CharacterController.TargetLayers = 0;
                this.AttackController.DamagableLayers = this.PlayerDamagableMask;
                break;
            case TARGETS_PLAYER:
                this.CharacterController.Targets = EnemyController.TargetType.Player;
                this.CharacterController.TargetLayers = 0;
                this.AttackController.DamagableLayers = this.PlayerDamagableMask;
                break;
            case TARGETS_ALL:
                this.CharacterController.Targets = EnemyController.TargetType.SpecifiedLayers;
                this.CharacterController.TargetLayers = this.AllTargetMask;
                this.AttackController.DamagableLayers = this.AllDamagableMask;
                break;
        }
    }

    private void configureHealthReduction(string option)
    {
        this.CharacterController.ReduceHealth = (option == SwitchConfigurer.TRUE);
    }
}
