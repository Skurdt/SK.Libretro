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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal static partial class Vulkan
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkComponentMapping
        {
            public VkComponentSwizzle r;
            public VkComponentSwizzle g;
            public VkComponentSwizzle b;
            public VkComponentSwizzle a;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageSubresourceRange
        {
            public uint aspectMask;
            public uint baseMipLevel;
            public uint levelCount;
            public uint baseArrayLayer;
            public uint layerCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageViewCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkImageViewCreateFlags
            public IntPtr image; // VkImage
            public VkImageViewType viewType;
            public VkFormat format;
            public VkComponentMapping components;
            public VkImageSubresourceRange subresourceRange;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkApplicationInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public IntPtr pApplicationName; // const char*
            public uint applicationVersion;
            public IntPtr pEngineName; // const char*
            public uint engineVersion;
            public uint apiVersion;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkPhysicalDeviceFeatures
        {
            public uint robustBufferAccess;
            public uint fullDrawIndexUint32;
            public uint imageCubeArray;
            public uint independentBlend;
            public uint geometryShader;
            public uint tessellationShader;
            public uint sampleRateShading;
            public uint dualSrcBlend;
            public uint logicOp;
            public uint multiDrawIndirect;
            public uint drawIndirectFirstInstance;
            public uint depthClamp;
            public uint depthBiasClamp;
            public uint fillModeNonSolid;
            public uint depthBounds;
            public uint wideLines;
            public uint largePoints;
            public uint alphaToOne;
            public uint multiViewport;
            public uint samplerAnisotropy;
            public uint textureCompressionETC2;
            public uint textureCompressionASTC_LDR;
            public uint textureCompressionBC;
            public uint occlusionQueryPrecise;
            public uint pipelineStatisticsQuery;
            public uint vertexPipelineStoresAndAtomics;
            public uint fragmentStoresAndAtomics;
            public uint shaderTessellationAndGeometryPointSize;
            public uint shaderImageGatherExtended;
            public uint shaderStorageImageExtendedFormats;
            public uint shaderStorageImageMultisample;
            public uint shaderStorageImageReadWithoutFormat;
            public uint shaderStorageImageWriteWithoutFormat;
            public uint shaderUniformBufferArrayDynamicIndexing;
            public uint shaderSampledImageArrayDynamicIndexing;
            public uint shaderStorageBufferArrayDynamicIndexing;
            public uint shaderStorageImageArrayDynamicIndexing;
            public uint shaderClipDistance;
            public uint shaderCullDistance;
            public uint shaderFloat64;
            public uint shaderInt64;
            public uint shaderInt16;
            public uint shaderResourceResidency;
            public uint shaderResourceMinLod;
            public uint sparseBinding;
            public uint sparseResidencyBuffer;
            public uint sparseResidencyImage2D;
            public uint sparseResidencyImage3D;
            public uint sparseResidency2Samples;
            public uint sparseResidency4Samples;
            public uint sparseResidency8Samples;
            public uint sparseResidency16Samples;
            public uint sparseResidencyAliased;
            public uint variableMultisampleRate;
            public uint inheritedQueries;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkInstanceCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkInstanceCreateFlags
            public IntPtr pApplicationInfo; // const VkApplicationInfo*
            public uint enabledLayerCount;
            public IntPtr ppEnabledLayerNames; // const char* const*
            public uint enabledExtensionCount;
            public IntPtr ppEnabledExtensionNames; // const char* const*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkDeviceCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkDeviceCreateFlags
            public uint queueCreateInfoCount;
            public IntPtr pQueueCreateInfos; // const VkDeviceQueueCreateInfo*
            public uint enabledLayerCount;
            public IntPtr ppEnabledLayerNames; // const char* const*
            public uint enabledExtensionCount;
            public IntPtr ppEnabledExtensionNames; // const char* const*
            public IntPtr pEnabledFeatures; // const VkPhysicalDeviceFeatures*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkBaseInStructure
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkQueueFamilyProperties
        {
            public uint queueFlags; // VkQueueFlags
            public uint queueCount;
            public uint timestampValidBits;
            public VkExtent3D minImageTransferGranularity;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkDeviceQueueCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkDeviceQueueCreateFlags
            public uint queueFamilyIndex;
            public uint queueCount;
            public IntPtr pQueuePriorities; // const float*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSurfaceCapabilitiesKHR
        {
            public uint minImageCount;
            public uint maxImageCount;
            public VkExtent2D currentExtent;
            public VkExtent2D minImageExtent;
            public VkExtent2D maxImageExtent;
            public uint maxImageArrayLayers;
            public uint supportedTransforms; // VkSurfaceTransformFlagsKHR
            public uint currentTransform; // VkSurfaceTransformFlagBitsKHR
            public uint supportedCompositeAlpha; // VkCompositeAlphaFlagsKHR
            public uint supportedUsageFlags; // VkImageUsageFlags
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSurfaceFormatKHR
        {
            public VkFormat format;
            public VkColorSpaceKHR colorSpace;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkExtent2D
        {
            public uint width;
            public uint height;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkExtent3D
        {
            public uint width;
            public uint height;
            public uint depth;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSwapchainCreateInfoKHR
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkSwapchainCreateFlagsKHR
            public IntPtr surface; // VkSurfaceKHR
            public uint minImageCount;
            public VkFormat imageFormat;
            public VkColorSpaceKHR imageColorSpace;
            public VkExtent2D imageExtent;
            public uint imageArrayLayers;
            public uint imageUsage; // VkImageUsageFlags
            public VkSharingMode imageSharingMode;
            public uint queueFamilyIndexCount;
            public IntPtr pQueueFamilyIndices;
            public uint preTransform; // VkSurfaceTransformFlagBitsKHR
            public uint compositeAlpha; // VkCompositeAlphaFlagBitsKHR
            public VkPresentModeKHR presentMode;
            public uint clipped;
            public IntPtr oldSwapchain; // VkSwapchainKHR
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSemaphoreCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkSemaphoreCreateFlags
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkCommandPoolCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkCommandPoolCreateFlags
            public uint queueFamilyIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkCommandBufferAllocateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public IntPtr commandPool; // VkCommandPool
            public VkCommandBufferLevel level;
            public uint commandBufferCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkFenceCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkFenceCreateFlags
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkBufferCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkBufferCreateFlags
            public ulong size;
            public uint usage; // VkBufferUsageFlags
            public VkSharingMode sharingMode;
            public uint queueFamilyIndexCount;
            public IntPtr pQueueFamilyIndices; // const uint32_t*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkMemoryRequirements
        {
            public ulong size;
            public ulong alignment;
            public uint memoryTypeBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkMemoryAllocateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public ulong allocationSize;
            public uint memoryTypeIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct VkPhysicalDeviceMemoryProperties
        {
            public const int VK_MAX_MEMORY_TYPES = 32;
            public const int VK_MAX_MEMORY_HEAPS = 16;
            public uint memoryTypeCount;
            public fixed uint memoryTypes[VK_MAX_MEMORY_TYPES * 2]; // propertyFlags, heapIndex for each VkMemoryType
            public uint memoryHeapCount;
            public fixed ulong memoryHeaps[VK_MAX_MEMORY_HEAPS * 2]; // size, flags for each VkMemoryHeap
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkBufferViewCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkBufferViewCreateFlags
            public IntPtr buffer; // VkBuffer
            public VkFormat format;
            public uint offset;
            public uint range;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkImageCreateFlags
            public VkImageType imageType;
            public VkFormat format;
            public VkExtent3D extent;
            public uint mipLevels;
            public uint arrayLayers;
            public VkSampleCountFlagBits samples;
            public VkImageTiling tiling;
            public uint usage; // VkImageUsageFlags
            public VkSharingMode sharingMode;
            public uint queueFamilyIndexCount;
            public IntPtr pQueueFamilyIndices; // const uint32_t*
            public VkImageLayout initialLayout;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkMemoryType
        {
            public uint propertyFlags; // VkMemoryPropertyFlags
            public uint heapIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkFormatProperties
        {
            public uint linearTilingFeatures; // VkFormatFeatureFlags
            public uint optimalTilingFeatures; // VkFormatFeatureFlags
            public uint bufferFeatures; // VkFormatFeatureFlags
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkCommandBufferBeginInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint flags; // VkCommandBufferUsageFlags
            public IntPtr pInheritanceInfo; // const VkCommandBufferInheritanceInfo*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageMemoryBarrier
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint srcAccessMask; // VkAccessFlags
            public uint dstAccessMask; // VkAccessFlags
            public VkImageLayout oldLayout;
            public VkImageLayout newLayout;
            public uint srcQueueFamilyIndex;
            public uint dstQueueFamilyIndex;
            public IntPtr image; // VkImage
            public VkImageSubresourceRange subresourceRange;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageSubresourceLayers
        {
            public uint aspectMask; // VkImageAspectFlags
            public uint mipLevel;
            public uint baseArrayLayer;
            public uint layerCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkOffset3D
        {
            public int x;
            public int y;
            public int z;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct VkImageBlit
        {
            public VkImageSubresourceLayers srcSubresource;
            public fixed int srcOffsets[6]; // 2 * VkOffset3D (x, y, z) each
            public VkImageSubresourceLayers dstSubresource;
            public fixed int dstOffsets[6]; // 2 * VkOffset3D (x, y, z) each
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkBufferImageCopy
        {
            public ulong bufferOffset;
            public uint bufferRowLength;
            public uint bufferImageHeight;
            public VkImageSubresourceLayers imageSubresource;
            public VkOffset3D imageOffset;
            public VkExtent3D imageExtent;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSubmitInfo
        {
            public VkStructureType sType;
            public IntPtr pNext; // const void*
            public uint waitSemaphoreCount;
            public IntPtr pWaitSemaphores; // const VkSemaphore*
            public IntPtr pWaitDstStageMask; // const VkPipelineStageFlags*
            public uint commandBufferCount;
            public IntPtr pCommandBuffers; // const VkCommandBuffer*
            public uint signalSemaphoreCount;
            public IntPtr pSignalSemaphores; // const VkSemaphore*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkImageSubresource
        {
            public uint aspectMask;
            public uint mipLevel;
            public uint arrayLayer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSubresourceLayout
        {
            public ulong offset;
            public ulong size;
            public ulong rowPitch;
            public ulong arrayPitch;
            public ulong depthPitch;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkEventCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkQueryPoolCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint queryType;
            public uint queryCount;
            public uint pipelineStatistics;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkShaderModuleCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public UIntPtr codeSize;
            public IntPtr pCode; // const uint32_t*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkPipelineCacheCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public UIntPtr initialDataSize;
            public IntPtr pInitialData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkPipelineLayoutCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint setLayoutCount;
            public IntPtr pSetLayouts; // const VkDescriptorSetLayout*
            public uint pushConstantRangeCount;
            public IntPtr pPushConstantRanges; // const VkPushConstantRange*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkSamplerCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint magFilter;
            public uint minFilter;
            public uint mipmapMode;
            public uint addressModeU;
            public uint addressModeV;
            public uint addressModeW;
            public float mipLodBias;
            public uint anisotropyEnable;
            public float maxAnisotropy;
            public uint compareEnable;
            public uint compareOp;
            public float minLod;
            public float maxLod;
            public uint borderColor;
            public uint unnormalizedCoordinates;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkDescriptorSetLayoutCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint bindingCount;
            public IntPtr pBindings; // const VkDescriptorSetLayoutBinding*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkDescriptorPoolCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint maxSets;
            public uint poolSizeCount;
            public IntPtr pPoolSizes; // const VkDescriptorPoolSize*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkDescriptorSetAllocateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public IntPtr descriptorPool;
            public uint descriptorSetCount;
            public IntPtr pSetLayouts; // const VkDescriptorSetLayout*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkFramebufferCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public IntPtr renderPass;
            public uint attachmentCount;
            public IntPtr pAttachments; // const VkImageView*
            public uint width;
            public uint height;
            public uint layers;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VkRenderPassCreateInfo
        {
            public VkStructureType sType;
            public IntPtr pNext;
            public uint flags;
            public uint attachmentCount;
            public IntPtr pAttachments; // const VkAttachmentDescription*
            public uint subpassCount;
            public IntPtr pSubpasses; // const VkSubpassDescription*
            public uint dependencyCount;
            public IntPtr pDependencies; // const VkSubpassDependency*
        }
    }
}
