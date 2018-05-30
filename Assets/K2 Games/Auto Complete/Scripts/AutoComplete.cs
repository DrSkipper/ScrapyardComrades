using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Functionality for displaying an editor AutoComplete box that will select a word from a dictionary of strings or an enum.
/// 
/// Author: Jason Guthrie
/// Company: K2 Games
/// Website: www.facebook.com/K2Games
/// </summary>
public class AutoComplete
{
    const int ClosestWordsToShow = 3;//how many results do we display

    public static List<string[]> acceptedWords = new List<string[]>();
    List<float> matchCount = new List<float>();//how many letters we matched for each word
    List<float> bestMatches = new List<float>();
    List<int> bestMatchIndexes = new List<int>();
    public string enteredText = "";

    public string[] results;//these can be accessed directly instead of displaying the buttons

    #region Key Presses
    int selectedIndex;//for highlighting with the keyboard
    KeyCode keyHit;
    #endregion

    /// <summary>
    /// Sets a dictionary from an enum
    /// </summary>
    /// <param name="enumType">Your enum. To use this call SetDictionary(typeof(YourEnum));</param>
    public static int SetDictionary(Type enumType)
    {
        acceptedWords.Add(Enum.GetNames(enumType));
        return acceptedWords.Count - 1;
    }

    /// <summary>
    /// Set a dictionary from a strig array of words instead
    /// </summary>
    /// <param name="AcceptedWords"></param>
    public static void SetDictionary(string[] AcceptedWords)
    {
        acceptedWords.Add(AcceptedWords);
    }

    public string[] CheckWord(string current)
    {
        return CheckWord(current, 0);
    }

    /// <summary>
    /// Checks a word against a list of given words and returns the closest matches to that word
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public string[] CheckWord(string current, int dictionary)
    {
        if(current == null || current.Length == 0)
            return null;

        matchCount.Clear();//clear existing matches
        bestMatches.Clear();
        bestMatchIndexes.Clear();

        #region Determine Fitness
        for(int i = 0; i < acceptedWords[dictionary].Length; i++)
            matchCount.Add(CompareFitness(current, acceptedWords[dictionary][i].ToLower()));//add the nubmer of characters that match to this list
        #endregion

        for(int i = 0; i < matchCount.Count; i++)//now compare the results to find the best ones
            if(matchCount[i] > 0)//ignore 0's
            {
                bool inserted = false;

                if(bestMatches.Count == 0)//if the list is empty just stick in the match
                {
                    bestMatches.Add(matchCount[i]);
                    bestMatchIndexes.Add(i);//store the index as well
                    inserted = true;
                }

                for(int j = 0; j < bestMatches.Count; j++)//loop for all potential results
                    if(matchCount[i] > bestMatches[j])//if better than the previous match
                    {
                        bestMatches.Insert(j, matchCount[i]);//nudge up the list
                        bestMatchIndexes.Insert(j, i);//and dont forget the indexes
                        inserted = true;
                        break;
                    }

                if(!inserted)//if nothing has been inserted
                {
                    bestMatches.Add(matchCount[i]);//then stick this result on the end
                    bestMatchIndexes.Add(i);
                }
            }

        results = new string[ClosestWordsToShow];//store the results

        for(int i = 0; i < results.Length && i < bestMatchIndexes.Count; i++)
            results[i] = acceptedWords[dictionary][bestMatchIndexes[i]];//basically grab the closest words

        return results;//return the results
    }

