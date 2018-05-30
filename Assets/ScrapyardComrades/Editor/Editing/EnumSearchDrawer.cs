using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

//Source: https://stackoverflow.com/questions/36880094/unity-gui-combo-box-with-search

[CustomPropertyDrawer(typeof(SoundData.Key), false)]
public class EnumSearchPropertyDrawer : PropertyDrawer
{

    struct EnumStringValuePair : IComparable<EnumStringValuePair>
    {
        public string strValue;
        public int intValue;

        public int CompareTo(EnumStringValuePair another)
        {
            if (intValue < another.intValue)
                return -1;
            else if (intValue > another.intValue)
                return 1;
            return 0;
        }
    }

    Dictionary<int, string> filters = new Dictionary<int, string>();
    //string filter = string.Empty;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int y = (int)position.position.y;
        if (!filters.ContainsKey(y))
            filters[y] = string.Empty;
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(new Rect(position.x, position.y, 100, 20), property.name);
        filters[y] = EditorGUI.TextField(new Rect(position.x + 100, position.y, 60, 15), filters[y]);
        List<EnumStringValuePair> enumList = GetEnumList(filters[y]);
        List<string> enumStrList = new List<string>(enumList.Count);
        for (int i = 0; i < enumList.Count; ++i)
        {
            enumStrList.Add(enumList[i].strValue);
        }
        int selectedIndex = 0;
        for (int i = 0; i < enumList.Count; ++i)
        {
            if (enumList[i].intValue == property.intValue)
            {
                selectedIndex = i;
                break;
            }
        }
        selectedIndex = EditorGUI.Popup(new Rect(position.x + 170, position.y, 200, 20), selectedIndex, enumStrList.ToArray());
        if (!StringExtensions.IsEmpty(filters[y]) && enumList.Count > selectedIndex && selectedIndex >= 0)
        {
            property.intValue = enumList[selectedIndex].intValue;
        }
        EditorGUI.EndProperty();
    }

    List<EnumStringValuePair> allList = null;

    private List<EnumStringValuePair> GetEnumList(string filter)
    {
        if (allList == null)
        {
            allList = new List<EnumStringValuePair>();
            Array enumValues = Enum.GetValues(typeof(SoundData.Key));

            for (int i = 0; i < enumValues.Length - 1; ++i)
            {
                EnumStringValuePair pair = new EnumStringValuePair();
                pair.strValue = enumValues.GetValue(i).ToString();
                pair.intValue = (int)enumValues.GetValue(i);

                allList.Add(pair);
            }
        }

        List<EnumStringValuePair> ret = new List<EnumStringValuePair>();
        Regex regex = new Regex(filter.ToLower().Trim('(', ')', '\\'));
        for (int i = 0; i < allList.Count; ++i)
        {
            if (regex.IsMatch(allList[i].strValue.ToLower()))
            {
                ret.Add(allList[i]);
            }
        }
        return ret;
    }

}
