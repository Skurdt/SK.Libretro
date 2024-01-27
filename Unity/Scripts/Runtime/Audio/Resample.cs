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

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal unsafe class Resampler
    {
        public static void LinearResample(short* sourceSamples, int sourceLength, NativeArray<float> destinationSamples,
                                          int destinationLength, int sourceSampleRate, int targetSampleRate)
        {
            float ratio = (float)targetSampleRate / (float)sourceSampleRate;
            int lastSourceSample = sourceLength - 1;
            int lastDestinationSample = destinationLength - 1;

            for (int i = 0; i < destinationSamples.Length; i++)
            {
                float sourceIndex = i / ratio;
                int leftSampleIndex = Mathf.FloorToInt(sourceIndex);
                int rightSampleIndex = Mathf.CeilToInt(sourceIndex);
                float lerp = sourceIndex - leftSampleIndex;

                if (rightSampleIndex > lastSourceSample)
                {
                    rightSampleIndex = lastSourceSample;
                }

                if (leftSampleIndex > lastSourceSample)
                {
                    leftSampleIndex = lastSourceSample;
                }

                destinationSamples[i] = Mathf.Lerp(sourceSamples[leftSampleIndex] * AudioHandler.NORMALIZED_GAIN,
                                                  sourceSamples[rightSampleIndex] * AudioHandler.NORMALIZED_GAIN,
                                                  lerp);
            }
        }

        private static float CubicInterpolate(float v0, float v1, float v2, float v3, float x)
        {
            float P = (v3 - v2) - (v0 - v1);
            float Q = (v0 - v1) - P;
            float R = v2 - v0;
            float S = v1;
            return P * x * x * x + Q * x * x + R * x + S;
        }

        public static void CubicResample(short* sourceSamples, NativeArray<float> destinationSamples,
                             int sourceSampleRate, int targetSampleRate, int index)
        {
            float sampleIndex = index * (float)sourceSampleRate / targetSampleRate;
            int sampleIndex1 = (int)math.floor(sampleIndex);
            int sampleIndex0 = math.clamp(sampleIndex1 - 1, 0, sourceSampleRate - 1);
            int sampleIndex2 = math.clamp(sampleIndex1 + 1, 0, sourceSampleRate - 1);
            int sampleIndex3 = math.clamp(sampleIndex1 + 2, 0, sourceSampleRate - 1);
            float fractionalIndex = sampleIndex - sampleIndex1;
            float v0 = sourceSamples[sampleIndex0] * AudioHandler.NORMALIZED_GAIN;
            float v1 = sourceSamples[sampleIndex1] * AudioHandler.NORMALIZED_GAIN;
            float v2 = sourceSamples[sampleIndex2] * AudioHandler.NORMALIZED_GAIN;
            float v3 = sourceSamples[sampleIndex3] * AudioHandler.NORMALIZED_GAIN;
            destinationSamples[index] = CubicInterpolate(v0, v1, v2, v3, fractionalIndex);
        }
    }
}
