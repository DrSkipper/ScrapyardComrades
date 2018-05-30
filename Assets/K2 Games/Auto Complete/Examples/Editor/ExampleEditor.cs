#define StoreVersion

using UnityEngine;
using System.Collections;
using UnityEditor;
#if(!StoreVersion)//I like to keep everything in the framework together. Feel free to uncomment this if you do to
using K2Framework;
#endif

[CustomEditor(typeof(Example))]

/// <summary>
/// An example class for displaying some AutoComplete boxes.
/// Note that with this functionality the enumes are stored in the class but any entered text will be lost when you hit play or select a new object.
/// 
/// Author: Jason Guthrie
/// Company: K2 Games
/// Website: www.facebook.com/K2Games
/// </summary>
public class ExampleEditor : Editor
{
    #region Variables
    AutoComplete[] minecraftStyleCraftingRecipe;
    AutoComplete outputBox;

    SerializedProperty recipeOutput;//a quick example to grab data from your class directly for those not so familiar with inspector/editor scripts
    #endregion

    #region Methods
    void OnEnable()
    {
        #region Instantiate AutoComplete Boxes
        minecraftStyleCraftingRecipe = new AutoComplete[9];//define our recipe grid

        for (int i = 0; i < minecraftStyleCraftingRecipe.Length; i++)//and set some defualt values to avoid null pointer errors
            minecraftStyleCraftingRecipe[i] = new AutoComplete();

        outputBox = new AutoComplete();
        #endregion

        #region Instantiate Recipe Data
        Example current = (Example)target;

        if (current.recipe == null || current.recipe.Length == 0)//if there is no data in the class
            current.recipe = new Example.ExampleEnum[9];//initialise the recipe. No need to initiliase the enum values they will be set by defualt
        #endregion

        recipeOutput = serializedObject.FindProperty("recipeOutput");//grab the data directly from your class. The string here is the name of the variable in question. Accessing data like this is important for linking with Undo and Redo features of Unity

        AutoComplete.SetDictionary(typeof(Example.ExampleEnum));//Load the autocomplete dictionary with your enum. This will also accept a string array
    }

    public override void OnInspectorGUI()
    {
        #region Initialising Editor Data
        serializedObject.Update();//needed to access the data in your class
        Example current = (Example)target;//grab the object currently viewed and convert to a usable format. Working in this way is simple but it will Not allow you to undo and redo any changes you make.
        #endregion

        #region Crafting Grid Example
        GUILayout.Space(25);//Pad out our grid to make it look nicer

        #region Top Row
        GUILayout.BeginHorizontal();//make a horizontal group

        for (int i = 0; i < 3; i++)
            DisplayItem(current, i);//display some text boxes

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(15);

        #region Middle Row
        GUILayout.BeginHorizontal();

        for (int i = 3; i < 6; i++)
            DisplayItem(current, i);

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(15);

        #region Bottom Row
        GUILayout.BeginHorizontal();

        for (int i = 6; i < 9; i++)
            DisplayItem(current, i);

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(25);
        #endregion

        #region Single Box Example
        GUILayout.BeginHorizontal();
        GUILayout.Label("Output Type");

        recipeOutput.enumValueIndex = (int)((Example.ExampleEnum)AutoComplete.DisplayBox(typeof(Example.ExampleEnum),//first define the type of your enum
            (Example.ExampleEnum)recipeOutput.enumValueIndex,//the enum value in question The casting gets a little messy when you work with SerialisedProperties
            Example.ExampleEnum.Invalid,//what to return when the text isn't in the enum
            outputBox));//the AutoComplete instance

        GUILayout.EndHorizontal();
        #endregion

        #region Finalising Editor Data
        if (GUI.changed)
            EditorUtility.SetDirty(target);

        serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        #endregion
    }

    /// <summary>
    /// An example helper function so the box can be called with one line of code
    /// </summary>
    /// <param name="current">Which class are we editing</param>
    /// <param name="index">Which AutoComplete box is being displayed</param>
    void DisplayItem(Example current, int index)
    {
        current.recipe[index] = (Example.ExampleEnum)AutoComplete.DisplayBox(typeof(Example.ExampleEnum),//so display our box. First tell it what sort of enum we are dealing with, and cast it for your output 
            current.recipe[index],//the enum item in question
            Example.ExampleEnum.Invalid, //what to return if the entered text isn't in the enum
            minecraftStyleCraftingRecipe[index]);//and the AutoComplete box instance

        GUILayout.Space(25);//pad out the GUI to make it look nicer
    }
    #endregion
}