    /// <summary>
    /// Basically compares two strings and counts how many letters they have that are the same
    /// </summary>
    public float CompareFitness(string individual, string target)
    {
        float sum = 0;

        for(int i = 0; i < individual.Length; i++)
            for(int j = 0; j < target.Length; j++)
                if(individual[i] == target[j])
                {
                    sum++;

                    if(i == j)//if the letters are in the same position
                        sum++;//add slightly more fitness to appear higher than other items
                }

        return sum / (float)target.Length;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Call this from any editor script to display the box
    /// </summary>
    public void DisplayBox(int dictionary)
    {
        Event currentEvent = Event.current;
        GUILayout.BeginVertical();

        #region Key Presses
        if(results != null && results.Length > 0 && enteredText.Length > 0)//and if there are valid entries for what they have entered so far
        {
            if(currentEvent.type == EventType.KeyUp)
            {
                keyHit = currentEvent.keyCode;

                if(EditorWindow.focusedWindow != null)
                    EditorWindow.focusedWindow.Repaint();//repaint when you hit enter to animate the change of UI nicely
            }

            if(currentEvent.type == EventType.Layout)//wait until input has passed
            {
                #region Enter
                if(keyHit == KeyCode.Return)
                {
                    enteredText = results[selectedIndex];//select the result

                    if(EditorWindow.focusedWindow != null)
                        EditorWindow.focusedWindow.Repaint();
                }
                #endregion


                #region Down Arrow
                if(keyHit == KeyCode.DownArrow)
                    do
                        selectedIndex = (selectedIndex + 1) % results.Length;
                    while(results[selectedIndex] == null);
                #endregion

                #region Up Arrow
                if(keyHit == KeyCode.UpArrow)
                    do
                    {
                        selectedIndex--;

                        if(selectedIndex < 0)
                            selectedIndex = results.Length - 1;
                    }
                    while(results[selectedIndex] == null);
                #endregion

                keyHit = KeyCode.None;
            }
        }
        else
            selectedIndex = 0;
        #endregion

        enteredText = EditorGUILayout.TextField(enteredText);

        CheckWord(enteredText, dictionary);

        if(results != null && enteredText.Length > 0)
            for(int i = 0; i < results.Length; i++)
                if(results[i] != null)
                {
                    if(i == selectedIndex)
                        GUI.color = Color.yellow;

                    if(GUILayout.Button(results[i]))//if the button is pressed
                        enteredText = results[i];//update the text field

                    GUI.color = Color.white;
                }

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Converts string into an enum value. Since this involves a try catch this can be a little expensive.
    /// </summary>
    /// <param name="text">The text to convert</param>
    /// <param name="enumType">Your enum converted to a type. E.G typeof(YourEnum)</param>
    /// <param name="undefinedValue">What value to return if the text is not contained in this enum</param>
    /// <returns></returns>
    static Enum ConvertEnum(string text, Type enumType, Enum undefinedValue)
    {
        try
        {
            Enum temp = (Enum)System.Enum.Parse(enumType, text);

            return temp;
        }
        catch(System.Exception e)
        {
            return undefinedValue;
        }
    }

    public static Enum DisplayBox(Type enumType, Enum currentValue, Enum undefinedValue, AutoComplete box)
    {
        return DisplayBox(enumType, currentValue, undefinedValue, box, 0);
    }

    /// <summary>
    /// Displays an example box to search for your enum.
    /// </summary>
    /// <param name="enumType">Use typeof(YourEnum)</param>
    /// <param name="currentValue">Your current enum value</param>
    /// <param name="undefinedValue">What to return when the entered text isn't in your enum</param>
    /// <param name="box">Which instance of AutoComplete will handle this enum</param>
    /// <returns></returns>
    public static Enum DisplayBox(Type enumType, Enum currentValue, Enum undefinedValue, AutoComplete box, int dictionary)
    {
        if(currentValue.Equals(undefinedValue))//if there is currently no value stored for the enum
        {
            #region Text Box
            box.DisplayBox(dictionary);//display the box

            Enum temp = ConvertEnum(box.enteredText, enumType, undefinedValue);//try and convert the entered text to an enum

            if(temp != undefinedValue)//if the entered text is valid then store it
                currentValue = temp;
            #endregion
        }
        else
        {
            #region Enum Popup
            GUILayout.BeginHorizontal();

            currentValue = (Enum)EditorGUILayout.EnumPopup(currentValue);//a value has been displayed so just display a standard enum field

            if(GUILayout.Button("-", GUILayout.MaxWidth(20)))//also display a button to remove the enum if it needs changed
            {
                currentValue = undefinedValue;
                box.enteredText = "";
            }

            GUILayout.EndHorizontal();
            #endregion
        }

        return currentValue;
    }
#endif
}
