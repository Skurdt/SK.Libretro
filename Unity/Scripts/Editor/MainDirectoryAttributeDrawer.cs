/* MIT License

 * Copyright (c) 2022 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using SK.Libretro.Unity;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    [CustomPropertyDrawer(typeof(MainDirectoryAttribute))]
    public sealed class MainDirectoryAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _ = EditorGUI.BeginProperty(position, label, property);

            Rect buttonRect = new(position.x + 14f, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Reset Main Directory"))
                property.stringValue = BridgeSettings.DefaultMainDirectory;

            Rect totalRect     = new(position.x, buttonRect.y + buttonRect.height + 4f, position.width, EditorGUIUtility.singleLineHeight);
            Rect textFieldRect = EditorGUI.PrefixLabel(totalRect, new GUIContent(nameof(BridgeSettings.MainDirectory)));
            property.stringValue = GUI.TextField(textFieldRect, property.stringValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => (EditorGUIUtility.singleLineHeight * 2f) + 6f;
    }
}
