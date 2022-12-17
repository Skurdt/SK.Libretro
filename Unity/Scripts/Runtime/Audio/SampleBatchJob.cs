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

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace SK.Libretro.Unity
{
    [BurstCompile]
    internal unsafe struct SampleBatchJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableUnsafePtrRestriction] public short* SourceSamples;
        [WriteOnly] public NativeArray<float> DestinationSamples;
        [ReadOnly] public int SourceSampleRate;
        [ReadOnly] public int TargetSampleRate;

        public void Execute(int index)
        {
            float sampleIndex         = index * (float)SourceSampleRate / TargetSampleRate;
            int sampleIndex1          = (int)math.floor(sampleIndex);
            int sampleIndex2          = (int)math.ceil(sampleIndex);
            float interpolationFactor = sampleIndex - sampleIndex1;
            DestinationSamples[index] = math.lerp(SourceSamples[sampleIndex1] * AudioHandler.NORMALIZED_GAIN,
                                                  SourceSamples[sampleIndex2] * AudioHandler.NORMALIZED_GAIN,
                                                  interpolationFactor);
        }
    }
}
