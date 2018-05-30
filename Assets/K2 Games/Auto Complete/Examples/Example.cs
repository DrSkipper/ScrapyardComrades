using UnityEngine;
using System.Collections;

public class Example : MonoBehaviour
{
    public enum ExampleEnum
    {
        Invalid = 0, Sword, Bow, Axe, Knife, Hammer, Arrow, Spear, ShortSword, Armour, Boots, Gloves, MoreGloves, HeyEvenMoreGloves, RunningOutOfIdeas,
        Random, Words, FortyTwo, Smeg, Space, Defence, Game, Thing, Bunny, Armadillo,
        A, Man, Walked, Into, Bar, Said, Ouch,
        Jeeze, With, Cherry, On, Top,
        Really, Long, Enum, Thats, Annoying, To, Scroll, And, Find, Values, Dont, You, Hate, That
    }

    public ExampleEnum[] recipe;
    public ExampleEnum recipeOutput;
}
