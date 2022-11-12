/* MIT License

 * Copyright (c) 2021-2022 Skurdt
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

namespace SK.Libretro
{
    [System.Serializable]
    public sealed class Option
    {
        public string Key { get; private set; }
        public string Description { get; private set; }
        public string Info { get; private set; }
        public string CurrentValue { get; private set; }
        public string[] PossibleValues { get; private set; }
        public bool Visible { get; private set; } = true;

        internal Option(string line)
        : this(line.Split(';'))
        {
        }

        internal Option(string[] lineSplit)
        {
            Update(lineSplit);
            CurrentValue = lineSplit[2];
        }

        internal Option(string key, string[] lineSplit)
        {
            Update(key, lineSplit);
            CurrentValue = lineSplit[1].Trim().Split('|')[0];
        }

        internal Option(string key, string description, string info, string value, string[] possibleValues)
        {
            Update(key, description, info, possibleValues);
            CurrentValue = value;
        }

        public void Update(int index)
        {
            if (PossibleValues is null || PossibleValues.Length == 0)
                return;

            int clampedIndex = index.Clamp(0, PossibleValues.Length - 1);
            CurrentValue = PossibleValues[clampedIndex];
        }

        internal void Update(string key, string[] lineSplit)
        {
            Key            = key;
            Description    = lineSplit[0];
            PossibleValues = lineSplit[1].Trim().Split('|');
        }

        internal void Update(string[] lineSplit)
        {
            Key            = lineSplit[0];
            Description    = lineSplit[1];
            PossibleValues = lineSplit[3].Trim().Split('|');
        }

        internal void Update(string key, string description, string info, string[] possibleValues)
        {
            Key            = key;
            Description    = description;
            Info           = info;
            PossibleValues = possibleValues;
        }

        internal void SetVisibility(bool visible) => Visible = visible;
    }
}
