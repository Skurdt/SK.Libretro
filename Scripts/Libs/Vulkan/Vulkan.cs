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
        public const string VK_KHR_SWAPCHAIN_EXTENSION_NAME = "VK_KHR_swapchain";
        public const uint VK_FALSE = 0;
        public const uint VK_TRUE = 1;
        public static readonly IntPtr VK_NULL_HANDLE = IntPtr.Zero;

        public const uint VK_QUEUE_FAMILY_IGNORED = ~0U;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr vkGetInstanceProcAddrDelegate(IntPtr instance, string pName);
        public static vkGetInstanceProcAddrDelegate vkGetInstanceProcAddr;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr vkGetDeviceProcAddrDelegate(IntPtr device, string pName);
        public static vkGetDeviceProcAddrDelegate vkGetDeviceProcAddr;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateInstanceDelegate(ref VkInstanceCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pInstance);
        public static vkCreateInstanceDelegate vkCreateInstance;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumerateInstanceExtensionPropertiesDelegate(string pLayerName, ref uint pPropertyCount, IntPtr pProperties);
        public static vkEnumerateInstanceExtensionPropertiesDelegate vkEnumerateInstanceExtensionProperties;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumerateInstanceLayerPropertiesDelegate(ref uint pPropertyCount, IntPtr pProperties);
        public static vkEnumerateInstanceLayerPropertiesDelegate vkEnumerateInstanceLayerProperties;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumerateInstanceVersionDelegate(ref uint pApiVersion);
        public static vkEnumerateInstanceVersionDelegate vkEnumerateInstanceVersion;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyInstanceDelegate(IntPtr instance, IntPtr pAllocator);
        public static vkDestroyInstanceDelegate vkDestroyInstance;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumeratePhysicalDevicesDelegate(IntPtr physicalDevice, ref uint pPhysicalDeviceCount, IntPtr pPhysicalDevices);
        public static vkEnumeratePhysicalDevicesDelegate vkEnumeratePhysicalDevices;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDeviceFeaturesDelegate(IntPtr physicalDevice, IntPtr pFeatures);
        public static vkGetPhysicalDeviceFeaturesDelegate vkGetPhysicalDeviceFeatures;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDeviceFormatPropertiesDelegate(IntPtr physicalDevice, VkFormat format, ref VkFormatProperties pFormatProperties);
        public static vkGetPhysicalDeviceFormatPropertiesDelegate vkGetPhysicalDeviceFormatProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPhysicalDeviceImageFormatPropertiesDelegate(IntPtr physicalDevice, int format, int type, int tiling, int usage, int flags, IntPtr pImageFormatProperties);
        public static vkGetPhysicalDeviceImageFormatPropertiesDelegate vkGetPhysicalDeviceImageFormatProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDevicePropertiesDelegate(IntPtr physicalDevice, IntPtr pProperties);
        public static vkGetPhysicalDevicePropertiesDelegate vkGetPhysicalDeviceProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDeviceQueueFamilyPropertiesDelegate(IntPtr physicalDevice, ref uint pQueueFamilyPropertyCount, IntPtr pQueueFamilyProperties);
        public static vkGetPhysicalDeviceQueueFamilyPropertiesDelegate vkGetPhysicalDeviceQueueFamilyProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDeviceMemoryPropertiesDelegate(IntPtr physicalDevice, ref VkPhysicalDeviceMemoryProperties pMemoryProperties);
        public static vkGetPhysicalDeviceMemoryPropertiesDelegate vkGetPhysicalDeviceMemoryProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateDeviceDelegate(IntPtr physicalDevice, ref VkDeviceCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pDevice);
        public static vkCreateDeviceDelegate vkCreateDevice;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyDeviceDelegate(IntPtr device, IntPtr pAllocator);
        public static vkDestroyDeviceDelegate vkDestroyDevice;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumerateDeviceExtensionPropertiesDelegate(IntPtr device, string pLayerName, ref uint pPropertyCount, IntPtr pProperties);
        public static vkEnumerateDeviceExtensionPropertiesDelegate vkEnumerateDeviceExtensionProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEnumerateDeviceLayerPropertiesDelegate(IntPtr device, ref uint pPropertyCount, IntPtr pProperties);
        public static vkEnumerateDeviceLayerPropertiesDelegate vkEnumerateDeviceLayerProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetDeviceQueueDelegate(IntPtr device, uint queueFamilyIndex, uint queueIndex, IntPtr pQueue);
        public static vkGetDeviceQueueDelegate vkGetDeviceQueue;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkQueueSubmitDelegate(IntPtr queue, uint submitCount, IntPtr pSubmits, IntPtr fence);
        public static vkQueueSubmitDelegate vkQueueSubmit;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkQueueWaitIdleDelegate(IntPtr queue);
        public static vkQueueWaitIdleDelegate vkQueueWaitIdle;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkDeviceWaitIdleDelegate(IntPtr device);
        public static vkDeviceWaitIdleDelegate vkDeviceWaitIdle;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkAllocateMemoryDelegate(IntPtr device, ref VkMemoryAllocateInfo pAllocateInfo, IntPtr pAllocator, out IntPtr pMemory);
        public static vkAllocateMemoryDelegate vkAllocateMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkFreeMemoryDelegate(IntPtr device, IntPtr memory, IntPtr pAllocator);
        public static vkFreeMemoryDelegate vkFreeMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkMapMemoryDelegate(IntPtr device, IntPtr memory, ulong offset, ulong size, uint flags, ref IntPtr ppData);
        public static vkMapMemoryDelegate vkMapMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkUnmapMemoryDelegate(IntPtr device, IntPtr memory);
        public static vkUnmapMemoryDelegate vkUnmapMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkFlushMappedMemoryRangesDelegate(IntPtr device, uint memoryRangeCount, IntPtr pMemoryRanges);
        public static vkFlushMappedMemoryRangesDelegate vkFlushMappedMemoryRanges;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkInvalidateMappedMemoryRangesDelegate(IntPtr device, uint memoryRangeCount, IntPtr pMemoryRanges);
        public static vkInvalidateMappedMemoryRangesDelegate vkInvalidateMappedMemoryRanges;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetDeviceMemoryCommitmentDelegate(IntPtr device, IntPtr memory, IntPtr pCommittedMemoryInBytes);
        public static vkGetDeviceMemoryCommitmentDelegate vkGetDeviceMemoryCommitment;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkBindBufferMemoryDelegate(IntPtr device, IntPtr buffer, IntPtr memory, ulong memoryOffset);
        public static vkBindBufferMemoryDelegate vkBindBufferMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkBindImageMemoryDelegate(IntPtr device, IntPtr image, IntPtr memory, ulong memoryOffset);
        public static vkBindImageMemoryDelegate vkBindImageMemory;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetBufferMemoryRequirementsDelegate(IntPtr device, IntPtr buffer, ref VkMemoryRequirements pMemoryRequirements);
        public static vkGetBufferMemoryRequirementsDelegate vkGetBufferMemoryRequirements;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetImageMemoryRequirementsDelegate(IntPtr device, IntPtr image, ref VkMemoryRequirements pMemoryRequirements);
        public static vkGetImageMemoryRequirementsDelegate vkGetImageMemoryRequirements;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetImageSparseMemoryRequirementsDelegate(IntPtr device, IntPtr image, ref uint pSparseMemoryRequirementCount, IntPtr pSparseMemoryRequirements);
        public static vkGetImageSparseMemoryRequirementsDelegate vkGetImageSparseMemoryRequirements;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetPhysicalDeviceSparseImageFormatPropertiesDelegate(IntPtr physicalDevice, int format, int type, int samples, int usage, int tiling, ref uint pPropertyCount, IntPtr pProperties);
        public static vkGetPhysicalDeviceSparseImageFormatPropertiesDelegate vkGetPhysicalDeviceSparseImageFormatProperties;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkQueueBindSparseDelegate(IntPtr queue, uint bindInfoCount, IntPtr pBindInfo, IntPtr fence);
        public static vkQueueBindSparseDelegate vkQueueBindSparse;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateFenceDelegate(IntPtr device, ref VkFenceCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pFence);
        public static vkCreateFenceDelegate vkCreateFence;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyFenceDelegate(IntPtr device, IntPtr fence, IntPtr pAllocator);
        public static vkDestroyFenceDelegate vkDestroyFence;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkResetFencesDelegate(IntPtr device, uint fenceCount, ref IntPtr pFences);
        public static vkResetFencesDelegate vkResetFences;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetFenceStatusDelegate(IntPtr device, IntPtr fence);
        public static vkGetFenceStatusDelegate vkGetFenceStatus;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkWaitForFencesDelegate(IntPtr device, uint fenceCount, IntPtr pFences, uint waitAll, ulong timeout);
        public static vkWaitForFencesDelegate vkWaitForFences;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateSemaphoreDelegate(IntPtr device, ref VkSemaphoreCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pSemaphore);
        public static vkCreateSemaphoreDelegate vkCreateSemaphore;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroySemaphoreDelegate(IntPtr device, IntPtr semaphore, IntPtr pAllocator);
        public static vkDestroySemaphoreDelegate vkDestroySemaphore;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateEventDelegate(IntPtr device, ref VkEventCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pEvent);
        public static vkCreateEventDelegate vkCreateEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyEventDelegate(IntPtr device, IntPtr e, IntPtr pAllocator);
        public static vkDestroyEventDelegate vkDestroyEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetEventStatusDelegate(IntPtr device, IntPtr e);
        public static vkGetEventStatusDelegate vkGetEventStatus;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkSetEventDelegate(IntPtr device, IntPtr e);
        public static vkSetEventDelegate vkSetEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkResetEventDelegate(IntPtr device, IntPtr e);
        public static vkResetEventDelegate vkResetEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateQueryPoolDelegate(IntPtr device, ref VkQueryPoolCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pQueryPool);
        public static vkCreateQueryPoolDelegate vkCreateQueryPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyQueryPoolDelegate(IntPtr device, IntPtr queryPool, IntPtr pAllocator);
        public static vkDestroyQueryPoolDelegate vkDestroyQueryPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetQueryPoolResultsDelegate(IntPtr device, IntPtr queryPool, uint firstQuery, uint queryCount, IntPtr dataSize, IntPtr pData, ulong stride, uint flags);
        public static vkGetQueryPoolResultsDelegate vkGetQueryPoolResults;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateBufferDelegate(IntPtr device, ref VkBufferCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pBuffer);
        public static vkCreateBufferDelegate vkCreateBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyBufferDelegate(IntPtr device, IntPtr buffer, IntPtr pAllocator);
        public static vkDestroyBufferDelegate vkDestroyBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateBufferViewDelegate(IntPtr device, ref VkBufferViewCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pView);
        public static vkCreateBufferViewDelegate vkCreateBufferView;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyBufferViewDelegate(IntPtr device, IntPtr bufferView, IntPtr pAllocator);
        public static vkDestroyBufferViewDelegate vkDestroyBufferView;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateImageDelegate(IntPtr device, ref VkImageCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pImage);
        public static vkCreateImageDelegate vkCreateImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyImageDelegate(IntPtr device, IntPtr image, IntPtr pAllocator);
        public static vkDestroyImageDelegate vkDestroyImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetImageSubresourceLayoutDelegate(IntPtr device, IntPtr image, ref VkImageSubresource pSubresource, ref VkSubresourceLayout pLayout);
        public static vkGetImageSubresourceLayoutDelegate vkGetImageSubresourceLayout;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateImageViewDelegate(IntPtr device, ref VkImageViewCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pView);
        public static vkCreateImageViewDelegate vkCreateImageView;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyImageViewDelegate(IntPtr device, IntPtr imageView, IntPtr pAllocator);
        public static vkDestroyImageViewDelegate vkDestroyImageView;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateShaderModuleDelegate(IntPtr device, ref VkShaderModuleCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pShaderModule);
        public static vkCreateShaderModuleDelegate vkCreateShaderModule;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyShaderModuleDelegate(IntPtr device, IntPtr shaderModule, IntPtr pAllocator);
        public static vkDestroyShaderModuleDelegate vkDestroyShaderModule;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreatePipelineCacheDelegate(IntPtr device, ref VkPipelineCacheCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pPipelineCache);
        public static vkCreatePipelineCacheDelegate vkCreatePipelineCache;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyPipelineCacheDelegate(IntPtr device, IntPtr pipelineCache, IntPtr pAllocator);
        public static vkDestroyPipelineCacheDelegate vkDestroyPipelineCache;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPipelineCacheDataDelegate(IntPtr device, IntPtr pipelineCache, IntPtr pDataSize, IntPtr pData);
        public static vkGetPipelineCacheDataDelegate vkGetPipelineCacheData;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkMergePipelineCachesDelegate(IntPtr device, IntPtr pipelineCache, uint srcCacheCount, IntPtr pSrcCaches);
        public static vkMergePipelineCachesDelegate vkMergePipelineCaches;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateGraphicsPipelinesDelegate(IntPtr device, IntPtr pipelineCache, uint createInfoCount, IntPtr pCreateInfos, IntPtr pAllocator, IntPtr pPipelines);
        public static vkCreateGraphicsPipelinesDelegate vkCreateGraphicsPipelines;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateComputePipelinesDelegate(IntPtr device, IntPtr pipelineCache, uint createInfoCount, IntPtr pCreateInfos, IntPtr pAllocator, IntPtr pPipelines);
        public static vkCreateComputePipelinesDelegate vkCreateComputePipelines;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyPipelineDelegate(IntPtr device, IntPtr pipeline, IntPtr pAllocator);
        public static vkDestroyPipelineDelegate vkDestroyPipeline;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreatePipelineLayoutDelegate(IntPtr device, ref VkPipelineLayoutCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pPipelineLayout);
        public static vkCreatePipelineLayoutDelegate vkCreatePipelineLayout;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyPipelineLayoutDelegate(IntPtr device, IntPtr pipelineLayout, IntPtr pAllocator);
        public static vkDestroyPipelineLayoutDelegate vkDestroyPipelineLayout;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateSamplerDelegate(IntPtr device, ref VkSamplerCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pSampler);
        public static vkCreateSamplerDelegate vkCreateSampler;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroySamplerDelegate(IntPtr device, IntPtr sampler, IntPtr pAllocator);
        public static vkDestroySamplerDelegate vkDestroySampler;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateDescriptorSetLayoutDelegate(IntPtr device, ref VkDescriptorSetLayoutCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pSetLayout);
        public static vkCreateDescriptorSetLayoutDelegate vkCreateDescriptorSetLayout;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyDescriptorSetLayoutDelegate(IntPtr device, IntPtr setLayout, IntPtr pAllocator);
        public static vkDestroyDescriptorSetLayoutDelegate vkDestroyDescriptorSetLayout;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateDescriptorPoolDelegate(IntPtr device, ref VkDescriptorPoolCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pDescriptorPool);
        public static vkCreateDescriptorPoolDelegate vkCreateDescriptorPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyDescriptorPoolDelegate(IntPtr device, IntPtr descriptorPool, IntPtr pAllocator);
        public static vkDestroyDescriptorPoolDelegate vkDestroyDescriptorPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkResetDescriptorPoolDelegate(IntPtr device, IntPtr descriptorPool, uint flags);
        public static vkResetDescriptorPoolDelegate vkResetDescriptorPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkAllocateDescriptorSetsDelegate(IntPtr device, ref VkDescriptorSetAllocateInfo pAllocateInfo, out IntPtr pDescriptorSets);
        public static vkAllocateDescriptorSetsDelegate vkAllocateDescriptorSets;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkFreeDescriptorSetsDelegate(IntPtr device, IntPtr descriptorPool, uint descriptorSetCount, IntPtr pDescriptorSets);
        public static vkFreeDescriptorSetsDelegate vkFreeDescriptorSets;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkUpdateDescriptorSetsDelegate(IntPtr device, uint writeCount, IntPtr pDescriptorWrites, uint copyCount, IntPtr pDescriptorCopies);
        public static vkUpdateDescriptorSetsDelegate vkUpdateDescriptorSets;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateFramebufferDelegate(IntPtr device, ref VkFramebufferCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pFramebuffer);
        public static vkCreateFramebufferDelegate vkCreateFramebuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyFramebufferDelegate(IntPtr device, IntPtr framebuffer, IntPtr pAllocator);
        public static vkDestroyFramebufferDelegate vkDestroyFramebuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateRenderPassDelegate(IntPtr device, ref VkRenderPassCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pRenderPass);
        public static vkCreateRenderPassDelegate vkCreateRenderPass;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyRenderPassDelegate(IntPtr device, IntPtr renderPass, IntPtr pAllocator);
        public static vkDestroyRenderPassDelegate vkDestroyRenderPass;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetRenderAreaGranularityDelegate(IntPtr device, IntPtr renderPass, IntPtr pGranularity);
        public static vkGetRenderAreaGranularityDelegate vkGetRenderAreaGranularity;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateCommandPoolDelegate(IntPtr device, ref VkCommandPoolCreateInfo pCreateInfo, IntPtr pAllocator, out IntPtr pCommandPool);
        public static vkCreateCommandPoolDelegate vkCreateCommandPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroyCommandPoolDelegate(IntPtr device, IntPtr commandPool, IntPtr pAllocator);
        public static vkDestroyCommandPoolDelegate vkDestroyCommandPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkResetCommandPoolDelegate(IntPtr device, IntPtr commandPool, uint flags);
        public static vkResetCommandPoolDelegate vkResetCommandPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkAllocateCommandBuffersDelegate(IntPtr device, ref VkCommandBufferAllocateInfo pAllocateInfo, IntPtr pCommandBuffers);
        public static vkAllocateCommandBuffersDelegate vkAllocateCommandBuffers;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkFreeCommandBuffersDelegate(IntPtr device, IntPtr commandPool, uint commandBufferCount, IntPtr pCommandBuffers);
        public static vkFreeCommandBuffersDelegate vkFreeCommandBuffers;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkBeginCommandBufferDelegate(IntPtr commandBuffer, ref VkCommandBufferBeginInfo pBeginInfo);
        public static vkBeginCommandBufferDelegate vkBeginCommandBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkEndCommandBufferDelegate(IntPtr commandBuffer);
        public static vkEndCommandBufferDelegate vkEndCommandBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkResetCommandBufferDelegate(IntPtr commandBuffer, uint flags);
        public static vkResetCommandBufferDelegate vkResetCommandBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBindPipelineDelegate(IntPtr commandBuffer, int pipelineBindPoint, IntPtr pipeline);
        public static vkCmdBindPipelineDelegate vkCmdBindPipeline;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetViewportDelegate(IntPtr commandBuffer, uint firstViewport, uint viewportCount, IntPtr pViewports);
        public static vkCmdSetViewportDelegate vkCmdSetViewport;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetScissorDelegate(IntPtr commandBuffer, uint firstScissor, uint scissorCount, IntPtr pScissors);
        public static vkCmdSetScissorDelegate vkCmdSetScissor;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetLineWidthDelegate(IntPtr commandBuffer, float lineWidth);
        public static vkCmdSetLineWidthDelegate vkCmdSetLineWidth;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetDepthBiasDelegate(IntPtr commandBuffer, float depthBiasConstantFactor, float depthBiasClamp, float depthBiasSlopeFactor);
        public static vkCmdSetDepthBiasDelegate vkCmdSetDepthBias;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetBlendConstantsDelegate(IntPtr commandBuffer, float blendConstants0, float blendConstants1, float blendConstants2, float blendConstants3);
        public static vkCmdSetBlendConstantsDelegate vkCmdSetBlendConstants;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetDepthBoundsDelegate(IntPtr commandBuffer, float minDepthBounds, float maxDepthBounds);
        public static vkCmdSetDepthBoundsDelegate vkCmdSetDepthBounds;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetStencilCompareMaskDelegate(IntPtr commandBuffer, int faceMask, uint compareMask);
        public static vkCmdSetStencilCompareMaskDelegate vkCmdSetStencilCompareMask;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetStencilWriteMaskDelegate(IntPtr commandBuffer, int faceMask, uint writeMask);
        public static vkCmdSetStencilWriteMaskDelegate vkCmdSetStencilWriteMask;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetStencilReferenceDelegate(IntPtr commandBuffer, int faceMask, uint reference);
        public static vkCmdSetStencilReferenceDelegate vkCmdSetStencilReference;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBindDescriptorSetsDelegate(IntPtr commandBuffer, int pipelineBindPoint, IntPtr layout, uint firstSet, uint descriptorSetCount, IntPtr pDescriptorSets, uint dynamicOffsetCount, IntPtr pDynamicOffsets);
        public static vkCmdBindDescriptorSetsDelegate vkCmdBindDescriptorSets;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBindIndexBufferDelegate(IntPtr commandBuffer, IntPtr buffer, ulong offset, int indexType);
        public static vkCmdBindIndexBufferDelegate vkCmdBindIndexBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBindVertexBuffersDelegate(IntPtr commandBuffer, uint firstBinding, uint bindingCount, IntPtr pBuffers, IntPtr pOffsets);
        public static vkCmdBindVertexBuffersDelegate vkCmdBindVertexBuffers;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDrawDelegate(IntPtr commandBuffer, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
        public static vkCmdDrawDelegate vkCmdDraw;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDrawIndexedDelegate(IntPtr commandBuffer, uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance);
        public static vkCmdDrawIndexedDelegate vkCmdDrawIndexed;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDrawIndirectDelegate(IntPtr commandBuffer, IntPtr buffer, ulong offset, uint drawCount, uint stride);
        public static vkCmdDrawIndirectDelegate vkCmdDrawIndirect;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDrawIndexedIndirectDelegate(IntPtr commandBuffer, IntPtr buffer, ulong offset, uint drawCount, uint stride);
        public static vkCmdDrawIndexedIndirectDelegate vkCmdDrawIndexedIndirect;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDispatchDelegate(IntPtr commandBuffer, uint groupCountX, uint groupCountY, uint groupCountZ);
        public static vkCmdDispatchDelegate vkCmdDispatch;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdDispatchIndirectDelegate(IntPtr commandBuffer, IntPtr buffer, ulong offset);
        public static vkCmdDispatchIndirectDelegate vkCmdDispatchIndirect;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdCopyBufferDelegate(IntPtr commandBuffer, IntPtr srcBuffer, IntPtr dstBuffer, uint regionCount, IntPtr pRegions);
        public static vkCmdCopyBufferDelegate vkCmdCopyBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdCopyImageDelegate(IntPtr commandBuffer, IntPtr srcImage, int srcImageLayout, IntPtr dstImage, int dstImageLayout, uint regionCount, IntPtr pRegions);
        public static vkCmdCopyImageDelegate vkCmdCopyImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBlitImageDelegate(IntPtr commandBuffer, IntPtr srcImage, VkImageLayout srcImageLayout, IntPtr dstImage, VkImageLayout dstImageLayout, uint regionCount, IntPtr pRegions, VkFilter filter);
        public static vkCmdBlitImageDelegate vkCmdBlitImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdCopyBufferToImageDelegate(IntPtr commandBuffer, IntPtr srcBuffer, IntPtr dstImage, VkImageLayout dstImageLayout, uint regionCount, IntPtr pRegions);
        public static vkCmdCopyBufferToImageDelegate vkCmdCopyBufferToImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdCopyImageToBufferDelegate(IntPtr commandBuffer, IntPtr srcImage, VkImageLayout srcImageLayout, IntPtr dstBuffer, uint regionCount, IntPtr pRegions);
        public static vkCmdCopyImageToBufferDelegate vkCmdCopyImageToBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdUpdateBufferDelegate(IntPtr commandBuffer, IntPtr dstBuffer, ulong dstOffset, ulong dataSize, IntPtr pData);
        public static vkCmdUpdateBufferDelegate vkCmdUpdateBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdFillBufferDelegate(IntPtr commandBuffer, IntPtr dstBuffer, ulong dstOffset, ulong size, uint data);
        public static vkCmdFillBufferDelegate vkCmdFillBuffer;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdClearColorImageDelegate(IntPtr commandBuffer, IntPtr image, int imageLayout, IntPtr pColor, uint rangeCount, IntPtr pRanges);
        public static vkCmdClearColorImageDelegate vkCmdClearColorImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdClearDepthStencilImageDelegate(IntPtr commandBuffer, IntPtr image, int imageLayout, IntPtr pDepthStencil, uint rangeCount, IntPtr pRanges);
        public static vkCmdClearDepthStencilImageDelegate vkCmdClearDepthStencilImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdClearAttachmentsDelegate(IntPtr commandBuffer, uint attachmentCount, IntPtr pAttachments, uint rectCount, IntPtr pRects);
        public static vkCmdClearAttachmentsDelegate vkCmdClearAttachments;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdResolveImageDelegate(IntPtr commandBuffer, IntPtr srcImage, int srcImageLayout, IntPtr dstImage, int dstImageLayout, uint regionCount, IntPtr pRegions);
        public static vkCmdResolveImageDelegate vkCmdResolveImage;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdSetEventDelegate(IntPtr commandBuffer, IntPtr e, int stageMask);
        public static vkCmdSetEventDelegate vkCmdSetEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdResetEventDelegate(IntPtr commandBuffer, IntPtr e, int stageMask);
        public static vkCmdResetEventDelegate vkCmdResetEvent;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdWaitEventsDelegate(IntPtr commandBuffer, uint eventCount, IntPtr pEvents, int srcStageMask, int dstStageMask, uint memoryBarrierCount, IntPtr pMemoryBarriers, uint bufferMemoryBarrierCount, IntPtr pBufferMemoryBarriers, uint imageMemoryBarrierCount, IntPtr pImageMemoryBarriers);
        public static vkCmdWaitEventsDelegate vkCmdWaitEvents;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdPipelineBarrierDelegate(IntPtr commandBuffer, uint srcStageMask, uint dstStageMask, uint dependencyFlags, uint memoryBarrierCount, IntPtr pMemoryBarriers, uint bufferMemoryBarrierCount, IntPtr pBufferMemoryBarriers, uint imageMemoryBarrierCount, IntPtr pImageMemoryBarriers);
        public static vkCmdPipelineBarrierDelegate vkCmdPipelineBarrier;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBeginQueryDelegate(IntPtr commandBuffer, IntPtr queryPool, uint query, uint flags);
        public static vkCmdBeginQueryDelegate vkCmdBeginQuery;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdEndQueryDelegate(IntPtr commandBuffer, IntPtr queryPool, uint query);
        public static vkCmdEndQueryDelegate vkCmdEndQuery;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdResetQueryPoolDelegate(IntPtr commandBuffer, IntPtr queryPool, uint firstQuery, uint queryCount);
        public static vkCmdResetQueryPoolDelegate vkCmdResetQueryPool;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdWriteTimestampDelegate(IntPtr commandBuffer, int pipelineStage, IntPtr queryPool, uint query);
        public static vkCmdWriteTimestampDelegate vkCmdWriteTimestamp;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdCopyQueryPoolResultsDelegate(IntPtr commandBuffer, IntPtr queryPool, uint firstQuery, uint queryCount, IntPtr dstBuffer, ulong dstOffset, ulong stride, uint flags);
        public static vkCmdCopyQueryPoolResultsDelegate vkCmdCopyQueryPoolResults;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdPushConstantsDelegate(IntPtr commandBuffer, IntPtr layout, int stageFlags, uint offset, uint size, IntPtr pValues);
        public static vkCmdPushConstantsDelegate vkCmdPushConstants;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdBeginRenderPassDelegate(IntPtr commandBuffer, IntPtr pRenderPassBegin, int contents);
        public static vkCmdBeginRenderPassDelegate vkCmdBeginRenderPass;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdNextSubpassDelegate(IntPtr commandBuffer, int contents);
        public static vkCmdNextSubpassDelegate vkCmdNextSubpass;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdEndRenderPassDelegate(IntPtr commandBuffer);
        public static vkCmdEndRenderPassDelegate vkCmdEndRenderPass;
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkCmdExecuteCommandsDelegate(IntPtr commandBuffer, uint commandBufferCount, IntPtr pCommandBuffers);
        public static vkCmdExecuteCommandsDelegate vkCmdExecuteCommands;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPhysicalDeviceSurfaceSupportKHRDelegate(IntPtr physicalDevice, uint queueFamilyIndex, IntPtr surface, out uint pSupported);
        public static vkGetPhysicalDeviceSurfaceSupportKHRDelegate vkGetPhysicalDeviceSurfaceSupportKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate(IntPtr physicalDevice, IntPtr surface, IntPtr pSurfaceCapabilities);
        public static vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate vkGetPhysicalDeviceSurfaceCapabilitiesKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPhysicalDeviceSurfaceFormatsKHRDelegate(IntPtr physicalDevice, IntPtr surface, ref uint pSurfaceFormatCount, IntPtr pSurfaceFormats);
        public static vkGetPhysicalDeviceSurfaceFormatsKHRDelegate vkGetPhysicalDeviceSurfaceFormatsKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkGetPhysicalDeviceSurfacePresentModesKHRDelegate(IntPtr physicalDevice, IntPtr surface, ref uint pPresentModeCount, IntPtr pPresentModes);
        public static vkGetPhysicalDeviceSurfacePresentModesKHRDelegate vkGetPhysicalDeviceSurfacePresentModesKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkCreateSwapchainKHRDelegate(IntPtr device, ref VkSwapchainCreateInfoKHR pCreateInfo, IntPtr pAllocator, out IntPtr pSwapchain);
        public static vkCreateSwapchainKHRDelegate vkCreateSwapchainKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkDestroySwapchainKHRDelegate(IntPtr device, IntPtr swapchain, IntPtr pAllocator);
        public static vkDestroySwapchainKHRDelegate vkDestroySwapchainKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void vkGetSwapchainImagesKHRDelegate(IntPtr device, IntPtr swapchain, ref uint pSwapchainImageCount, IntPtr pSwapchainImages);
        public static vkGetSwapchainImagesKHRDelegate vkGetSwapchainImagesKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkAcquireNextImageKHRDelegate(IntPtr device, IntPtr swapchain, ulong timeout, IntPtr semaphore, IntPtr fence, ref uint pImageIndex);
        public static vkAcquireNextImageKHRDelegate vkAcquireNextImageKHR;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate VkResult vkQueuePresentKHRDelegate(IntPtr queue, IntPtr pPresentInfo);
        public static vkQueuePresentKHRDelegate vkQueuePresentKHR;

        public static bool VulkanInit(IntPtr getInstanceProcAddr)
        {
            if (getInstanceProcAddr == IntPtr.Zero)
                return false;
            vkGetInstanceProcAddr = getInstanceProcAddr.GetDelegate<vkGetInstanceProcAddrDelegate>();
            return true;
        }

        public static T VulkanLoadInstanceSymbol<T>(IntPtr instance, bool throwOnError = true) where T : Delegate
        {
            var functionName = typeof(T).Name.Replace("Delegate", "");
            var procAddr = vkGetInstanceProcAddr(instance, functionName);
            return procAddr != IntPtr.Zero
                 ? procAddr.GetDelegate<T>()
                 : throwOnError
                 ? throw new Exception($"Failed to load vulkan instance symbol for {functionName}") 
                 : null;
        }

        public static T VulkanLoadDeviceSymbol<T>(IntPtr device, bool throwOnError = true) where T : Delegate
        {
            var functionName = typeof(T).Name.Replace("Delegate", "");
            var procAddr = vkGetDeviceProcAddr(device, functionName);
            return procAddr != IntPtr.Zero
                 ? procAddr.GetDelegate<T>()
                 : throwOnError
                 ? throw new Exception($"Failed to load vulkan device symbol for {functionName}") 
                 : null;
        }

        public static T VulkanLoadGlobalSymbol<T>(bool throwOnError = true) where T : Delegate
            => VulkanLoadInstanceSymbol<T>(IntPtr.Zero, throwOnError);

        public static bool VulkanLoadGlobalSymbols()
        {
            try
            {
                vkCreateInstance = VulkanLoadGlobalSymbol<vkCreateInstanceDelegate>();
                vkEnumerateInstanceExtensionProperties = VulkanLoadGlobalSymbol<vkEnumerateInstanceExtensionPropertiesDelegate>();
                vkEnumerateInstanceLayerProperties = VulkanLoadGlobalSymbol<vkEnumerateInstanceLayerPropertiesDelegate>();
                vkEnumerateInstanceVersion = VulkanLoadGlobalSymbol<vkEnumerateInstanceVersionDelegate>();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool VulkanLoadCoreSymbols(IntPtr instance)
        {
            try
            {
                vkDestroyInstance = VulkanLoadInstanceSymbol<vkDestroyInstanceDelegate>(instance);
                vkEnumeratePhysicalDevices = VulkanLoadInstanceSymbol<vkEnumeratePhysicalDevicesDelegate>(instance);
                vkGetPhysicalDeviceFeatures = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceFeaturesDelegate>(instance);
                vkGetPhysicalDeviceFormatProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceFormatPropertiesDelegate>(instance);
                vkGetPhysicalDeviceImageFormatProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceImageFormatPropertiesDelegate>(instance);
                vkGetPhysicalDeviceProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDevicePropertiesDelegate>(instance);
                vkGetPhysicalDeviceQueueFamilyProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceQueueFamilyPropertiesDelegate>(instance);
                vkGetPhysicalDeviceMemoryProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceMemoryPropertiesDelegate>(instance);
                vkGetDeviceProcAddr = VulkanLoadInstanceSymbol<vkGetDeviceProcAddrDelegate>(instance);
                vkCreateDevice = VulkanLoadInstanceSymbol<vkCreateDeviceDelegate>(instance);
                vkDestroyDevice = VulkanLoadInstanceSymbol<vkDestroyDeviceDelegate>(instance);
                vkEnumerateDeviceExtensionProperties = VulkanLoadInstanceSymbol<vkEnumerateDeviceExtensionPropertiesDelegate>(instance);
                vkEnumerateDeviceLayerProperties = VulkanLoadInstanceSymbol<vkEnumerateDeviceLayerPropertiesDelegate>(instance);
                vkGetDeviceQueue = VulkanLoadInstanceSymbol<vkGetDeviceQueueDelegate>(instance);
                vkQueueSubmit = VulkanLoadInstanceSymbol<vkQueueSubmitDelegate>(instance);
                vkQueueWaitIdle = VulkanLoadInstanceSymbol<vkQueueWaitIdleDelegate>(instance);
                vkDeviceWaitIdle = VulkanLoadInstanceSymbol<vkDeviceWaitIdleDelegate>(instance);
                vkAllocateMemory = VulkanLoadInstanceSymbol<vkAllocateMemoryDelegate>(instance);
                vkFreeMemory = VulkanLoadInstanceSymbol<vkFreeMemoryDelegate>(instance);
                vkMapMemory = VulkanLoadInstanceSymbol<vkMapMemoryDelegate>(instance);
                vkUnmapMemory = VulkanLoadInstanceSymbol<vkUnmapMemoryDelegate>(instance);
                vkFlushMappedMemoryRanges = VulkanLoadInstanceSymbol<vkFlushMappedMemoryRangesDelegate>(instance);
                vkInvalidateMappedMemoryRanges = VulkanLoadInstanceSymbol<vkInvalidateMappedMemoryRangesDelegate>(instance);
                vkGetDeviceMemoryCommitment = VulkanLoadInstanceSymbol<vkGetDeviceMemoryCommitmentDelegate>(instance);
                vkBindBufferMemory = VulkanLoadInstanceSymbol<vkBindBufferMemoryDelegate>(instance);
                vkBindImageMemory = VulkanLoadInstanceSymbol<vkBindImageMemoryDelegate>(instance);
                vkGetBufferMemoryRequirements = VulkanLoadInstanceSymbol<vkGetBufferMemoryRequirementsDelegate>(instance);
                vkGetImageMemoryRequirements = VulkanLoadInstanceSymbol<vkGetImageMemoryRequirementsDelegate>(instance);
                vkGetImageSparseMemoryRequirements = VulkanLoadInstanceSymbol<vkGetImageSparseMemoryRequirementsDelegate>(instance);
                vkGetPhysicalDeviceSparseImageFormatProperties = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSparseImageFormatPropertiesDelegate>(instance);
                vkQueueBindSparse = VulkanLoadInstanceSymbol<vkQueueBindSparseDelegate>(instance);
                vkCreateFence = VulkanLoadInstanceSymbol<vkCreateFenceDelegate>(instance);
                vkDestroyFence = VulkanLoadInstanceSymbol<vkDestroyFenceDelegate>(instance);
                vkResetFences = VulkanLoadInstanceSymbol<vkResetFencesDelegate>(instance);
                vkGetFenceStatus = VulkanLoadInstanceSymbol<vkGetFenceStatusDelegate>(instance);
                vkWaitForFences = VulkanLoadInstanceSymbol<vkWaitForFencesDelegate>(instance);
                vkCreateSemaphore = VulkanLoadInstanceSymbol<vkCreateSemaphoreDelegate>(instance);
                vkDestroySemaphore = VulkanLoadInstanceSymbol<vkDestroySemaphoreDelegate>(instance);
                vkCreateEvent = VulkanLoadInstanceSymbol<vkCreateEventDelegate>(instance);
                vkDestroyEvent = VulkanLoadInstanceSymbol<vkDestroyEventDelegate>(instance);
                vkGetEventStatus = VulkanLoadInstanceSymbol<vkGetEventStatusDelegate>(instance);
                vkSetEvent = VulkanLoadInstanceSymbol<vkSetEventDelegate>(instance);
                vkResetEvent = VulkanLoadInstanceSymbol<vkResetEventDelegate>(instance);
                vkCreateQueryPool = VulkanLoadInstanceSymbol<vkCreateQueryPoolDelegate>(instance);
                vkDestroyQueryPool = VulkanLoadInstanceSymbol<vkDestroyQueryPoolDelegate>(instance);
                vkGetQueryPoolResults = VulkanLoadInstanceSymbol<vkGetQueryPoolResultsDelegate>(instance);
                vkCreateBuffer = VulkanLoadInstanceSymbol<vkCreateBufferDelegate>(instance);
                vkDestroyBuffer = VulkanLoadInstanceSymbol<vkDestroyBufferDelegate>(instance);
                vkCreateBufferView = VulkanLoadInstanceSymbol<vkCreateBufferViewDelegate>(instance);
                vkDestroyBufferView = VulkanLoadInstanceSymbol<vkDestroyBufferViewDelegate>(instance);
                vkCreateImage = VulkanLoadInstanceSymbol<vkCreateImageDelegate>(instance);
                vkDestroyImage = VulkanLoadInstanceSymbol<vkDestroyImageDelegate>(instance);
                vkGetImageSubresourceLayout = VulkanLoadInstanceSymbol<vkGetImageSubresourceLayoutDelegate>(instance);
                vkCreateImageView = VulkanLoadInstanceSymbol<vkCreateImageViewDelegate>(instance);
                vkDestroyImageView = VulkanLoadInstanceSymbol<vkDestroyImageViewDelegate>(instance);
                vkCreateShaderModule = VulkanLoadInstanceSymbol<vkCreateShaderModuleDelegate>(instance);
                vkDestroyShaderModule = VulkanLoadInstanceSymbol<vkDestroyShaderModuleDelegate>(instance);
                vkCreatePipelineCache = VulkanLoadInstanceSymbol<vkCreatePipelineCacheDelegate>(instance);
                vkDestroyPipelineCache = VulkanLoadInstanceSymbol<vkDestroyPipelineCacheDelegate>(instance);
                vkGetPipelineCacheData = VulkanLoadInstanceSymbol<vkGetPipelineCacheDataDelegate>(instance);
                vkMergePipelineCaches = VulkanLoadInstanceSymbol<vkMergePipelineCachesDelegate>(instance);
                vkCreateGraphicsPipelines = VulkanLoadInstanceSymbol<vkCreateGraphicsPipelinesDelegate>(instance);
                vkCreateComputePipelines = VulkanLoadInstanceSymbol<vkCreateComputePipelinesDelegate>(instance);
                vkDestroyPipeline = VulkanLoadInstanceSymbol<vkDestroyPipelineDelegate>(instance);
                vkCreatePipelineLayout = VulkanLoadInstanceSymbol<vkCreatePipelineLayoutDelegate>(instance);
                vkDestroyPipelineLayout = VulkanLoadInstanceSymbol<vkDestroyPipelineLayoutDelegate>(instance);
                vkCreateSampler = VulkanLoadInstanceSymbol<vkCreateSamplerDelegate>(instance);
                vkDestroySampler = VulkanLoadInstanceSymbol<vkDestroySamplerDelegate>(instance);
                vkCreateDescriptorSetLayout = VulkanLoadInstanceSymbol<vkCreateDescriptorSetLayoutDelegate>(instance);
                vkDestroyDescriptorSetLayout = VulkanLoadInstanceSymbol<vkDestroyDescriptorSetLayoutDelegate>(instance);
                vkCreateDescriptorPool = VulkanLoadInstanceSymbol<vkCreateDescriptorPoolDelegate>(instance);
                vkDestroyDescriptorPool = VulkanLoadInstanceSymbol<vkDestroyDescriptorPoolDelegate>(instance);
                vkResetDescriptorPool = VulkanLoadInstanceSymbol<vkResetDescriptorPoolDelegate>(instance);
                vkAllocateDescriptorSets = VulkanLoadInstanceSymbol<vkAllocateDescriptorSetsDelegate>(instance);
                vkFreeDescriptorSets = VulkanLoadInstanceSymbol<vkFreeDescriptorSetsDelegate>(instance);
                vkUpdateDescriptorSets = VulkanLoadInstanceSymbol<vkUpdateDescriptorSetsDelegate>(instance);
                vkCreateFramebuffer = VulkanLoadInstanceSymbol<vkCreateFramebufferDelegate>(instance);
                vkDestroyFramebuffer = VulkanLoadInstanceSymbol<vkDestroyFramebufferDelegate>(instance);
                vkCreateRenderPass = VulkanLoadInstanceSymbol<vkCreateRenderPassDelegate>(instance);
                vkDestroyRenderPass = VulkanLoadInstanceSymbol<vkDestroyRenderPassDelegate>(instance);
                vkGetRenderAreaGranularity = VulkanLoadInstanceSymbol<vkGetRenderAreaGranularityDelegate>(instance);
                vkCreateCommandPool = VulkanLoadInstanceSymbol<vkCreateCommandPoolDelegate>(instance);
                vkDestroyCommandPool = VulkanLoadInstanceSymbol<vkDestroyCommandPoolDelegate>(instance);
                vkResetCommandPool = VulkanLoadInstanceSymbol<vkResetCommandPoolDelegate>(instance);
                vkAllocateCommandBuffers = VulkanLoadInstanceSymbol<vkAllocateCommandBuffersDelegate>(instance);
                vkFreeCommandBuffers = VulkanLoadInstanceSymbol<vkFreeCommandBuffersDelegate>(instance);
                vkBeginCommandBuffer = VulkanLoadInstanceSymbol<vkBeginCommandBufferDelegate>(instance);
                vkEndCommandBuffer = VulkanLoadInstanceSymbol<vkEndCommandBufferDelegate>(instance);
                vkResetCommandBuffer = VulkanLoadInstanceSymbol<vkResetCommandBufferDelegate>(instance);
                vkCmdBindPipeline = VulkanLoadInstanceSymbol<vkCmdBindPipelineDelegate>(instance);
                vkCmdSetViewport = VulkanLoadInstanceSymbol<vkCmdSetViewportDelegate>(instance);
                vkCmdSetScissor = VulkanLoadInstanceSymbol<vkCmdSetScissorDelegate>(instance);
                vkCmdSetLineWidth = VulkanLoadInstanceSymbol<vkCmdSetLineWidthDelegate>(instance);
                vkCmdSetDepthBias = VulkanLoadInstanceSymbol<vkCmdSetDepthBiasDelegate>(instance);
                vkCmdSetBlendConstants = VulkanLoadInstanceSymbol<vkCmdSetBlendConstantsDelegate>(instance);
                vkCmdSetDepthBounds = VulkanLoadInstanceSymbol<vkCmdSetDepthBoundsDelegate>(instance);
                vkCmdSetStencilCompareMask = VulkanLoadInstanceSymbol<vkCmdSetStencilCompareMaskDelegate>(instance);
                vkCmdSetStencilWriteMask = VulkanLoadInstanceSymbol<vkCmdSetStencilWriteMaskDelegate>(instance);
                vkCmdSetStencilReference = VulkanLoadInstanceSymbol<vkCmdSetStencilReferenceDelegate>(instance);
                vkCmdBindDescriptorSets = VulkanLoadInstanceSymbol<vkCmdBindDescriptorSetsDelegate>(instance);
                vkCmdBindIndexBuffer = VulkanLoadInstanceSymbol<vkCmdBindIndexBufferDelegate>(instance);
                vkCmdBindVertexBuffers = VulkanLoadInstanceSymbol<vkCmdBindVertexBuffersDelegate>(instance);
                vkCmdDraw = VulkanLoadInstanceSymbol<vkCmdDrawDelegate>(instance);
                vkCmdDrawIndexed = VulkanLoadInstanceSymbol<vkCmdDrawIndexedDelegate>(instance);
                vkCmdDrawIndirect = VulkanLoadInstanceSymbol<vkCmdDrawIndirectDelegate>(instance);
                vkCmdDrawIndexedIndirect = VulkanLoadInstanceSymbol<vkCmdDrawIndexedIndirectDelegate>(instance);
                vkCmdDispatch = VulkanLoadInstanceSymbol<vkCmdDispatchDelegate>(instance);
                vkCmdDispatchIndirect = VulkanLoadInstanceSymbol<vkCmdDispatchIndirectDelegate>(instance);
                vkCmdCopyBuffer = VulkanLoadInstanceSymbol<vkCmdCopyBufferDelegate>(instance);
                vkCmdCopyImage = VulkanLoadInstanceSymbol<vkCmdCopyImageDelegate>(instance);
                vkCmdBlitImage = VulkanLoadInstanceSymbol<vkCmdBlitImageDelegate>(instance);
                vkCmdCopyBufferToImage = VulkanLoadInstanceSymbol<vkCmdCopyBufferToImageDelegate>(instance);
                vkCmdCopyImageToBuffer = VulkanLoadInstanceSymbol<vkCmdCopyImageToBufferDelegate>(instance);
                vkCmdUpdateBuffer = VulkanLoadInstanceSymbol<vkCmdUpdateBufferDelegate>(instance);
                vkCmdFillBuffer = VulkanLoadInstanceSymbol<vkCmdFillBufferDelegate>(instance);
                vkCmdClearColorImage = VulkanLoadInstanceSymbol<vkCmdClearColorImageDelegate>(instance);
                vkCmdClearDepthStencilImage = VulkanLoadInstanceSymbol<vkCmdClearDepthStencilImageDelegate>(instance);
                vkCmdClearAttachments = VulkanLoadInstanceSymbol<vkCmdClearAttachmentsDelegate>(instance);
                vkCmdResolveImage = VulkanLoadInstanceSymbol<vkCmdResolveImageDelegate>(instance);
                vkCmdSetEvent = VulkanLoadInstanceSymbol<vkCmdSetEventDelegate>(instance);
                vkCmdResetEvent = VulkanLoadInstanceSymbol<vkCmdResetEventDelegate>(instance);
                vkCmdWaitEvents = VulkanLoadInstanceSymbol<vkCmdWaitEventsDelegate>(instance);
                vkCmdPipelineBarrier = VulkanLoadInstanceSymbol<vkCmdPipelineBarrierDelegate>(instance);
                vkCmdBeginQuery = VulkanLoadInstanceSymbol<vkCmdBeginQueryDelegate>(instance);
                vkCmdEndQuery = VulkanLoadInstanceSymbol<vkCmdEndQueryDelegate>(instance);
                vkCmdResetQueryPool = VulkanLoadInstanceSymbol<vkCmdResetQueryPoolDelegate>(instance);
                vkCmdWriteTimestamp = VulkanLoadInstanceSymbol<vkCmdWriteTimestampDelegate>(instance);
                vkCmdCopyQueryPoolResults = VulkanLoadInstanceSymbol<vkCmdCopyQueryPoolResultsDelegate>(instance);
                vkCmdPushConstants = VulkanLoadInstanceSymbol<vkCmdPushConstantsDelegate>(instance);
                vkCmdBeginRenderPass = VulkanLoadInstanceSymbol<vkCmdBeginRenderPassDelegate>(instance);
                vkCmdNextSubpass = VulkanLoadInstanceSymbol<vkCmdNextSubpassDelegate>(instance);
                vkCmdEndRenderPass = VulkanLoadInstanceSymbol<vkCmdEndRenderPassDelegate>(instance);
                vkCmdExecuteCommands = VulkanLoadInstanceSymbol<vkCmdExecuteCommandsDelegate>(instance);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool VulkanLoadCoreDeviceSymbols(IntPtr device)
        {
            try
            {
                vkDestroyDevice = VulkanLoadDeviceSymbol<vkDestroyDeviceDelegate>(device);
                vkGetDeviceQueue = VulkanLoadDeviceSymbol<vkGetDeviceQueueDelegate>(device);
                vkQueueSubmit = VulkanLoadDeviceSymbol<vkQueueSubmitDelegate>(device);
                vkQueueWaitIdle = VulkanLoadDeviceSymbol<vkQueueWaitIdleDelegate>(device);
                vkDeviceWaitIdle = VulkanLoadDeviceSymbol<vkDeviceWaitIdleDelegate>(device);
                vkAllocateMemory = VulkanLoadDeviceSymbol<vkAllocateMemoryDelegate>(device);
                vkFreeMemory = VulkanLoadDeviceSymbol<vkFreeMemoryDelegate>(device);
                vkMapMemory = VulkanLoadDeviceSymbol<vkMapMemoryDelegate>(device);
                vkUnmapMemory = VulkanLoadDeviceSymbol<vkUnmapMemoryDelegate>(device);
                vkFlushMappedMemoryRanges = VulkanLoadDeviceSymbol<vkFlushMappedMemoryRangesDelegate>(device);
                vkInvalidateMappedMemoryRanges = VulkanLoadDeviceSymbol<vkInvalidateMappedMemoryRangesDelegate>(device);
                vkGetDeviceMemoryCommitment = VulkanLoadDeviceSymbol<vkGetDeviceMemoryCommitmentDelegate>(device);
                vkBindBufferMemory = VulkanLoadDeviceSymbol<vkBindBufferMemoryDelegate>(device);
                vkBindImageMemory = VulkanLoadDeviceSymbol<vkBindImageMemoryDelegate>(device);
                vkGetBufferMemoryRequirements = VulkanLoadDeviceSymbol<vkGetBufferMemoryRequirementsDelegate>(device);
                vkGetImageMemoryRequirements = VulkanLoadDeviceSymbol<vkGetImageMemoryRequirementsDelegate>(device);
                vkGetImageSparseMemoryRequirements = VulkanLoadDeviceSymbol<vkGetImageSparseMemoryRequirementsDelegate>(device);
                vkQueueBindSparse = VulkanLoadDeviceSymbol<vkQueueBindSparseDelegate>(device);
                vkCreateFence = VulkanLoadDeviceSymbol<vkCreateFenceDelegate>(device);
                vkDestroyFence = VulkanLoadDeviceSymbol<vkDestroyFenceDelegate>(device);
                vkResetFences = VulkanLoadDeviceSymbol<vkResetFencesDelegate>(device);
                vkGetFenceStatus = VulkanLoadDeviceSymbol<vkGetFenceStatusDelegate>(device);
                vkWaitForFences = VulkanLoadDeviceSymbol<vkWaitForFencesDelegate>(device);
                vkCreateSemaphore = VulkanLoadDeviceSymbol<vkCreateSemaphoreDelegate>(device);
                vkDestroySemaphore = VulkanLoadDeviceSymbol<vkDestroySemaphoreDelegate>(device);
                vkCreateEvent = VulkanLoadDeviceSymbol<vkCreateEventDelegate>(device);
                vkDestroyEvent = VulkanLoadDeviceSymbol<vkDestroyEventDelegate>(device);
                vkGetEventStatus = VulkanLoadDeviceSymbol<vkGetEventStatusDelegate>(device);
                vkSetEvent = VulkanLoadDeviceSymbol<vkSetEventDelegate>(device);
                vkResetEvent = VulkanLoadDeviceSymbol<vkResetEventDelegate>(device);
                vkCreateQueryPool = VulkanLoadDeviceSymbol<vkCreateQueryPoolDelegate>(device);
                vkDestroyQueryPool = VulkanLoadDeviceSymbol<vkDestroyQueryPoolDelegate>(device);
                vkGetQueryPoolResults = VulkanLoadDeviceSymbol<vkGetQueryPoolResultsDelegate>(device);
                vkCreateBuffer = VulkanLoadDeviceSymbol<vkCreateBufferDelegate>(device);
                vkDestroyBuffer = VulkanLoadDeviceSymbol<vkDestroyBufferDelegate>(device);
                vkCreateBufferView = VulkanLoadDeviceSymbol<vkCreateBufferViewDelegate>(device);
                vkDestroyBufferView = VulkanLoadDeviceSymbol<vkDestroyBufferViewDelegate>(device);
                vkCreateImage = VulkanLoadDeviceSymbol<vkCreateImageDelegate>(device);
                vkDestroyImage = VulkanLoadDeviceSymbol<vkDestroyImageDelegate>(device);
                vkGetImageSubresourceLayout = VulkanLoadDeviceSymbol<vkGetImageSubresourceLayoutDelegate>(device);
                vkCreateImageView = VulkanLoadDeviceSymbol<vkCreateImageViewDelegate>(device);
                vkDestroyImageView = VulkanLoadDeviceSymbol<vkDestroyImageViewDelegate>(device);
                vkCreateShaderModule = VulkanLoadDeviceSymbol<vkCreateShaderModuleDelegate>(device);
                vkDestroyShaderModule = VulkanLoadDeviceSymbol<vkDestroyShaderModuleDelegate>(device);
                vkCreatePipelineCache = VulkanLoadDeviceSymbol<vkCreatePipelineCacheDelegate>(device);
                vkDestroyPipelineCache = VulkanLoadDeviceSymbol<vkDestroyPipelineCacheDelegate>(device);
                vkGetPipelineCacheData = VulkanLoadDeviceSymbol<vkGetPipelineCacheDataDelegate>(device);
                vkMergePipelineCaches = VulkanLoadDeviceSymbol<vkMergePipelineCachesDelegate>(device);
                vkCreateGraphicsPipelines = VulkanLoadDeviceSymbol<vkCreateGraphicsPipelinesDelegate>(device);
                vkCreateComputePipelines = VulkanLoadDeviceSymbol<vkCreateComputePipelinesDelegate>(device);
                vkDestroyPipeline = VulkanLoadDeviceSymbol<vkDestroyPipelineDelegate>(device);
                vkCreatePipelineLayout = VulkanLoadDeviceSymbol<vkCreatePipelineLayoutDelegate>(device);
                vkDestroyPipelineLayout = VulkanLoadDeviceSymbol<vkDestroyPipelineLayoutDelegate>(device);
                vkCreateSampler = VulkanLoadDeviceSymbol<vkCreateSamplerDelegate>(device);
                vkDestroySampler = VulkanLoadDeviceSymbol<vkDestroySamplerDelegate>(device);
                vkCreateDescriptorSetLayout = VulkanLoadDeviceSymbol<vkCreateDescriptorSetLayoutDelegate>(device);
                vkDestroyDescriptorSetLayout = VulkanLoadDeviceSymbol<vkDestroyDescriptorSetLayoutDelegate>(device);
                vkCreateDescriptorPool = VulkanLoadDeviceSymbol<vkCreateDescriptorPoolDelegate>(device);
                vkDestroyDescriptorPool = VulkanLoadDeviceSymbol<vkDestroyDescriptorPoolDelegate>(device);
                vkResetDescriptorPool = VulkanLoadDeviceSymbol<vkResetDescriptorPoolDelegate>(device);
                vkAllocateDescriptorSets = VulkanLoadDeviceSymbol<vkAllocateDescriptorSetsDelegate>(device);
                vkFreeDescriptorSets = VulkanLoadDeviceSymbol<vkFreeDescriptorSetsDelegate>(device);
                vkUpdateDescriptorSets = VulkanLoadDeviceSymbol<vkUpdateDescriptorSetsDelegate>(device);
                vkCreateFramebuffer = VulkanLoadDeviceSymbol<vkCreateFramebufferDelegate>(device);
                vkDestroyFramebuffer = VulkanLoadDeviceSymbol<vkDestroyFramebufferDelegate>(device);
                vkCreateRenderPass = VulkanLoadDeviceSymbol<vkCreateRenderPassDelegate>(device);
                vkDestroyRenderPass = VulkanLoadDeviceSymbol<vkDestroyRenderPassDelegate>(device);
                vkGetRenderAreaGranularity = VulkanLoadDeviceSymbol<vkGetRenderAreaGranularityDelegate>(device);
                vkCreateCommandPool = VulkanLoadDeviceSymbol<vkCreateCommandPoolDelegate>(device);
                vkDestroyCommandPool = VulkanLoadDeviceSymbol<vkDestroyCommandPoolDelegate>(device);
                vkResetCommandPool = VulkanLoadDeviceSymbol<vkResetCommandPoolDelegate>(device);
                vkAllocateCommandBuffers = VulkanLoadDeviceSymbol<vkAllocateCommandBuffersDelegate>(device);
                vkFreeCommandBuffers = VulkanLoadDeviceSymbol<vkFreeCommandBuffersDelegate>(device);
                vkBeginCommandBuffer = VulkanLoadDeviceSymbol<vkBeginCommandBufferDelegate>(device);
                vkEndCommandBuffer = VulkanLoadDeviceSymbol<vkEndCommandBufferDelegate>(device);
                vkResetCommandBuffer = VulkanLoadDeviceSymbol<vkResetCommandBufferDelegate>(device);
                vkCmdBindPipeline = VulkanLoadDeviceSymbol<vkCmdBindPipelineDelegate>(device);
                vkCmdSetViewport = VulkanLoadDeviceSymbol<vkCmdSetViewportDelegate>(device);
                vkCmdSetScissor = VulkanLoadDeviceSymbol<vkCmdSetScissorDelegate>(device);
                vkCmdSetLineWidth = VulkanLoadDeviceSymbol<vkCmdSetLineWidthDelegate>(device);
                vkCmdSetDepthBias = VulkanLoadDeviceSymbol<vkCmdSetDepthBiasDelegate>(device);
                vkCmdSetBlendConstants = VulkanLoadDeviceSymbol<vkCmdSetBlendConstantsDelegate>(device);
                vkCmdSetDepthBounds = VulkanLoadDeviceSymbol<vkCmdSetDepthBoundsDelegate>(device);
                vkCmdSetStencilCompareMask = VulkanLoadDeviceSymbol<vkCmdSetStencilCompareMaskDelegate>(device);
                vkCmdSetStencilWriteMask = VulkanLoadDeviceSymbol<vkCmdSetStencilWriteMaskDelegate>(device);
                vkCmdSetStencilReference = VulkanLoadDeviceSymbol<vkCmdSetStencilReferenceDelegate>(device);
                vkCmdBindDescriptorSets = VulkanLoadDeviceSymbol<vkCmdBindDescriptorSetsDelegate>(device);
                vkCmdBindIndexBuffer = VulkanLoadDeviceSymbol<vkCmdBindIndexBufferDelegate>(device);
                vkCmdBindVertexBuffers = VulkanLoadDeviceSymbol<vkCmdBindVertexBuffersDelegate>(device);
                vkCmdDraw = VulkanLoadDeviceSymbol<vkCmdDrawDelegate>(device);
                vkCmdDrawIndexed = VulkanLoadDeviceSymbol<vkCmdDrawIndexedDelegate>(device);
                vkCmdDrawIndirect = VulkanLoadDeviceSymbol<vkCmdDrawIndirectDelegate>(device);
                vkCmdDrawIndexedIndirect = VulkanLoadDeviceSymbol<vkCmdDrawIndexedIndirectDelegate>(device);
                vkCmdDispatch = VulkanLoadDeviceSymbol<vkCmdDispatchDelegate>(device);
                vkCmdDispatchIndirect = VulkanLoadDeviceSymbol<vkCmdDispatchIndirectDelegate>(device);
                vkCmdCopyBuffer = VulkanLoadDeviceSymbol<vkCmdCopyBufferDelegate>(device);
                vkCmdCopyImage = VulkanLoadDeviceSymbol<vkCmdCopyImageDelegate>(device);
                vkCmdBlitImage = VulkanLoadDeviceSymbol<vkCmdBlitImageDelegate>(device);
                vkCmdCopyBufferToImage = VulkanLoadDeviceSymbol<vkCmdCopyBufferToImageDelegate>(device);
                vkCmdCopyImageToBuffer = VulkanLoadDeviceSymbol<vkCmdCopyImageToBufferDelegate>(device);
                vkCmdUpdateBuffer = VulkanLoadDeviceSymbol<vkCmdUpdateBufferDelegate>(device);
                vkCmdFillBuffer = VulkanLoadDeviceSymbol<vkCmdFillBufferDelegate>(device);
                vkCmdClearColorImage = VulkanLoadDeviceSymbol<vkCmdClearColorImageDelegate>(device);
                vkCmdClearDepthStencilImage = VulkanLoadDeviceSymbol<vkCmdClearDepthStencilImageDelegate>(device);
                vkCmdClearAttachments = VulkanLoadDeviceSymbol<vkCmdClearAttachmentsDelegate>(device);
                vkCmdResolveImage = VulkanLoadDeviceSymbol<vkCmdResolveImageDelegate>(device);
                vkCmdSetEvent = VulkanLoadDeviceSymbol<vkCmdSetEventDelegate>(device);
                vkCmdResetEvent = VulkanLoadDeviceSymbol<vkCmdResetEventDelegate>(device);
                vkCmdWaitEvents = VulkanLoadDeviceSymbol<vkCmdWaitEventsDelegate>(device);
                vkCmdPipelineBarrier = VulkanLoadDeviceSymbol<vkCmdPipelineBarrierDelegate>(device);
                vkCmdBeginQuery = VulkanLoadDeviceSymbol<vkCmdBeginQueryDelegate>(device);
                vkCmdEndQuery = VulkanLoadDeviceSymbol<vkCmdEndQueryDelegate>(device);
                vkCmdResetQueryPool = VulkanLoadDeviceSymbol<vkCmdResetQueryPoolDelegate>(device);
                vkCmdWriteTimestamp = VulkanLoadDeviceSymbol<vkCmdWriteTimestampDelegate>(device);
                vkCmdCopyQueryPoolResults = VulkanLoadDeviceSymbol<vkCmdCopyQueryPoolResultsDelegate>(device, false);
                vkCmdPushConstants = VulkanLoadDeviceSymbol<vkCmdPushConstantsDelegate>(device);
                vkCmdBeginRenderPass = VulkanLoadDeviceSymbol<vkCmdBeginRenderPassDelegate>(device);
                vkCmdNextSubpass = VulkanLoadDeviceSymbol<vkCmdNextSubpassDelegate>(device);
                vkCmdEndRenderPass = VulkanLoadDeviceSymbol<vkCmdEndRenderPassDelegate>(device);
                vkCmdExecuteCommands = VulkanLoadDeviceSymbol<vkCmdExecuteCommandsDelegate>(device);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
