using UnityEngine;
using System.Collections.Generic;

public class EnemyConfigurer : ObjectConfigurer
{
    private const string NAME = "Enemy";
    private const string VARIANT_TYPE = "variant";

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
    public SCCharacterController CharacterController;
    public CharacterVisualizer CharacterVisualizer;
    public SCSpriteAnimator SpriteAnimator;

    public override ObjectParamType[] ParameterTypes
    {
        get
        {
            List<string> variantNames = new List<string>();
            if (this.Variants != null)
            {
                for (int i = 0; i < this.Variants.Length; ++i)
                {
                    variantNames.Add(this.Variants[i].Name);
                }
            }

            return new ObjectParamType[] {
                new ObjectParamType(VARIANT_TYPE, variantNames.ToArray())
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
}
