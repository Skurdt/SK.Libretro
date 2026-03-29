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

using SK.Libretro.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static SK.Libretro.SDL.Header;
using static SK.Libretro.Vulkan;

namespace SK.Libretro
{
    internal sealed class HardwareRenderProxyVulkan : HardwareRenderProxy
    {
        // Helper to create and initialize a managed VkApplicationInfo
        private VkApplicationInfo CreateManagedVkApplicationInfo()
        {
            var app_info = new VkApplicationInfo
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_APPLICATION_INFO,
                apiVersion = (0u << 29) | (1u << 22) | (1u << 12) | 0u
            };
            if (app_info.pApplicationName == IntPtr.Zero)
            {
                _vkAppNamePtr = Marshal.StringToHGlobalAnsi("LibretroUnityFE");
                app_info.pApplicationName = _vkAppNamePtr;
                _vkAppNameAllocated = true;
            }
            else
            {
                _vkAppNamePtr = app_info.pApplicationName;
                _vkAppNameAllocated = false;
            }
            if (app_info.pEngineName == IntPtr.Zero)
            {
                _vkEngineNamePtr = Marshal.StringToHGlobalAnsi("LibretroUnityFE");
                app_info.pEngineName = _vkEngineNamePtr;
                _vkEngineNameAllocated = true;
            }
            else
            {
                _vkEngineNamePtr = app_info.pEngineName;
                _vkEngineNameAllocated = false;
            }
            return app_info;
        }

        private static readonly retro_vulkan_set_image_t _set_image = SetImage;
        private static readonly retro_vulkan_get_sync_index_t _get_sync_index = GetSyncIndex;
        private static readonly retro_vulkan_get_sync_index_mask_t _get_sync_index_mask = GetSyncIndexMask;
        private static readonly retro_vulkan_set_command_buffers_t _set_command_buffers = SetCommandBuffers;
        private static readonly retro_vulkan_wait_sync_index_t _wait_sync_index = WaitSyncIndex;
        private static readonly retro_vulkan_lock_queue_t _lock_queue = LockQueue;
        private static readonly retro_vulkan_unlock_queue_t _unlock_queue = UnlockQueue;
        private static readonly retro_vulkan_set_signal_semaphore_t _set_signal_semaphore = SetSignalSemaphore;

        private const int READBACK_SLOTS = 3;

        private readonly List<string> _instance_extensions = new();
        private readonly IntPtr[] _command_buffers = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _fences = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _sync_index_fences = new IntPtr[READBACK_SLOTS];
        private readonly List<IntPtr> _wait_semaphores = new();
        private readonly List<IntPtr> _pending_cmd_buffers = new();
        private readonly IntPtr[] _readback_buffers = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _readback_images = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _readback_memories = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _readback_mapped_ptrs = new IntPtr[READBACK_SLOTS];
        private readonly IntPtr[] _readback_image_memories = new IntPtr[READBACK_SLOTS];
        private readonly VkImageLayout[] _readback_image_layouts = new VkImageLayout[READBACK_SLOTS];
        private readonly VkExtent2D[] _readback_image_extents = new VkExtent2D[READBACK_SLOTS];
        private readonly VkFormat[] _readback_image_formats = new VkFormat[READBACK_SLOTS];
        private readonly uint[] _readback_sizes = new uint[READBACK_SLOTS];
        private readonly uint[] _readback_widths = new uint[READBACK_SLOTS];
        private readonly uint[] _readback_heights = new uint[READBACK_SLOTS];
        private readonly object _queueLock = new();
        private readonly object _stateLock = new();
        private readonly object _submitLock = new();

        private SDL.WindowVulkan _window = null;
        private retro_hw_render_context_negotiation_interface_vulkan _negotiation_interface;
        private bool _has_negotiation_interface;
        private IntPtr _instance = VK_NULL_HANDLE;
        private IntPtr _surface = VK_NULL_HANDLE;
        private IntPtr _gpu = VK_NULL_HANDLE;
        private IntPtr _device = VK_NULL_HANDLE;
        private IntPtr _queue = VK_NULL_HANDLE;
        private uint _queueIndex;
        private VkFormat _swapchain_format;
        private uint _width;
        private uint _height;
        private IntPtr _swapchain;
        private IntPtr[] _swapchain_images = Array.Empty<IntPtr>();
        private IntPtr[] _swapchain_image_views = Array.Empty<IntPtr>();
        private IntPtr _acquire_semaphore = VK_NULL_HANDLE;
        private IntPtr _present_semaphore = VK_NULL_HANDLE;
        private IntPtr _command_pool = VK_NULL_HANDLE;

        // Depth/stencil resources
        private IntPtr _depthStencilImage = VK_NULL_HANDLE;
        private IntPtr _depthStencilImageMemory = VK_NULL_HANDLE;
        private IntPtr _depthStencilImageView = VK_NULL_HANDLE;
        private VkFormat _depthStencilFormat = VkFormat.VK_FORMAT_UNDEFINED;
        private retro_hw_render_interface_vulkan _interface = new();
        private bool _initialized;
        private uint _sync_index;
        private uint _sync_mask;
        private bool _has_image = false;
        private IntPtr _core_image = VK_NULL_HANDLE;
        private VkFormat _image_format = VkFormat.VK_FORMAT_UNDEFINED;
        private VkImageLayout _image_layout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
        private VkImageSubresourceRange _image_subresource_range;
        private uint _src_queue_family = VK_QUEUE_FAMILY_IGNORED;
        private IntPtr _signal_semaphore = VK_NULL_HANDLE;
        private uint _readback_slot = 0;
        private uint _readback_frame_count = 0;
        private uint _last_ready_slot = 0;
        private bool _has_last_ready_slot = false;
        // Hold unmanaged memory for VkApplicationInfo and its strings for the lifetime of the instance
        private IntPtr _vkAppInfoPtr = IntPtr.Zero;
        private IntPtr _vkAppNamePtr = IntPtr.Zero;
        private IntPtr _vkEngineNamePtr = IntPtr.Zero;
        private bool _vkAppNameAllocated = false;
        private bool _vkEngineNameAllocated = false;
        private readonly VkMemoryType[] _memoryTypes = Array.Empty<VkMemoryType>();
        private readonly VkMemoryType[] _readbackMemoryTypes = Array.Empty<VkMemoryType>();

        public HardwareRenderProxyVulkan(Wrapper wrapper, retro_hw_render_callback hwRenderCallback)
        : base(wrapper, hwRenderCallback)
        {
        }

        public override bool Init(int width, int height)
        {
            if (_initialized)
                DeInit();
                
            _width = (uint)width;
            _height = (uint)height;

            try
            {
                var uniqueTitle = $"HardwareRenderProxyVulkan_{GetHashCode()}_{DateTime.UtcNow.Ticks}";
                _window = new(uniqueTitle, width, height);
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e, "SK.Libretro.HardwareRenderProxyVulkan.CreateNativeWindow");
                DeInit();
                return false;
            }

            if (!CreateInstance())
            {
                DeInit();
                return false;
            }

            if (!CreateDevice())
            {
                DeInit();
                return false;
            }

            if (!CreateSwapchain())
            {
                DeInit();
                return false;
            }

            // Create depth/stencil image if needed
            if (Depth || Stencil)
            {
                if (!CreateDepthStencilResources((uint)width, (uint)height))
                {
                    DeInit();
                    return false;
                }
            }

            if (!CreateCommandPool())
            {
                DeInit();
                return false;
            }

            ContextReset?.Invoke();

            _initialized = true;
            return true;
        }

        public override void Resize(int width, int height)
        {
            if (!_initialized)
                return;

            if (_device.IsNotNull())
                _ = vkDeviceWaitIdle(_device);

            DestroySwapchain();
            DestroyDepthStencilResources();

            // Destroy and reset readback images and buffers (size-dependent)
            if (_device.IsNotNull())
            {
                for (var i = 0; i < READBACK_SLOTS; ++i)
                {
                    if (_readback_buffers[i].IsNotNull())
                    {
                        vkDestroyBuffer(_device, _readback_buffers[i], IntPtr.Zero);
                        _readback_buffers[i] = VK_NULL_HANDLE;
                    }
                    if (_readback_memories[i].IsNotNull())
                    {
                        vkFreeMemory(_device, _readback_memories[i], IntPtr.Zero);
                        _readback_memories[i] = VK_NULL_HANDLE;
                    }
                    _readback_mapped_ptrs[i] = IntPtr.Zero;
                    _readback_sizes[i] = 0;
                    _readback_widths[i] = 0;
                    _readback_heights[i] = 0;
                    if (_readback_images[i].IsNotNull())
                    {
                        vkDestroyImage(_device, _readback_images[i], IntPtr.Zero);
                        _readback_images[i] = VK_NULL_HANDLE;
                    }
                    if (_readback_image_memories[i].IsNotNull())
                    {
                        vkFreeMemory(_device, _readback_image_memories[i], IntPtr.Zero);
                        _readback_image_memories[i] = VK_NULL_HANDLE;
                    }
                    _readback_image_layouts[i] = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                    _readback_image_extents[i] = new VkExtent2D { width = 0, height = 0 };
                    _readback_image_formats[i] = VkFormat.VK_FORMAT_UNDEFINED;
                }
            }
            _readback_slot = 0;
            _readback_frame_count = 0;
            _last_ready_slot = 0;
            _has_last_ready_slot = false;
            Array.Clear(_sync_index_fences, 0, _sync_index_fences.Length);

            _width = (uint)width;
            _height = (uint)height;

            _window.Resize(width, height);

            if (!CreateSwapchain())
            {
                _wrapper.LogHandler.LogError("Failed to recreate Vulkan swapchain during resize.", "SK.Libretro.HardwareRenderProxyVulkan.Resize");
                return;
            }

            if (Depth || Stencil)
            {
                if (!CreateDepthStencilResources(_width, _height))
                {
                    _wrapper.LogHandler.LogError("Failed to recreate Vulkan depth/stencil resources during resize.", "SK.Libretro.HardwareRenderProxyVulkan.Resize");
                    return;
                }
            }


            _wrapper.LogHandler.LogInfo($"Vulkan swapchain and all size-dependent resources recreated for resize: {_width}x{_height}", "SK.Libretro.HardwareRenderProxyVulkan.Resize");
        }

        public override void CallContextDestroy()
        {
            // Ensure all GPU work is complete before destroying anything
            if (_device.IsNotNull())
            {
                var wait_result = vkDeviceWaitIdle(_device);
                if (wait_result == VkResult.VK_ERROR_DEVICE_LOST)
                {
                    _wrapper.LogHandler.LogWarning("Device lost during context destroy - skipping wait and proceeding with destruction.", "SK.Libretro.HardwareRenderProxyVulkan.CallContextDestroy");
                    // Device is already lost, skip all further Vulkan calls and just cleanup handles
                    _device = VK_NULL_HANDLE;
                    _queue = VK_NULL_HANDLE;
                    _swapchain = VK_NULL_HANDLE;
                    _command_pool = VK_NULL_HANDLE;
                    for (var i = 0; i < READBACK_SLOTS; ++i)
                    {
                        _fences[i] = VK_NULL_HANDLE;
                        _readback_buffers[i] = VK_NULL_HANDLE;
                        _readback_images[i] = VK_NULL_HANDLE;
                    }
                    _instance = VK_NULL_HANDLE;
                    _surface = VK_NULL_HANDLE;
                    // Destroy window and cleanup without calling Vulkan
                    try
                    {
                        _window.Dispose();
                    }
                    catch (Exception e)
                    {
                        _wrapper.LogHandler.LogException(e, "SK.Libretro.HardwareRenderProxyVulkan.DestroyNativeWindow");
                    }
                    _window = null;
                    
                    // Reset all state
                    _interface = default;
                    _negotiation_interface = default;
                    _has_negotiation_interface = false;
                    _has_image = false;
                    _image_format = VkFormat.VK_FORMAT_UNDEFINED;
                    _image_layout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                    _image_subresource_range = new VkImageSubresourceRange { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 };
                    _sync_index = 0;
                    _sync_mask = 1;
                    Array.Clear(_sync_index_fences, 0, _sync_index_fences.Length);
                    _wrapper.LogHandler.LogInfo("Vulkan proxy shutdown complete (device lost recovery).", "SK.Libretro.HardwareRenderProxyVulkan.CallContextDestroy");
                    return;
                }
            }

            ContextDestroy?.Invoke();
        }

        public override void DeInit()
        {
            // Destroy all Vulkan resources while device is still valid
            DestroyCommandPool();
            DestroySwapchain();

            // Destroy depth/stencil resources
            DestroyDepthStencilResources();

            // Destroy readback resources
            if (_device.IsNotNull())
            {
                for (var i = 0; i < READBACK_SLOTS; ++i)
                {
                    if (_readback_buffers[i].IsNotNull())
                    {
                        vkDestroyBuffer(_device, _readback_buffers[i], IntPtr.Zero);
                        _readback_buffers[i] = VK_NULL_HANDLE;
                    }
                    if (_readback_memories[i].IsNotNull())
                    {
                        vkFreeMemory(_device, _readback_memories[i], IntPtr.Zero);
                        _readback_memories[i] = VK_NULL_HANDLE;
                    }
                    _readback_mapped_ptrs[i] = IntPtr.Zero;
                    _readback_sizes[i] = 0;
                    _readback_widths[i] = 0;
                    _readback_heights[i] = 0;
                    if (_readback_images[i].IsNotNull())
                    {
                        vkDestroyImage(_device, _readback_images[i], IntPtr.Zero);
                        _readback_images[i] = VK_NULL_HANDLE;
                    }
                    if (_readback_image_memories[i].IsNotNull())
                    {
                        vkFreeMemory(_device, _readback_image_memories[i], IntPtr.Zero);
                        _readback_image_memories[i] = VK_NULL_HANDLE;
                    }
                    _readback_image_layouts[i] = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                    _readback_image_extents[i] = new VkExtent2D { width = 0, height = 0 };
                    _readback_image_formats[i] = VkFormat.VK_FORMAT_UNDEFINED;
                }
            }
            _readback_slot = 0;
            _readback_frame_count = 0;
            _last_ready_slot = 0;
            _has_last_ready_slot = false;
            Array.Clear(_sync_index_fences, 0, _sync_index_fences.Length);

            DestroyDevice();

            // Destroy surface after device
            if (_surface.IsNotNull() && _instance.IsNotNull())
            {
                SDL_Vulkan_DestroySurface(_instance, _surface, IntPtr.Zero);
                _surface = VK_NULL_HANDLE;
            }

            DestroyInstance();

            try
            {
                _window?.Dispose();
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e, "SK.Libretro.HardwareRenderProxyVulkan.DestroyNativeWindow");
            }
             _window = null;

            // Reset all state
            _interface = default;
            _negotiation_interface = default;
            _has_negotiation_interface = false;
            _has_image = false;
            _image_format = VkFormat.VK_FORMAT_UNDEFINED;
            _image_layout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            _image_subresource_range = new VkImageSubresourceRange { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 };
            _sync_index = 0;
            _sync_mask = 1;
            
            _initialized = false;
            _wrapper.LogHandler.LogInfo("Vulkan proxy shutdown complete.", "SK.Libretro.HardwareRenderProxyVulkan.DeInit");
        }

        private void AdvanceSyncIndex() => _sync_index = (_sync_index + 1) % READBACK_SLOTS;

        private uint GetBytesPerPixel(VkFormat format) => format switch
        {
            VkFormat.VK_FORMAT_B8G8R8A8_UNORM
            or VkFormat.VK_FORMAT_B8G8R8A8_SRGB
            or VkFormat.VK_FORMAT_R8G8B8A8_UNORM
            or VkFormat.VK_FORMAT_R8G8B8A8_SRGB
            or VkFormat.VK_FORMAT_A8B8G8R8_UNORM_PACK32
            or VkFormat.VK_FORMAT_A8B8G8R8_SRGB_PACK32 => 4,

            VkFormat.VK_FORMAT_B5G6R5_UNORM_PACK16
            or VkFormat.VK_FORMAT_R5G6B5_UNORM_PACK16
            or VkFormat.VK_FORMAT_A1R5G5B5_UNORM_PACK16
            or VkFormat.VK_FORMAT_A1B5G5R5_UNORM_PACK16 => 2,

            _ => 0
        };

        public override bool ReadbackFrame(uint width, uint height, ref byte[] textureData)
        {
            if (!_initialized)
                return false;

            if (_device.IsNull())
                return false;

            var use_gpu_flip = true;

            var core_image = VK_NULL_HANDLE;
            var core_layout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            var core_format = VkFormat.VK_FORMAT_UNDEFINED;
            var srcQueueFamily = VK_QUEUE_FAMILY_IGNORED;
            var wait_semaphores = new List<IntPtr>();
            var pendingCommandBuffers = new List<IntPtr>();
            var signal_semaphore = VK_NULL_HANDLE;

            lock (_stateLock)
            {
                if (!_has_image)
                    return false;

                core_image = _core_image;
                core_layout = _image_layout;
                core_format = _image_format;
                srcQueueFamily = _src_queue_family;
                wait_semaphores = _wait_semaphores;
                pendingCommandBuffers = _pending_cmd_buffers;
                signal_semaphore = _signal_semaphore;
                _wait_semaphores.Clear();
                _pending_cmd_buffers.Clear();
                _signal_semaphore = VK_NULL_HANDLE;
            }

            var bytes_per_pixel = GetBytesPerPixel(core_format);
            if (bytes_per_pixel == 0)
                return false;

            var size = width * height * bytes_per_pixel;
            if (size == 0)
                return false;

            var write_slot = _readback_slot;
            var read_slot = (write_slot + 1) % READBACK_SLOTS;
            var current_sync_index = _sync_index;

            if (!EnsureReadbackBuffer(write_slot, size))
                return false;

            if (!SupportsBlitForFormat(_gpu, core_format) || !EnsureReadbackImage(write_slot, width, height, core_format))
                use_gpu_flip = false;

            var cmd = _command_buffers[write_slot];
            var write_fence = _fences[write_slot];
            var copy_slot = write_slot;

            lock (_submitLock)
            {
                var reset_result = vkResetFences(_device, 1, ref write_fence);
                if (reset_result != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogWarning("vkResetFences failed with result: " + reset_result + ", skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                    AdvanceSyncIndex();
                    return true;
                }

                _ = vkResetCommandBuffer(cmd, 0);

                var beginInfo = new VkCommandBufferBeginInfo
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO
                };
                var begin_result = vkBeginCommandBuffer(cmd, ref beginInfo);
                if (begin_result != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogWarning("vkBeginCommandBuffer failed with result: " + begin_result + ", skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                    AdvanceSyncIndex();
                    return true;
                }

                // Track all unmanaged allocations for cleanup
                var allocsToFree = new List<IntPtr>();

                // Transition core image to TRANSFER_SRC.
                var needsQueueTransfer = srcQueueFamily != VK_QUEUE_FAMILY_IGNORED && srcQueueFamily != _queueIndex;

                var coreBarriers = new VkImageMemoryBarrier[1];
                coreBarriers[0] = new VkImageMemoryBarrier
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
                    oldLayout = core_layout,
                    newLayout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                    srcQueueFamilyIndex = needsQueueTransfer ? srcQueueFamily : VK_QUEUE_FAMILY_IGNORED,
                    dstQueueFamilyIndex = needsQueueTransfer ? _queueIndex : VK_QUEUE_FAMILY_IGNORED,
                    image = core_image,
                    srcAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_MEMORY_WRITE_BIT,
                    dstAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_READ_BIT,
                    subresourceRange = new VkImageSubresourceRange { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 }
                };
                var coreBarriersHandle = GCHandle.Alloc(coreBarriers, GCHandleType.Pinned);
                try
                {
                    vkCmdPipelineBarrier(cmd,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         0, 0, VK_NULL_HANDLE, 0, VK_NULL_HANDLE,
                                         1, coreBarriersHandle.AddrOfPinnedObject());
                }
                finally
                {
                    coreBarriersHandle.Free();
                }

                var copy_source_image = core_image;
                var copy_source_layout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;

                if (use_gpu_flip)
                {
                    var blitPtrsToFree = new List<IntPtr>();

                    var readbackImageBarrier = new VkImageMemoryBarrier
                    {
                        sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
                        oldLayout = _readback_image_layouts[write_slot],
                        newLayout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                        srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                        dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                        image = _readback_images[write_slot],
                        subresourceRange = new VkImageSubresourceRange { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 },
                        srcAccessMask = 0
                    };

                    if (_readback_image_layouts[write_slot] == VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
                        readbackImageBarrier.srcAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_READ_BIT;
                    readbackImageBarrier.dstAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_WRITE_BIT;

                    var readbackImageBarrierPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkImageMemoryBarrier)));
                    Marshal.StructureToPtr(readbackImageBarrier, readbackImageBarrierPtr, false);
                    blitPtrsToFree.Add(readbackImageBarrierPtr);

                    vkCmdPipelineBarrier(cmd,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         0, 0, VK_NULL_HANDLE, 0, VK_NULL_HANDLE,
                                         1, readbackImageBarrierPtr);

                    var blitRegionSrcOffsets = new VkOffset3D[2]
                    {
                        new() { x = 0, y = (int)height, z = 0 },
                        new() { x = (int)width, y = 0, z = 1 }
                    };
                    var blitRegionSrcOffsetsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkOffset3D)) * 2);
                    for (var i = 0; i < 2; ++i)
                        Marshal.StructureToPtr(blitRegionSrcOffsets[i], blitRegionSrcOffsetsPtr + (i * Marshal.SizeOf(typeof(VkOffset3D))), false);
                    blitPtrsToFree.Add(blitRegionSrcOffsetsPtr);

                    var blitRegionDstOffsets = new VkOffset3D[2]
                    {
                        new() { x = 0, y = 0, z = 0 },
                        new() { x = (int)width, y = (int)height, z = 1 }
                    };
                    var blitRegionDstOffsetsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkOffset3D)) * 2);

                    var blitRegion = new VkImageBlit
                    {
                        srcSubresource = new VkImageSubresourceLayers { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, mipLevel = 0, baseArrayLayer = 0, layerCount = 1 },
                        dstSubresource = new VkImageSubresourceLayers { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, mipLevel = 0, baseArrayLayer = 0, layerCount = 1 }
                    };
                    unsafe
                    {
                        blitRegion.srcOffsets[0] = blitRegionSrcOffsets[0].x;
                        blitRegion.srcOffsets[1] = blitRegionSrcOffsets[0].y;
                        blitRegion.srcOffsets[2] = blitRegionSrcOffsets[0].z;
                        blitRegion.srcOffsets[3] = blitRegionSrcOffsets[1].x;
                        blitRegion.srcOffsets[4] = blitRegionSrcOffsets[1].y;
                        blitRegion.srcOffsets[5] = blitRegionSrcOffsets[1].z;

                        blitRegion.dstOffsets[0] = blitRegionDstOffsets[0].x;
                        blitRegion.dstOffsets[1] = blitRegionDstOffsets[0].y;
                        blitRegion.dstOffsets[2] = blitRegionDstOffsets[0].z;
                        blitRegion.dstOffsets[3] = blitRegionDstOffsets[1].x;
                        blitRegion.dstOffsets[4] = blitRegionDstOffsets[1].y;
                        blitRegion.dstOffsets[5] = blitRegionDstOffsets[1].z;
                    }

                    var blitRegionPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkImageBlit)));
                    Marshal.StructureToPtr(blitRegion, blitRegionPtr, false);
                    blitPtrsToFree.Add(blitRegionPtr);

                    vkCmdBlitImage(cmd,
                                   core_image,
                                   VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                                   _readback_images[write_slot],
                                   VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                                   1,
                                   blitRegionPtr,
                                   VkFilter.VK_FILTER_NEAREST);

                    readbackImageBarrier.oldLayout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
                    readbackImageBarrier.newLayout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
                    readbackImageBarrier.srcAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_WRITE_BIT;
                    readbackImageBarrier.dstAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_READ_BIT;
                    Marshal.StructureToPtr(readbackImageBarrier, readbackImageBarrierPtr, false);

                    vkCmdPipelineBarrier(cmd,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         0, 0, IntPtr.Zero, 0, IntPtr.Zero,
                                         1, readbackImageBarrierPtr);

                    // Free all blit-related allocations
                    foreach (var ptr in blitPtrsToFree)
                        Marshal.FreeHGlobal(ptr);

                    copy_source_image = _readback_images[write_slot];
                    copy_source_layout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
                    _readback_image_layouts[write_slot] = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
                }

                var readbackRegions = new VkBufferImageCopy[1];
                readbackRegions[0] = new VkBufferImageCopy
                {
                    bufferRowLength = 0,
                    bufferImageHeight = 0,
                    imageSubresource = new VkImageSubresourceLayers { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, mipLevel = 0, baseArrayLayer = 0, layerCount = 1 },
                    imageExtent = new VkExtent3D { width = width, height = height, depth = 1 }
                };
                var readbackRegionsHandle = GCHandle.Alloc(readbackRegions, GCHandleType.Pinned);
                try
                {
                    vkCmdCopyImageToBuffer(cmd, copy_source_image, copy_source_layout, _readback_buffers[write_slot], 1, readbackRegionsHandle.AddrOfPinnedObject());
                }
                finally { readbackRegionsHandle.Free(); }

                // Transition core image back to original layout
                var coreBarriersBack = new VkImageMemoryBarrier[1];
                coreBarriersBack[0] = coreBarriers[0];
                coreBarriersBack[0].oldLayout = VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
                coreBarriersBack[0].newLayout = core_layout;
                coreBarriersBack[0].srcQueueFamilyIndex = needsQueueTransfer ? _queueIndex : VK_QUEUE_FAMILY_IGNORED;
                coreBarriersBack[0].dstQueueFamilyIndex = needsQueueTransfer ? srcQueueFamily : VK_QUEUE_FAMILY_IGNORED;
                coreBarriersBack[0].srcAccessMask = (uint)VkAccessFlagBits.VK_ACCESS_TRANSFER_READ_BIT;
                coreBarriersBack[0].dstAccessMask = (uint)(VkAccessFlagBits.VK_ACCESS_MEMORY_READ_BIT | VkAccessFlagBits.VK_ACCESS_MEMORY_WRITE_BIT);
                var coreBarriersBackHandle = GCHandle.Alloc(coreBarriersBack, GCHandleType.Pinned);
                try
                {
                    vkCmdPipelineBarrier(cmd,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT,
                                         (uint)VkPipelineStageFlagBits.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
                                         0, 0, IntPtr.Zero, 0, IntPtr.Zero,
                                         1, coreBarriersBackHandle.AddrOfPinnedObject());
                }
                finally { coreBarriersBackHandle.Free(); }

                var endResult = vkEndCommandBuffer(cmd);
                if (endResult != VkResult.VK_SUCCESS)
                {
                    // Free all allocations before returning
                    foreach (var ptr in allocsToFree)
                        Marshal.FreeHGlobal(ptr);
                    _wrapper.LogHandler.LogWarning("vkEndCommandBuffer failed with result: " + endResult + ", skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                    AdvanceSyncIndex();
                    return true;
                }

                // Submit with pending command buffers + our readback command buffer
                var submitCommandBuffers = new List<IntPtr>(pendingCommandBuffers.Count + 1);
                if (pendingCommandBuffers.Count > 0)
                    submitCommandBuffers.AddRange(pendingCommandBuffers);
                submitCommandBuffers.Add(cmd);

                List<VkPipelineStageFlagBits> waitStages = new();
                for (var i = 0; i < wait_semaphores.Count; ++i)
                    waitStages.Add(VkPipelineStageFlagBits.VK_PIPELINE_STAGE_TRANSFER_BIT);

                var submitCmdBuffersHandle = GCHandle.Alloc(submitCommandBuffers.ToArray(), GCHandleType.Pinned);
                try
                {
                    VkSubmitInfo submitInfo = new()
                    {
                        sType = VkStructureType.VK_STRUCTURE_TYPE_SUBMIT_INFO,
                        commandBufferCount = (uint)submitCommandBuffers.Count,
                        pCommandBuffers = submitCmdBuffersHandle.AddrOfPinnedObject()
                    };

                    if (wait_semaphores.Count > 0)
                    {
                        submitInfo.waitSemaphoreCount = (uint)wait_semaphores.Count;
                        var waitSemaphoresHandle = GCHandle.Alloc(wait_semaphores.ToArray(), GCHandleType.Pinned);
                        var waitStagesHandle = GCHandle.Alloc(waitStages.ToArray(), GCHandleType.Pinned);
                        try
                        {
                            submitInfo.pWaitSemaphores = waitSemaphoresHandle.AddrOfPinnedObject();
                            submitInfo.pWaitDstStageMask = waitStagesHandle.AddrOfPinnedObject();

                            if (signal_semaphore.IsNotNull())
                            {
                                submitInfo.signalSemaphoreCount = 1;
                                var signalSemaphoresHandle = GCHandle.Alloc(new[] { signal_semaphore }, GCHandleType.Pinned);
                                try
                                {
                                    submitInfo.pSignalSemaphores = signalSemaphoresHandle.AddrOfPinnedObject();

                                    var submitInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkSubmitInfo)));
                                    Marshal.StructureToPtr(submitInfo, submitInfoPtr, false);

                                    lock (_queueLock)
                                    {
                                        var submit_result = vkQueueSubmit(_queue, 1, submitInfoPtr, write_fence);
                                        if (submit_result == VkResult.VK_ERROR_DEVICE_LOST)
                                        {
                                            _wrapper.LogHandler.LogError("Device lost during vkQueueSubmit! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                            AdvanceSyncIndex();
                                            return true;
                                        }
                                        if (submit_result != VkResult.VK_SUCCESS)
                                        {
                                            _wrapper.LogHandler.LogError("vkQueueSubmit failed with result: " + submit_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                            AdvanceSyncIndex();
                                            return true;
                                        }
                                    }
                                }
                                finally
                                {
                                    signalSemaphoresHandle.Free();
                                }
                            }
                            else
                            {
                                lock (_queueLock)
                                {
                                    var submitInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkSubmitInfo)));
                                    Marshal.StructureToPtr(submitInfo, submitInfoPtr, false);

                                    var submit_result = vkQueueSubmit(_queue, 1, submitInfoPtr, write_fence);
                                    if (submit_result == VkResult.VK_ERROR_DEVICE_LOST)
                                    {
                                        _wrapper.LogHandler.LogError("Device lost during vkQueueSubmit! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                        AdvanceSyncIndex();
                                        return true;
                                    }
                                    if (submit_result != VkResult.VK_SUCCESS)
                                    {
                                        _wrapper.LogHandler.LogError("vkQueueSubmit failed with result: " + submit_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                        AdvanceSyncIndex();
                                        return true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            waitSemaphoresHandle.Free();
                            waitStagesHandle.Free();
                        }
                    }
                    else
                    {
                        if (signal_semaphore.IsNotNull())
                        {
                            submitInfo.signalSemaphoreCount = 1;
                            var signalSemaphoresHandle = GCHandle.Alloc(new[] { signal_semaphore }, GCHandleType.Pinned);
                            try
                            {
                                submitInfo.pSignalSemaphores = signalSemaphoresHandle.AddrOfPinnedObject();

                                lock (_queueLock)
                                {
                                    var submitInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkSubmitInfo)));
                                    Marshal.StructureToPtr(submitInfo, submitInfoPtr, false);

                                    var submit_result = vkQueueSubmit(_queue, 1, submitInfoPtr, write_fence);
                                    if (submit_result == VkResult.VK_ERROR_DEVICE_LOST)
                                    {
                                        _wrapper.LogHandler.LogError("Device lost during vkQueueSubmit! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                        AdvanceSyncIndex();
                                        return true;
                                    }
                                    if (submit_result != VkResult.VK_SUCCESS)
                                    {
                                        _wrapper.LogHandler.LogError("vkQueueSubmit failed with result: " + submit_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                        AdvanceSyncIndex();
                                        return true;
                                    }
                                }
                            }
                            finally
                            {
                                signalSemaphoresHandle.Free();
                            }
                        }
                        else
                        {
                            lock (_queueLock)
                            {
                                var submitInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VkSubmitInfo)));
                                Marshal.StructureToPtr(submitInfo, submitInfoPtr, false);

                                var submit_result = vkQueueSubmit(_queue, 1, submitInfoPtr, write_fence);
                                if (submit_result == VkResult.VK_ERROR_DEVICE_LOST)
                                {
                                    _wrapper.LogHandler.LogError("Device lost during vkQueueSubmit! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                    AdvanceSyncIndex();
                                    return true;
                                }
                                if (submit_result != VkResult.VK_SUCCESS)
                                {
                                    _wrapper.LogHandler.LogError("vkQueueSubmit failed with result: " + submit_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                                    AdvanceSyncIndex();
                                    return true;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    submitCmdBuffersHandle.Free();
                    // Free all allocations after command buffer submission
                    foreach (var ptr in allocsToFree)
                        Marshal.FreeHGlobal(ptr);
                }

                _readback_widths[write_slot] = width;
                _readback_heights[write_slot] = height;
                _sync_index_fences[current_sync_index % READBACK_SLOTS] = write_fence;
                if (_readback_frame_count < READBACK_SLOTS - 1)
                    ++_readback_frame_count;

                // Triple-buffered: read from slot 2 frames old.
                // Non-blocking poll: if data is not ready yet, skip this frame.
                if (_readback_frame_count >= 2 &&
                    _readback_widths[read_slot] == width &&
                    _readback_heights[read_slot] == height)
                {
                    var read_fence = _fences[read_slot];
                    var fence_result = vkGetFenceStatus(_device, read_fence);
                    if (fence_result == VkResult.VK_NOT_READY)
                    {
                        if (_has_last_ready_slot &&
                            _readback_widths[_last_ready_slot] == width &&
                            _readback_heights[_last_ready_slot] == height)
                        {
                            copy_slot = _last_ready_slot;
                        }
                        else
                        {
                        _readback_slot = (write_slot + 1) % READBACK_SLOTS;
                        AdvanceSyncIndex();
                        return false;
                        }
                    }
                    else if (fence_result == VkResult.VK_ERROR_DEVICE_LOST)
                    {
                        _wrapper.LogHandler.LogError("Device lost during vkGetFenceStatus! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                        AdvanceSyncIndex();
                        return true;
                    }
                    else if (fence_result != VkResult.VK_SUCCESS)
                    {
                        _wrapper.LogHandler.LogError("vkGetFenceStatus failed with result: " + fence_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                        AdvanceSyncIndex();
                        return true;
                    }
                    else
                    {
                        copy_slot = read_slot;
                    }
                }
                else
                {
                    var fence_result = vkGetFenceStatus(_device, write_fence);
                    if (fence_result == VkResult.VK_NOT_READY)
                    {
                        if (_has_last_ready_slot &&
                            _readback_widths[_last_ready_slot] == width &&
                            _readback_heights[_last_ready_slot] == height)
                        {
                            copy_slot = _last_ready_slot;
                        }
                        else
                        {
                        _readback_slot = (write_slot + 1) % READBACK_SLOTS;
                        AdvanceSyncIndex();
                        return false;
                        }
                    }
                    else if (fence_result == VkResult.VK_ERROR_DEVICE_LOST)
                    {
                        _wrapper.LogHandler.LogError("Device lost during vkGetFenceStatus! Skipping frame.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                        AdvanceSyncIndex();
                        return true;
                    }
                    else if (fence_result != VkResult.VK_SUCCESS)
                    {
                        _wrapper.LogHandler.LogError("vkGetFenceStatus failed with result: " + fence_result, "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                        AdvanceSyncIndex();
                        return true;
                    }
                    else
                    {
                        copy_slot = write_slot;
                    }
                }

                _last_ready_slot = copy_slot;
                _has_last_ready_slot = true;
            }

            var mapped = _readback_mapped_ptrs[copy_slot];
            var mapped_locally = false;
            if (mapped.IsNull())
            {
                if (vkMapMemory(_device, _readback_memories[copy_slot], 0, size, 0, ref mapped) != VkResult.VK_SUCCESS)
                    return false;
                _readback_mapped_ptrs[copy_slot] = mapped;
                mapped_locally = true;
            }

            if (mapped == IntPtr.Zero)
            {
                _wrapper.LogHandler.LogError("Mapped pointer is null in ReadbackFrame, cannot copy data.", "SK.Libretro.HardwareRenderProxyVulkan.ReadbackFrame");
                if (mapped_locally)
                    vkUnmapMemory(_device, _readback_memories[copy_slot]);
                return false;
            }

            Marshal.Copy(mapped, textureData, 0, (int)(width * height * bytes_per_pixel));

            if (mapped_locally)
                vkUnmapMemory(_device, _readback_memories[copy_slot]);

            _readback_slot = (write_slot + 1) % READBACK_SLOTS;
            AdvanceSyncIndex();

            return true;
        }

        public override bool GetHwRenderInterface(IntPtr iface)
        {
            if (iface.IsNull())
                return false;

            if (_device.IsNull())
                return false;

            _interface.interface_type = retro_hw_render_interface_type.RETRO_HW_RENDER_INTERFACE_VULKAN;
            _interface.interface_version = RETRO.HW_RENDER_INTERFACE_VULKAN_VERSION;
            // Store a GCHandle to this managed object as the handle
            if (_interface.handle != IntPtr.Zero)
            {
                // Free previous handle if any (defensive, not strictly needed unless reused)
                var prevHandle = GCHandle.FromIntPtr(_interface.handle);
                if (prevHandle.IsAllocated)
                    prevHandle.Free();
            }
            var gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
            _interface.handle = GCHandle.ToIntPtr(gcHandle);
            _interface.instance = _instance;
            _interface.gpu = _gpu;
            _interface.device = _device;
            _interface.get_device_proc_addr = Marshal.GetFunctionPointerForDelegate(vkGetDeviceProcAddr);
            _interface.get_instance_proc_addr = Marshal.GetFunctionPointerForDelegate(vkGetInstanceProcAddr);
            _interface.queue = _queue;
            _interface.queue_index = _queueIndex;
            _interface.set_image = Marshal.GetFunctionPointerForDelegate(_set_image);
            _interface.get_sync_index = Marshal.GetFunctionPointerForDelegate(_get_sync_index);
            _interface.get_sync_index_mask = Marshal.GetFunctionPointerForDelegate(_get_sync_index_mask);
            _interface.set_command_buffers = Marshal.GetFunctionPointerForDelegate(_set_command_buffers);
            _interface.wait_sync_index = Marshal.GetFunctionPointerForDelegate(_wait_sync_index);
            _interface.lock_queue = Marshal.GetFunctionPointerForDelegate(_lock_queue);
            _interface.unlock_queue = Marshal.GetFunctionPointerForDelegate(_unlock_queue);
            _interface.set_signal_semaphore = Marshal.GetFunctionPointerForDelegate(_set_signal_semaphore);

            _sync_index = 0u;
            _sync_mask = (1 << READBACK_SLOTS) - 1;
            Array.Clear(_sync_index_fences, 0, _sync_index_fences.Length);

            var interfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf<retro_hw_render_interface_vulkan>());
            try
            {
                Marshal.StructureToPtr(_interface, interfacePtr, false);
                Marshal.WriteIntPtr(iface, interfacePtr);
            }
            catch
            {
                Marshal.FreeHGlobal(interfacePtr);
                throw;
            }
            return true;
        }

        public override bool SetHwRenderContextNegotiationInterface(IntPtr negotiation_interface)
        {
            if (negotiation_interface.IsNull())
                return false;

            var baseInterface = Marshal.PtrToStructure<retro_hw_render_context_negotiation_interface>(negotiation_interface);
            if (baseInterface.interface_type != retro_hw_render_context_negotiation_interface_type.RETRO_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE_VULKAN)
                return false;

            _negotiation_interface = Marshal.PtrToStructure<retro_hw_render_context_negotiation_interface_vulkan>(negotiation_interface);
            _has_negotiation_interface = true;

            return true;
        }

        private static bool GetHandleContext(IntPtr handle, out HardwareRenderProxyVulkan context)
        {
            context = null;
            if (handle.IsNull())
                return false;

            var gcHandle = GCHandle.FromIntPtr(handle);
            if (gcHandle.Target is not HardwareRenderProxyVulkan ctx)
                return false;

            context = ctx;
            return true;
        }

        private static void SetImage(IntPtr handle, IntPtr image, uint num_semaphores, IntPtr semaphores, uint src_queue_family)
        {
            if (!GetHandleContext(handle, out var ctx))
                return;

            if (image.IsNull())
            {
                lock (ctx._stateLock)
                {
                    ctx._has_image = false;
                    ctx._core_image = VK_NULL_HANDLE;
                    ctx._image_format = VkFormat.VK_FORMAT_UNDEFINED;
                    ctx._image_layout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
                    ctx._image_subresource_range = new VkImageSubresourceRange { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 };
                    ctx._src_queue_family = VK_QUEUE_FAMILY_IGNORED;
                    ctx._wait_semaphores.Clear();
                    ctx._pending_cmd_buffers.Clear();
                    ctx._signal_semaphore = VK_NULL_HANDLE;
                    return;
                }
            }

            // Safety: libretro cores must provide the negotiation interface if they
            // hand out VkImage handles created on their device. If there's no
            // negotiation interface we cannot safely use the provided VkImage
            // because it may belong to another VkDevice — using it will crash the
            // driver (observed as an access violation in vendor ICD). Refuse the
            // image and log an error instead of attempting to operate on it.
            if (!ctx._has_negotiation_interface)
            {
                ctx._wrapper.LogHandler.LogError("Core provided a Vulkan image without setting a negotiation interface. This is not allowed and may cause instability. Refusing to use the provided image.", "SK.Libretro.HardwareRenderProxyVulkan.SetImage");
                return;
            }

            lock (ctx._stateLock)
            {
                var coreImage = Marshal.PtrToStructure<retro_vulkan_image>(image);
                var createInfo = coreImage.create_info;

                ctx._core_image = createInfo.image;
                ctx._image_format = createInfo.format;
                ctx._image_layout = coreImage.image_layout;
                ctx._image_subresource_range = createInfo.subresourceRange;
                if (ctx._image_subresource_range.aspectMask == 0)
                    ctx._image_subresource_range.aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT;
                ctx._src_queue_family = src_queue_family;
                ctx._has_image = true;
                ctx._wait_semaphores.Clear();
                if (semaphores != IntPtr.Zero && num_semaphores > 0)
                    for (var i = 0; i < num_semaphores; ++i)
                    {
                        var semPtr = Marshal.ReadIntPtr(semaphores, i * IntPtr.Size);
                        ctx._wait_semaphores.Add(semPtr);
                    }
            }
        }

        private static uint GetSyncIndex(IntPtr handle)
            => GetHandleContext(handle, out var ctx) ? ctx._sync_index : 0;

        private static uint GetSyncIndexMask(IntPtr handle)
            => GetHandleContext(handle, out var ctx) ? ctx._sync_mask : 1u;

        private static void SetCommandBuffers(IntPtr handle, uint nu_cmd, IntPtr cmd)
        {
            if (!GetHandleContext(handle, out var ctx))
                return;

            lock (ctx._stateLock)
            {
                ctx._pending_cmd_buffers.Clear();
                if (cmd.IsNotNull() && nu_cmd > 0)
                    for (var i = 0; i < nu_cmd; ++i)
                    {
                        var cmdBufPtr = Marshal.ReadIntPtr(cmd, i * IntPtr.Size);
                        ctx._pending_cmd_buffers.Add(cmdBufPtr);
                    }
            }
        }

        private static void WaitSyncIndex(IntPtr handle)
        {
            if (!GetHandleContext(handle, out var ctx) || ctx._device.IsNull())
                return;

            var sync_index = ctx._sync_index % READBACK_SLOTS;
            var fence = ctx._sync_index_fences[sync_index];
            if (fence.IsNull())
                return;

            var result = vkGetFenceStatus(ctx._device, fence);
            if (result == VkResult.VK_NOT_READY)
                return;

            if (result == VkResult.VK_ERROR_DEVICE_LOST)
            {
                ctx._wrapper.LogHandler.LogWarning("Device lost in vkGetFenceStatus - gracefully continuing", "SK.Libretro.HardwareRenderProxyVulkan.WaitSyncIndex");
                // Device is lost but we need to keep going - just return
                return;
            }
            if (result != VkResult.VK_SUCCESS)
                ctx._wrapper.LogHandler.LogWarning("vkGetFenceStatus failed with result: " + result, "SK.Libretro.HardwareRenderProxyVulkan.WaitSyncIndex");
        }

        private static void LockQueue(IntPtr handle)
        {
            if (GetHandleContext(handle, out var ctx))
                Monitor.Enter(ctx._queueLock);
        }

        private static void UnlockQueue(IntPtr handle)
        {
            if (GetHandleContext(handle, out var ctx))
                Monitor.Exit(ctx._queueLock);
        }

        private static void SetSignalSemaphore(IntPtr handle, IntPtr semaphore)
        {
            if (!GetHandleContext(handle, out var ctx))
                return;
            
            lock (ctx._stateLock)
            {
                ctx._signal_semaphore = semaphore;
            }
        }

        private bool CreateInstance()
        {
            if (!VulkanInit(SDL_Vulkan_GetVkGetInstanceProcAddr()))
            {
                _wrapper.LogHandler.LogError("Failed to initialize Vulkan library.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                return false;
            }

            if (!VulkanLoadGlobalSymbols())
            {
                _wrapper.LogHandler.LogError("Failed to load global Vulkan symbols.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                return false;
            }

            var instance_extensions = SDL_Vulkan_GetInstanceExtensions(out var instance_extensions_count);
            if (instance_extensions.IsNull() || instance_extensions_count == 0)
            {
                _wrapper.LogHandler.LogError("SDL.Vulkan_GetInstanceExtensions failed: " + SDL_GetError(), "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                return false;
            }

            _instance_extensions.Clear();
            for (var i = 0; i < instance_extensions_count; ++i)
            {
                var extPtr = Marshal.ReadIntPtr(instance_extensions, i * IntPtr.Size);
                var ext = Marshal.PtrToStringAnsi(extPtr);
                if (!string.IsNullOrWhiteSpace(ext))
                    _instance_extensions.Add(ext);
            }

            IntPtr appInfoPtr;
            bool appInfoFromNegotiation;
            if (_has_negotiation_interface && _negotiation_interface.get_application_info != IntPtr.Zero)
            {
                appInfoPtr = _negotiation_interface.get_application_info;
                appInfoFromNegotiation = true;
            }
            else
            {
                // Zero-initialize the struct to avoid garbage in padding bytes
                var app_info = new VkApplicationInfo
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_APPLICATION_INFO,
                    apiVersion = (0u << 29) | (1u << 22) | (1u << 12) | 0u
                };

                // Allocate unmanaged strings for pApplicationName and pEngineName if not set
                if (app_info.pApplicationName == IntPtr.Zero)
                {
                    _vkAppNamePtr = Marshal.StringToHGlobalAnsi("LibretroUnityFE");
                    app_info.pApplicationName = _vkAppNamePtr;
                    _vkAppNameAllocated = true;
                }
                else
                {
                    _vkAppNamePtr = app_info.pApplicationName;
                    _vkAppNameAllocated = false;
                }
                if (app_info.pEngineName == IntPtr.Zero)
                {
                    _vkEngineNamePtr = Marshal.StringToHGlobalAnsi("LibretroUnityFE");
                    app_info.pEngineName = _vkEngineNamePtr;
                    _vkEngineNameAllocated = true;
                }
                else
                {
                    _vkEngineNamePtr = app_info.pEngineName;
                    _vkEngineNameAllocated = false;
                }

                // Only allocate and assign _vkAppInfoPtr in fallback path
                _vkAppInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkApplicationInfo>());
                Marshal.StructureToPtr(app_info, _vkAppInfoPtr, false);
                appInfoPtr = _vkAppInfoPtr;
                appInfoFromNegotiation = false;
            }

            if (_has_negotiation_interface && _negotiation_interface.interface_version >= 2 && _negotiation_interface.create_instance != IntPtr.Zero)
            {
                InstanceWrapperContext wrapper_context = new()
                {
                    required_extensions = _instance_extensions
                };

                var wrapperContextPtr = Marshal.AllocHGlobal(Marshal.SizeOf<InstanceWrapperContext>());
                try
                {
                    Marshal.StructureToPtr(wrapper_context, wrapperContextPtr, false);
                    var createInstanceFunc = Marshal.GetDelegateForFunctionPointer<retro_vulkan_create_instance_t>(_negotiation_interface.create_instance);
                    var app_info_struct = _has_negotiation_interface && _negotiation_interface.get_application_info != IntPtr.Zero
                                        ? Marshal.PtrToStructure<VkApplicationInfo>(_negotiation_interface.get_application_info)
                                        : CreateManagedVkApplicationInfo();
                    _instance = createInstanceFunc(vkGetInstanceProcAddr, ref app_info_struct, CreateInstanceWrapper, wrapperContextPtr);
                }
                finally
                {
                    Marshal.FreeHGlobal(wrapperContextPtr);
                }
            }
            else
            {
                var app_info = CreateManagedVkApplicationInfo();
                _vkAppInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkApplicationInfo>());
                Marshal.StructureToPtr(app_info, _vkAppInfoPtr, false);
                appInfoPtr = _vkAppInfoPtr;
                appInfoFromNegotiation = false;

                // Helper to allocate array of unmanaged ANSI strings
                static IntPtr AllocStringArray(IEnumerable<string> strings, out List<IntPtr> stringHandles)
                {
                    var list = new List<IntPtr>();
                    foreach (var s in strings)
                        list.Add(Marshal.StringToHGlobalAnsi(s));
                    stringHandles = list;
                    var arrayPtr = Marshal.AllocHGlobal(IntPtr.Size * list.Count);
                    for (var i = 0; i < list.Count; ++i)
                        Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, list[i]);
                    return arrayPtr;
                }

                var validExtensions = new List<string>(_instance_extensions.Count);
                foreach (var ext in _instance_extensions)
                {
                    if (ext.All(c => c <= 127))
                        validExtensions.Add(ext);
                    else
                        _wrapper.LogHandler.LogWarning($"Skipping Vulkan extension '{ext}' due to non-ASCII characters.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                }

                List<IntPtr> extHandles = null;
                var extensionsPtr = IntPtr.Zero;
                try
                {
                    if (validExtensions.Count > 0)
                        extensionsPtr = AllocStringArray(validExtensions, out extHandles);

                    VkInstanceCreateInfo info = new()
                    {
                        sType = VkStructureType.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO,
                        pNext = IntPtr.Zero,
                        flags = 0,
                        pApplicationInfo = appInfoPtr,
                        enabledLayerCount = 0,
                        ppEnabledLayerNames = IntPtr.Zero,
                        enabledExtensionCount = (uint)validExtensions.Count,
                        ppEnabledExtensionNames = extensionsPtr
                    };

                    // Defensive logging for pointer validity
                    _wrapper.LogHandler.LogDebug($"VkInstanceCreateInfo.pApplicationInfo: 0x{info.pApplicationInfo.ToInt64():X}", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                    if (appInfoFromNegotiation)
                    {
                        // Optionally log some fields from the unmanaged struct for debugging
                        try
                        {
                            var appInfoDebug = Marshal.PtrToStructure<VkApplicationInfo>(appInfoPtr);
                            _wrapper.LogHandler.LogDebug($"Negotiation VkApplicationInfo: sType={appInfoDebug.sType}, apiVersion=0x{appInfoDebug.apiVersion:X}", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                        }
                        catch (Exception ex)
                        {
                            _wrapper.LogHandler.LogWarning($"Failed to marshal VkApplicationInfo from negotiation pointer: {ex}", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                        }
                    }

                    if (vkCreateInstance(ref info, IntPtr.Zero, out _instance) != VkResult.VK_SUCCESS)
                    {
                        _wrapper.LogHandler.LogError("vkCreateInstance failed.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                        return false;
                    }

                    if (_instance.IsNull())
                    {
                        _wrapper.LogHandler.LogError("Failed to create Vulkan instance.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                        return false;
                    }
                }
                finally
                {
                    if (extHandles != null)
                        foreach (var ptr in extHandles)
                            Marshal.FreeHGlobal(ptr);
                    if (extensionsPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(extensionsPtr);
                }
            }

            if (_instance.IsNull())
            {
                _wrapper.LogHandler.LogError("Failed to create Vulkan instance.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                return false;
            }

            if (!VulkanLoadCoreSymbols(_instance))
            {
                _wrapper.LogHandler.LogError("Failed to load Vulkan core symbols.", "SK.Libretro.HardwareRenderProxyVulkan.CreateInstance");
                return false;
            }

            return true;
        }

        private void DestroyInstance()
        {
            if (_instance.IsNull())
                return;

            vkDestroyInstance(_instance, IntPtr.Zero);
            _instance = VK_NULL_HANDLE;

            // Free unmanaged memory for fallback VkApplicationInfo and its strings
            if (_vkAppInfoPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_vkAppInfoPtr);
                _vkAppInfoPtr = IntPtr.Zero;
            }
            if (_vkAppNameAllocated && _vkAppNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_vkAppNamePtr);
                _vkAppNamePtr = IntPtr.Zero;
            }
            if (_vkEngineNameAllocated && _vkEngineNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_vkEngineNamePtr);
                _vkEngineNamePtr = IntPtr.Zero;
            }
        }

        private bool CreateNegotiationInterfaceDevice2()
        {
            if (!_has_negotiation_interface || _negotiation_interface.interface_version < 2 || _negotiation_interface.create_device2 == IntPtr.Zero)
                return false;

            var wrapperContextPtr = Marshal.AllocHGlobal(Marshal.SizeOf<DeviceWrapperContext>());
            try
            {
                DeviceWrapperContext wrapper_context = new();
                Marshal.StructureToPtr(wrapper_context, wrapperContextPtr, false);
                retro_vulkan_context context = new();
                var createDevice2Func = Marshal.GetDelegateForFunctionPointer<retro_vulkan_create_device2_t>(_negotiation_interface.create_device2);
                if (createDevice2Func(ref context, _instance, IntPtr.Zero, _surface, vkGetInstanceProcAddr, CreateDeviceWrapper, wrapperContextPtr))
                {
                    if (context.device.IsNull() || context.queue.IsNull())
                        return false;

                    _gpu = context.gpu;
                    _device = context.device;
                    _queue = context.queue;
                    _queueIndex = context.queue_family_index;

                    if (!VulkanLoadCoreDeviceSymbols(_device))
                    {
                        _wrapper.LogHandler.LogError("Failed to load core device symbols from negotiation interface device.", "SK.Libretro.HardwareRenderProxyVulkan.CreateNegotiationInterfaceDevice");
                        return false;
                    }

                    return true;
                }
                _wrapper.LogHandler.LogError("Negotiation interface create_device2 failed.", "SK.Libretro.HardwareRenderProxyVulkan.CreateNegotiationInterfaceDevice");
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(wrapperContextPtr);
            }
        }

        private bool CreateNegotiationInterfaceDevice()
        {
            if (!_has_negotiation_interface || _negotiation_interface.create_device == IntPtr.Zero)
                return false;

            retro_vulkan_context context = new();

            var gpu_count = 0u;
            if (vkEnumeratePhysicalDevices(_instance, ref gpu_count, IntPtr.Zero) != VkResult.VK_SUCCESS || gpu_count == 0)
                return false;

            var gpus = new IntPtr[gpu_count];
            var handle = GCHandle.Alloc(gpus, GCHandleType.Pinned);
            try
            {
                if (vkEnumeratePhysicalDevices(_instance, ref gpu_count, handle.AddrOfPinnedObject()) != VkResult.VK_SUCCESS)
                    return false;

                _gpu = gpus[0];
            }
            finally
            {
                handle.Free();
            }

            var required_extensions = new[]
            {
                VK_KHR_SWAPCHAIN_EXTENSION_NAME
            };

            // Marshal required_extensions to unmanaged memory
            var extensionsPtr = IntPtr.Zero;
            IntPtr[] extensionHandles = null;
            try
            {
                if (required_extensions.Length > 0)
                {
                    extensionHandles = new IntPtr[required_extensions.Length];
                    for (var i = 0; i < required_extensions.Length; ++i)
                        extensionHandles[i] = Marshal.StringToHGlobalAnsi(required_extensions[i]);
                    extensionsPtr = Marshal.AllocHGlobal(IntPtr.Size * required_extensions.Length);
                    for (var i = 0; i < required_extensions.Length; ++i)
                        Marshal.WriteIntPtr(extensionsPtr, i * IntPtr.Size, extensionHandles[i]);
                }

                VkPhysicalDeviceFeatures required_features = new();
                var createDeviceFunc = Marshal.GetDelegateForFunctionPointer<retro_vulkan_create_device_t>(_negotiation_interface.create_device);
                if (!createDeviceFunc(ref context, _instance, _gpu, _surface, vkGetInstanceProcAddr, extensionsPtr, (uint)required_extensions.Length, IntPtr.Zero, 0, out required_features))
                    return false;

                if (context.device.IsNull() || context.queue.IsNull())
                    return false;

                _gpu = context.gpu;
                _device = context.device;
                _queue = context.queue;
                _queueIndex = context.queue_family_index;

                if (!VulkanLoadCoreDeviceSymbols(_device))
                {
                    _wrapper.LogHandler.LogError("Failed to load core device symbols from negotiation interface device.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                    return false;
                }

                try
                {
                    vkGetPhysicalDeviceSurfaceSupportKHR = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSurfaceSupportKHRDelegate>(_instance);
                }
                catch (Exception ex)
                {
                    _wrapper.LogHandler.LogError("Failed to load vkGetPhysicalDeviceSurfaceSupportKHR function pointer: " + ex.Message, "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                    return false;
                }

                if (vkGetPhysicalDeviceSurfaceSupportKHR(_gpu, _queueIndex, _surface, out var supports_present) != VkResult.VK_SUCCESS || supports_present == 0)
                {
                    _wrapper.LogHandler.LogError("Core's queue family " + _queueIndex + " does not support presentation to SDL surface. This will cause vkCreateSwapchainKHR to fail. Core must create device with surface-compatible queue.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                    return false;
                }

                return true;
            }
            finally
            {
                if (extensionHandles is not null)
                    foreach (var ptr in extensionHandles)
                        Marshal.FreeHGlobal(ptr);

                if (extensionsPtr.IsNotNull())
                    Marshal.FreeHGlobal(extensionsPtr);
            }
        }

        private bool CreateFallbackDevice()
        {
            var gpu_count = 0u;
            if (vkEnumeratePhysicalDevices(_instance, ref gpu_count, IntPtr.Zero) != VkResult.VK_SUCCESS || gpu_count == 0)
                return false;

            var gpus = new IntPtr[gpu_count];
            var handle = GCHandle.Alloc(gpus, GCHandleType.Pinned);
            try
            {
                if (vkEnumeratePhysicalDevices(_instance, ref gpu_count, handle.AddrOfPinnedObject()) != VkResult.VK_SUCCESS)
                    return false;

                _gpu = gpus[0];
            }
            finally
            {
                handle.Free();
            }

            try
            {
                vkGetPhysicalDeviceSurfaceSupportKHR = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSurfaceSupportKHRDelegate>(_instance);
            }
            catch (Exception ex)
            {
                _wrapper.LogHandler.LogError("Failed to load vkGetPhysicalDeviceSurfaceSupportKHR function pointer: " + ex.Message, "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                return false;
            }

            var queue_family_count = 0u;
            vkGetPhysicalDeviceQueueFamilyProperties(_gpu, ref queue_family_count, IntPtr.Zero);
            var queue_families = new VkQueueFamilyProperties[queue_family_count];
            var handleQueueFamilies = GCHandle.Alloc(queue_families, GCHandleType.Pinned);
            try
            {
                vkGetPhysicalDeviceQueueFamilyProperties(_gpu, ref queue_family_count, handleQueueFamilies.AddrOfPinnedObject());
            }
            finally
            {
                handleQueueFamilies.Free();
            }

            var selected = -1;
            for (var i = 0; i < queue_family_count; ++i)
            {
                if ((queue_families[i].queueFlags & (uint)VkQueueFlagBits.VK_QUEUE_GRAPHICS_BIT) != 0)
                {
                    if (vkGetPhysicalDeviceSurfaceSupportKHR(_gpu, (uint)i, _surface, out var supports_present) == VkResult.VK_SUCCESS && supports_present != 0)
                    {
                        selected = i;
                        break;
                    }
                }
            }

            if (selected < 0)
            {
                for (var i = 0; i < queue_family_count; ++i)
                {
                    if (vkGetPhysicalDeviceSurfaceSupportKHR(_gpu, (uint)i, _surface, out var supports_present) == VkResult.VK_SUCCESS && supports_present != 0)
                    {
                        selected = i;
                        break;
                    }
                }
            }

            if (selected < 0)
                selected = 0;

            float[] qprioArr = { 1.0f };
            var qprioHandle = GCHandle.Alloc(qprioArr, GCHandleType.Pinned);
            try
            {
                VkDeviceQueueCreateInfo qinfo = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO,
                    queueFamilyIndex = (uint)selected,
                    queueCount = 1,
                    pQueuePriorities = qprioHandle.AddrOfPinnedObject()
                };

                var device_extensions = new[]
                {
                    VK_KHR_SWAPCHAIN_EXTENSION_NAME
                };

                var extensionHandles = Array.Empty<IntPtr>();
                var extensionsPtr = IntPtr.Zero;
                var qinfoPtr = IntPtr.Zero;
                try
                {
                    if (device_extensions.Length > 0)
                    {
                        extensionHandles = new IntPtr[device_extensions.Length];
                        for (var i = 0; i < device_extensions.Length; ++i)
                            extensionHandles[i] = Marshal.StringToHGlobalAnsi(device_extensions[i]);
                        extensionsPtr = Marshal.AllocHGlobal(IntPtr.Size * device_extensions.Length);
                        for (var i = 0; i < device_extensions.Length; ++i)
                            Marshal.WriteIntPtr(extensionsPtr, i * IntPtr.Size, extensionHandles[i]);
                    }

                    qinfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkDeviceQueueCreateInfo>());
                    Marshal.StructureToPtr(qinfo, qinfoPtr, false);

                    VkDeviceCreateInfo dinfo = new()
                    {
                        sType = VkStructureType.VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO,
                        queueCreateInfoCount = 1,
                        pQueueCreateInfos = qinfoPtr,
                        enabledExtensionCount = (uint)device_extensions.Length,
                        ppEnabledExtensionNames = extensionsPtr
                    };

                    if (vkCreateDevice(_gpu, ref dinfo, IntPtr.Zero, out _device) != VkResult.VK_SUCCESS)
                        return false;
                }
                finally
                {
                    if (extensionHandles is not null)
                    {
                        foreach (var ptr in extensionHandles)
                            Marshal.FreeHGlobal(ptr);
                    }
                    if (extensionsPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(extensionsPtr);
                    if (qinfoPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(qinfoPtr);
                }
            }
            finally
            {
                qprioHandle.Free();
            }

            if (!VulkanLoadCoreDeviceSymbols(_device))
            {
                _wrapper.LogHandler.LogError("Failed to load Vulkan core device symbols.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                return false;
            }

            var queuePtr = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                vkGetDeviceQueue(_device, (uint)selected, 0, queuePtr);
                _queue = Marshal.ReadIntPtr(queuePtr);
            }
            finally
            {
                Marshal.FreeHGlobal(queuePtr);
            }
            _queueIndex = (uint)selected;

            return true;
        }

        private bool CreateDevice()
        {
            if (_surface.IsNull())
            {
                if (!SDL_Vulkan_CreateSurface(_window, _instance, IntPtr.Zero, out _surface))
                {
                    _wrapper.LogHandler.LogError("Failed to create Vulkan surface: " + SDL_GetError(), "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                    return false;
                }
            }

            if (CreateNegotiationInterfaceDevice2())
            {
                _wrapper.LogHandler.LogInfo("Created Vulkan device using negotiation interface version 2.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                return true;
            }

            if (CreateNegotiationInterfaceDevice())
            {
                _wrapper.LogHandler.LogInfo("Created Vulkan device using negotiation interface version 1.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                return true;
            }

            if (CreateFallbackDevice())
            {
                _wrapper.LogHandler.LogInfo("Created Vulkan device using fallback method.", "SK.Libretro.HardwareRenderProxyVulkan.CreateDevice");
                return true;
            }

            return false;
        }

        private void DestroyDevice()
        {
            if (_device.IsNull())
                return;

            vkDestroyDevice(_device, IntPtr.Zero);
            _device = VK_NULL_HANDLE;
            _queue = VK_NULL_HANDLE;
            _gpu = VK_NULL_HANDLE;
        }

        private bool CreateSwapchain()
        {
            if (_device.IsNull() || _surface.IsNull())
            {
                _wrapper.LogHandler.LogError("Device or surface not created.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                return false;
            }

            // Load instance-level surface query functions
            try
            {
                vkGetPhysicalDeviceSurfaceCapabilitiesKHR = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate>(_instance);
                vkGetPhysicalDeviceSurfaceFormatsKHR = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSurfaceFormatsKHRDelegate>(_instance);
                vkGetPhysicalDeviceSurfacePresentModesKHR = VulkanLoadInstanceSymbol<vkGetPhysicalDeviceSurfacePresentModesKHRDelegate>(_instance);
            }
            catch (Exception ex)
            {
                _wrapper.LogHandler.LogError("Failed to load Vulkan surface query function pointers: " + ex.Message, "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                return false;
            }

            try
            {
                vkCreateSwapchainKHR = VulkanLoadDeviceSymbol<vkCreateSwapchainKHRDelegate>(_device);
                vkDestroySwapchainKHR = VulkanLoadDeviceSymbol<vkDestroySwapchainKHRDelegate>(_device);
                vkGetSwapchainImagesKHR = VulkanLoadDeviceSymbol<vkGetSwapchainImagesKHRDelegate>(_device);
                vkAcquireNextImageKHR = VulkanLoadDeviceSymbol<vkAcquireNextImageKHRDelegate>(_device);
                vkQueuePresentKHR = VulkanLoadDeviceSymbol<vkQueuePresentKHRDelegate>(_device);
            }
            catch (Exception ex)
            {
                _wrapper.LogHandler.LogError("Failed to load Vulkan device-level swapchain function pointers: " + ex.Message, "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                return false;
            }

            // Query surface capabilities
            var surface_caps_ptr = Marshal.AllocHGlobal(Marshal.SizeOf<VkSurfaceCapabilitiesKHR>());
            VkSurfaceCapabilitiesKHR surface_caps;
            try
            {
                if (vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_gpu, _surface, surface_caps_ptr) != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogError("Failed to get surface capabilities.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                    return false;
                }

                surface_caps = Marshal.PtrToStructure<VkSurfaceCapabilitiesKHR>(surface_caps_ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(surface_caps_ptr);
            }

            // _wrapper.LogHandler.LogInfo("Surface capabilities: " +
            //                             " min=" + surface_caps.minImageCount + 
            //                             " max=" + surface_caps.maxImageCount +
            //                             " currentExtent=(" + surface_caps.currentExtent.width + "x" + surface_caps.currentExtent.height + ")" +
            //                             " minExtent=(" + surface_caps.minImageExtent.width + "x" + surface_caps.minImageExtent.height + ")" +
            //                             " maxExtent=(" + surface_caps.maxImageExtent.width + "x" + surface_caps.maxImageExtent.height + ")");

            // Query surface formats
            var format_count = 0u;
            if (vkGetPhysicalDeviceSurfaceFormatsKHR(_gpu, _surface, ref format_count, IntPtr.Zero) != VkResult.VK_SUCCESS || format_count == 0)
            {
                _wrapper.LogHandler.LogError("No surface formats available.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                return false;
            }

            var formats = new VkSurfaceFormatKHR[format_count];
            var formatsHandle = GCHandle.Alloc(formats, GCHandleType.Pinned);
            try
            {
                if (vkGetPhysicalDeviceSurfaceFormatsKHR(_gpu, _surface, ref format_count, formatsHandle.AddrOfPinnedObject()) != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogError("Failed to get surface formats.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                    return false;
                }
            }
            finally
            {
                formatsHandle.Free();
            }

            // Select format (prefer BGRA8 SRGB)
            var chosen_format = formats[0];
            foreach (var fmt in formats)
            {
                if (fmt.format == VkFormat.VK_FORMAT_B8G8R8A8_SRGB && fmt.colorSpace == VkColorSpaceKHR.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
                {
                    chosen_format = fmt;
                    break;
                }
                else if (fmt.format == VkFormat.VK_FORMAT_B8G8R8A8_UNORM)
                    chosen_format = fmt;
            }

            _swapchain_format = chosen_format.format;
            _wrapper.LogHandler.LogInfo("Selected format: " + _swapchain_format + " colorSpace: " + chosen_format.colorSpace, "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            // Determine swap extent
            VkExtent2D extent = new();
            var extent_struct = surface_caps.currentExtent;
            if (extent_struct.width != uint.MaxValue)
            {
                extent = extent_struct;
            }
            else
            {
                extent.width = _width;
                extent.height = _height;
                var minImageExtent = surface_caps.minImageExtent;
                var maxImageExtent = surface_caps.maxImageExtent;
                extent.width = Math.Max(minImageExtent.width, Math.Min(maxImageExtent.width, extent.width));
                extent.height = Math.Max(minImageExtent.height, Math.Min(maxImageExtent.height, extent.height));
            }

            _wrapper.LogHandler.LogInfo("Selected extent: " + extent.width + "x" + extent.height, "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            // Determine image count - must match sync_mask (0x3 = 2 images for double-buffering)
            var image_count = 2u;
            // Ensure we don't violate surface capabilities
            if (image_count < surface_caps.minImageCount)
                image_count = surface_caps.minImageCount;
            if (surface_caps.maxImageCount > 0 && image_count > surface_caps.maxImageCount)
                image_count = surface_caps.maxImageCount;

            _wrapper.LogHandler.LogInfo("Using " + image_count + " swapchain images", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            // Create swapchain
            VkSwapchainCreateInfoKHR create_info = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR,
                surface = _surface,
                minImageCount = image_count,
                imageFormat = chosen_format.format,
                imageColorSpace = chosen_format.colorSpace,
                imageExtent = extent,
                imageArrayLayers = 1,
                imageUsage = (uint)(VkImageUsageFlagBits.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VkImageUsageFlagBits.VK_IMAGE_USAGE_TRANSFER_SRC_BIT),
                imageSharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE,
                preTransform = surface_caps.currentTransform,
                compositeAlpha = (uint)VkCompositeAlphaFlagBitsKHR.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR,
                presentMode = VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR,  // Always supported
                clipped = VK_TRUE,
                oldSwapchain = VK_NULL_HANDLE
            };

            _wrapper.LogHandler.LogInfo("vkCreateSwapchainKHR parameters:" +
                                        " surface=" + _surface +
                                        " device=" + _device +
                                        " minImageCount=" + image_count +
                                        " format=" + chosen_format.format +
                                        " extent=" + extent.width + "x" + extent.height +
                                        " imageUsage=" + create_info.imageUsage +
                                        " presentMode=" + create_info.presentMode, "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            _wrapper.LogHandler.LogInfo("Calling vkCreateSwapchainKHR...", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            var swapchain_result = vkCreateSwapchainKHR(_device, ref create_info, IntPtr.Zero, out _swapchain);
            if (swapchain_result != VkResult.VK_SUCCESS)
            {
                _wrapper.LogHandler.LogError("vkCreateSwapchainKHR failed with result: " + swapchain_result +
                                                " (surface=" + _surface + 
                                                ", device=" + _device + ")", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                return false;
            }

            // Get swapchain images
            var swapchain_image_count = 0u;
            vkGetSwapchainImagesKHR(_device, _swapchain, ref swapchain_image_count, IntPtr.Zero);
            _swapchain_images = new IntPtr[swapchain_image_count];
            var swapchainImagesHandle = GCHandle.Alloc(_swapchain_images, GCHandleType.Pinned);
            try
            {
                vkGetSwapchainImagesKHR(_device, _swapchain, ref swapchain_image_count, swapchainImagesHandle.AddrOfPinnedObject());
            }
            finally
            {
                swapchainImagesHandle.Free();
            }

            _wrapper.LogHandler.LogInfo("Swapchain created with " + swapchain_image_count + " images (requested " + image_count + ").", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");

            // Create image views
            _swapchain_image_views = new IntPtr[swapchain_image_count];
            for (var i = 0; i < swapchain_image_count; ++i)
            {
                var viewInfoComponents = new VkComponentMapping
                {
                        r = VkComponentSwizzle.VK_COMPONENT_SWIZZLE_IDENTITY,
                        g = VkComponentSwizzle.VK_COMPONENT_SWIZZLE_IDENTITY,
                        b = VkComponentSwizzle.VK_COMPONENT_SWIZZLE_IDENTITY,
                        a = VkComponentSwizzle.VK_COMPONENT_SWIZZLE_IDENTITY
                };

                VkImageViewCreateInfo view_info = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                    image = _swapchain_images[i],
                    viewType = VkImageViewType.VK_IMAGE_VIEW_TYPE_2D,
                    format = _swapchain_format,
                    components = viewInfoComponents,
                    subresourceRange = new() { aspectMask = (uint)VkImageAspectFlagBits.VK_IMAGE_ASPECT_COLOR_BIT, baseMipLevel = 0, levelCount = 1, baseArrayLayer = 0, layerCount = 1 }
                };

                if (vkCreateImageView(_device, ref view_info, IntPtr.Zero, out _swapchain_image_views[i]) != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogError("Failed to create image view.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                    DestroySwapchain();
                    return false;
                }
            }

            // Create semaphores for presentation
            VkSemaphoreCreateInfo se_info = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO
            };

            var acquireSemaphorePtr = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                if (vkCreateSemaphore(_device, ref se_info, IntPtr.Zero, out _acquire_semaphore) != VkResult.VK_SUCCESS)
                {
                    _wrapper.LogHandler.LogError("Failed to create acquire semaphore.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                    DestroySwapchain();
                    return false;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(acquireSemaphorePtr);
            }

            if (vkCreateSemaphore(_device, ref se_info, IntPtr.Zero, out _present_semaphore) != VkResult.VK_SUCCESS)
            {
                _wrapper.LogHandler.LogError("Failed to create present semaphore.", "SK.Libretro.HardwareRenderProxyVulkan.CreateSwapchain");
                DestroySwapchain();
                return false;
            }

            return true;
        }

        private void DestroySwapchain()
        {
            if (_device.IsNull())
                return;

            if (_acquire_semaphore.IsNotNull())
            {
                vkDestroySemaphore(_device, _acquire_semaphore, IntPtr.Zero);
                _acquire_semaphore = VK_NULL_HANDLE;
            }

            if (_present_semaphore.IsNotNull())
            {
                vkDestroySemaphore(_device, _present_semaphore, IntPtr.Zero);
                _present_semaphore = VK_NULL_HANDLE;
            }

            foreach (var view in _swapchain_image_views)
            {
                if (view.IsNotNull())
                    vkDestroyImageView(_device, view, IntPtr.Zero);
            }
            _swapchain_image_views = Array.Empty<IntPtr>();
            _swapchain_images = Array.Empty<IntPtr>();

            if (_swapchain.IsNotNull())
            {
                vkDestroySwapchainKHR(_device, _swapchain, IntPtr.Zero);
                _swapchain = VK_NULL_HANDLE;
            }

            _swapchain_format = VkFormat.VK_FORMAT_UNDEFINED;
        }

        private bool CreateDepthStencilResources(uint width, uint height)
        {
            // Pick format
            if (Depth && Stencil)
                _depthStencilFormat = VkFormat.VK_FORMAT_D24_UNORM_S8_UINT;
            else if (Depth)
                _depthStencilFormat = VkFormat.VK_FORMAT_D32_SFLOAT;
            else if (Stencil)
                _depthStencilFormat = VkFormat.VK_FORMAT_S8_UINT;
            else
                return true;

            VkImageCreateInfo imageInfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO,
                imageType = VkImageType.VK_IMAGE_TYPE_2D,
                format = _depthStencilFormat,
                extent = new VkExtent3D { width = width, height = height, depth = 1 },
                mipLevels = 1,
                arrayLayers = 1,
                samples = VkSampleCountFlagBits.VK_SAMPLE_COUNT_1_BIT,
                tiling = VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                usage = (uint)VkImageUsageFlagBits.VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT,
                sharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE,
                initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED
            };
            if (vkCreateImage(_device, ref imageInfo, IntPtr.Zero, out _depthStencilImage) != VkResult.VK_SUCCESS)
                return false;

            VkMemoryRequirements memReq = new();
            vkGetImageMemoryRequirements(_device, _depthStencilImage, ref memReq);
            var memType = FindMemoryType(_gpu, memReq.memoryTypeBits, (uint)VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
            if (memType == uint.MaxValue)
                return false;
            VkMemoryAllocateInfo allocInfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
                allocationSize = memReq.size,
                memoryTypeIndex = memType
            };
            if (vkAllocateMemory(_device, ref allocInfo, IntPtr.Zero, out _depthStencilImageMemory) != VkResult.VK_SUCCESS)
                return false;
            if (vkBindImageMemory(_device, _depthStencilImage, _depthStencilImageMemory, 0) != VkResult.VK_SUCCESS)
                return false;

            VkImageAspectFlagBits aspect = 0;
            if (Depth)
                aspect |= VkImageAspectFlagBits.VK_IMAGE_ASPECT_DEPTH_BIT;
            if (Stencil)
                aspect |= VkImageAspectFlagBits.VK_IMAGE_ASPECT_STENCIL_BIT;

            VkImageViewCreateInfo viewInfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                image = _depthStencilImage,
                viewType = VkImageViewType.VK_IMAGE_VIEW_TYPE_2D,
                format = _depthStencilFormat,
                subresourceRange = new VkImageSubresourceRange
                {
                    aspectMask = (uint)aspect,
                    baseMipLevel = 0,
                    levelCount = 1,
                    baseArrayLayer = 0,
                    layerCount = 1
                }
            };

            return vkCreateImageView(_device, ref viewInfo, IntPtr.Zero, out _depthStencilImageView) == VkResult.VK_SUCCESS;
        }

        private void DestroyDepthStencilResources()
        {
            if (_device.IsNull())
                return;

            if (_depthStencilImageView.IsNotNull())
            {
                vkDestroyImageView(_device, _depthStencilImageView, IntPtr.Zero);
                _depthStencilImageView = VK_NULL_HANDLE;
            }

            if (_depthStencilImage.IsNotNull())
            {
                vkDestroyImage(_device, _depthStencilImage, IntPtr.Zero);
                _depthStencilImage = VK_NULL_HANDLE;
            }

            if (_depthStencilImageMemory.IsNotNull())
            {
                vkFreeMemory(_device, _depthStencilImageMemory, IntPtr.Zero);
                _depthStencilImageMemory = VK_NULL_HANDLE;
            }
            
            _depthStencilFormat = VkFormat.VK_FORMAT_UNDEFINED;
        }

        private bool CreateCommandPool()
        {
            if (_device.IsNull())
                return false;

            VkCommandPoolCreateInfo info = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
                flags = (uint)VkCommandPoolCreateFlagBits.VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
                queueFamilyIndex = _queueIndex
            };

            if (vkCreateCommandPool(_device, ref info, IntPtr.Zero, out _command_pool) != VkResult.VK_SUCCESS)
                return false;

            VkCommandBufferAllocateInfo ainfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
                commandPool = _command_pool,
                level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY,
                commandBufferCount = READBACK_SLOTS
            };

            var commandBuffersHandle = GCHandle.Alloc(_command_buffers, GCHandleType.Pinned);
            try
            {
                if (vkAllocateCommandBuffers(_device, ref ainfo, commandBuffersHandle.AddrOfPinnedObject()) != VkResult.VK_SUCCESS)
                    return false;
            }
            finally
            {
                commandBuffersHandle.Free();
            }

            VkFenceCreateInfo finfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_FENCE_CREATE_INFO,
                flags = (uint)VkFenceCreateFlagBits.VK_FENCE_CREATE_SIGNALED_BIT
            };

            for (var i = 0; i < READBACK_SLOTS; ++i)
                if (vkCreateFence(_device, ref finfo, IntPtr.Zero, out _fences[i]) != VkResult.VK_SUCCESS)
                    return false;

            return true;
        }

        private void DestroyCommandPool()
        {
            if (_device.IsNull())
                return;

            if (_command_pool.IsNotNull())
            {
                var commandBuffersHandle = GCHandle.Alloc(_command_buffers, GCHandleType.Pinned);
                try
                {
                    vkFreeCommandBuffers(_device, _command_pool, READBACK_SLOTS, commandBuffersHandle.AddrOfPinnedObject());
                }
                finally
                {
                    commandBuffersHandle.Free();
                }
                vkDestroyCommandPool(_device, _command_pool, IntPtr.Zero);
            }

            for (var i = 0; i < READBACK_SLOTS; ++i)
            {
                if (_fences[i].IsNotNull())
                {
                    vkDestroyFence(_device, _fences[i], IntPtr.Zero);
                    _fences[i] = VK_NULL_HANDLE;
                }
            }
            
            _command_pool = VK_NULL_HANDLE;
            
            for (var i = 0; i < READBACK_SLOTS; ++i)
                _command_buffers[i] = VK_NULL_HANDLE;
        }

        private sealed class InstanceWrapperContext
        {
            public List<string> required_extensions = new();
            public List<string> required_layers = new();
        };

        private static void AppendUniqueNames(List<string> list, List<string> extra)
        {
            if (extra is null || extra.Count == 0)
                return;

            for (var i = 0; i < extra.Count; ++i)
            {
                var name = extra[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                if (!list.Contains(name))
                    list.Add(name);
            }
        }

        private static void AppendUniqueExtensions(List<string> extensions, List<string> extra) => AppendUniqueNames(extensions, extra);

        private static void AppendUniqueLayers(List<string> layers, List<string> extra) => AppendUniqueNames(layers, extra);

        private static IntPtr CreateInstanceWrapper(IntPtr opaque, ref VkInstanceCreateInfo create_info)
        {
            if (opaque.IsNull() || create_info.sType != VkStructureType.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO)
                return IntPtr.Zero;

            var context = Marshal.PtrToStructure<InstanceWrapperContext>(opaque);
            List<string> extensions = new();
            if (create_info.ppEnabledExtensionNames.IsNotNull())
            {
                var current = create_info.ppEnabledExtensionNames;
                for (var i = 0; i < create_info.enabledExtensionCount; ++i)
                {
                    var ext = current.AsString();
                    if (!string.IsNullOrWhiteSpace(ext))
                        extensions.Add(ext);
                    current += IntPtr.Size;
                }
            }
            AppendUniqueExtensions(extensions, context.required_extensions);

            List<string> layers = new();
            if (create_info.ppEnabledLayerNames.IsNotNull())
            {
                var current = create_info.ppEnabledLayerNames;
                for (var i = 0; i < create_info.enabledLayerCount; ++i)
                {
                    var layer = current.AsString();
                    if (!string.IsNullOrWhiteSpace(layer))
                        layers.Add(layer);
                    current += IntPtr.Size;
                }
            }
            AppendUniqueLayers(layers, context.required_layers);

            create_info.enabledExtensionCount = (uint)extensions.Count;
            IntPtr[] extensionPtrs = null;
            if (extensions.Count == 0)
                create_info.ppEnabledExtensionNames = IntPtr.Zero;
            else
            {
                extensionPtrs = new IntPtr[extensions.Count];
                for (var i = 0; i < extensions.Count; ++i)
                    extensionPtrs[i] = Marshal.StringToHGlobalAnsi(extensions[i]);
                var unmanagedArray = Marshal.AllocHGlobal(IntPtr.Size * extensions.Count);
                for (var i = 0; i < extensions.Count; ++i)
                    Marshal.WriteIntPtr(unmanagedArray, i * IntPtr.Size, extensionPtrs[i]);
                create_info.ppEnabledExtensionNames = unmanagedArray;
            }

            create_info.enabledLayerCount = (uint)layers.Count;
            IntPtr[] layerPtrs = null;
            if (layers.Count == 0)
                create_info.ppEnabledLayerNames = IntPtr.Zero;
            else
            {
                layerPtrs = new IntPtr[layers.Count];
                for (var i = 0; i < layers.Count; ++i)
                    layerPtrs[i] = Marshal.StringToHGlobalAnsi(layers[i]);
                var unmanagedArray = Marshal.AllocHGlobal(IntPtr.Size * layers.Count);
                for (var i = 0; i < layers.Count; ++i)
                    Marshal.WriteIntPtr(unmanagedArray, i * IntPtr.Size, layerPtrs[i]);
                create_info.ppEnabledLayerNames = unmanagedArray;
            }

            if (vkCreateInstance(ref create_info, IntPtr.Zero, out var resultInstance) != VkResult.VK_SUCCESS)
                resultInstance = IntPtr.Zero;

            if (extensionPtrs is not null)
            {
                foreach (var ptr in extensionPtrs)
                    Marshal.FreeHGlobal(ptr);
                Marshal.FreeHGlobal(create_info.ppEnabledExtensionNames);
            }
            if (layerPtrs is not null)
            {
                foreach (var ptr in layerPtrs)
                    Marshal.FreeHGlobal(ptr);
                Marshal.FreeHGlobal(create_info.ppEnabledLayerNames);
            }

            return resultInstance;
        }

        private sealed class DeviceWrapperContext
        {
            public List<string> required_extensions = new();
        };

        private static IntPtr CreateDeviceWrapper(IntPtr gpu, IntPtr opaque, ref VkDeviceCreateInfo create_info)
        {
            if (gpu.IsNull() || opaque.IsNull())
                return IntPtr.Zero;

            var context = Marshal.PtrToStructure<DeviceWrapperContext>(opaque);
            List<string> extensions = new();
            if (create_info.ppEnabledExtensionNames.IsNotNull() && create_info.enabledExtensionCount > 0)
            {
                var current = create_info.ppEnabledExtensionNames;
                for (var i = 0; i < create_info.enabledExtensionCount; ++i)
                {
                    var ext = current.AsString();
                    if (!string.IsNullOrWhiteSpace(ext))
                        extensions.Add(ext);
                    current += IntPtr.Size;
                }
            }

            AppendUniqueExtensions(extensions, context.required_extensions);

            create_info.enabledExtensionCount = (uint)extensions.Count;
            IntPtr[] extensionPtrs = null;
            if (extensions.Count == 0)
                create_info.ppEnabledExtensionNames = IntPtr.Zero;
            else
            {
                extensionPtrs = new IntPtr[extensions.Count];
                for (var i = 0; i < extensions.Count; ++i)
                    extensionPtrs[i] = Marshal.StringToHGlobalAnsi(extensions[i]);
                var unmanagedArray = Marshal.AllocHGlobal(IntPtr.Size * extensions.Count);
                for (var i = 0; i < extensions.Count; ++i)
                    Marshal.WriteIntPtr(unmanagedArray, i * IntPtr.Size, extensionPtrs[i]);
                create_info.ppEnabledExtensionNames = unmanagedArray;
            }

            var pnextPtr = create_info.pNext;
            while (pnextPtr != IntPtr.Zero)
            {
                var baseInStruct = Marshal.PtrToStructure<VkBaseInStructure>(pnextPtr);
                if (baseInStruct.sType == VkStructureType.VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_FEATURES_2)
                {
                    create_info.pEnabledFeatures = IntPtr.Zero;
                    break;
                }
                pnextPtr = baseInStruct.pNext;
            }

            if (vkCreateDevice(gpu, ref create_info, IntPtr.Zero, out var resultDevice) != VkResult.VK_SUCCESS)
                resultDevice = IntPtr.Zero;

            if (extensionPtrs is not null)
            {
                foreach (var ptr in extensionPtrs)
                    Marshal.FreeHGlobal(ptr);
                Marshal.FreeHGlobal(create_info.ppEnabledExtensionNames);
            }

            return resultDevice;
        }

        private bool EnsureReadbackBuffer(uint slot, uint size)
        {
            if (_readback_sizes[slot] >= size && _readback_buffers[slot].IsNotNull())
                return true;

            if (_readback_buffers[slot].IsNotNull())
            {
                if (_readback_mapped_ptrs[slot].IsNotNull())
                {
                    vkUnmapMemory(_device, _readback_memories[slot]);
                    _readback_mapped_ptrs[slot] = VK_NULL_HANDLE;
                }
                vkDestroyBuffer(_device, _readback_buffers[slot], VK_NULL_HANDLE);
                _readback_buffers[slot] = VK_NULL_HANDLE;
            }

            if (_readback_memories[slot].IsNotNull())
            {
                vkFreeMemory(_device, _readback_memories[slot], VK_NULL_HANDLE);
                _readback_memories[slot] = VK_NULL_HANDLE;
            }

            // Use ulong for Vulkan size parameters
            ulong vkSize = size;
            VkBufferCreateInfo binfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO,
                size = vkSize,
                usage = (uint)VkBufferUsageFlagBits.VK_BUFFER_USAGE_TRANSFER_DST_BIT
            };

            if (vkCreateBuffer(_device, ref binfo, VK_NULL_HANDLE, out _readback_buffers[slot]) != VkResult.VK_SUCCESS)
                return false;

            VkMemoryRequirements memReq = new();
            vkGetBufferMemoryRequirements(_device, _readback_buffers[slot], ref memReq);

            var memoryType = FindReadbackMemoryType(_gpu, memReq.memoryTypeBits);
            if (memoryType == uint.MaxValue)
                return false;

            VkMemoryAllocateInfo minfo = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
                allocationSize = memReq.size,
                memoryTypeIndex = memoryType
            };

            if (vkAllocateMemory(_device, ref minfo, VK_NULL_HANDLE, out _readback_memories[slot]) != VkResult.VK_SUCCESS)
                return false;

            if (vkBindBufferMemory(_device, _readback_buffers[slot], _readback_memories[slot], 0) != VkResult.VK_SUCCESS)
                return false;

            // Map persistently so we can avoid map/unmap per-frame.
            var mapped = VK_NULL_HANDLE;
            // Always map the full allocated size (memReq.size)
            if (vkMapMemory(_device, _readback_memories[slot], 0, memReq.size, 0, ref mapped) != VkResult.VK_SUCCESS)
            {
                vkDestroyBuffer(_device, _readback_buffers[slot], VK_NULL_HANDLE);
                _readback_buffers[slot] = VK_NULL_HANDLE;
                vkFreeMemory(_device, _readback_memories[slot], VK_NULL_HANDLE);
                _readback_memories[slot] = VK_NULL_HANDLE;
                return false;
            }

            _readback_mapped_ptrs[slot] = mapped;
            _readback_sizes[slot] = (uint)memReq.size;
            return true;
        }

        private bool EnsureReadbackImage(uint slot, uint width, uint height, VkFormat format)
        {
            if (_readback_images[slot].IsNotNull()
             && _readback_image_extents[slot].width == width
             && _readback_image_extents[slot].height == height
             && _readback_image_formats[slot] == format)
                return true;

            if (_readback_images[slot].IsNotNull())
                vkDestroyImage(_device, _readback_images[slot], VK_NULL_HANDLE);
            if (_readback_image_memories[slot].IsNotNull())
                vkFreeMemory(_device, _readback_image_memories[slot], VK_NULL_HANDLE);

            _readback_images[slot] = VK_NULL_HANDLE;
            _readback_image_memories[slot] = VK_NULL_HANDLE;
            _readback_image_layouts[slot] = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            _readback_image_extents[slot] = new() { width = 0, height = 0 };
            _readback_image_formats[slot] = VkFormat.VK_FORMAT_UNDEFINED;

            VkImageCreateInfo image_info = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO,
                imageType = VkImageType.VK_IMAGE_TYPE_2D,
                format = format,
                mipLevels = 1,
                arrayLayers = 1,
                samples = VkSampleCountFlagBits.VK_SAMPLE_COUNT_1_BIT,
                tiling = VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                usage = (uint)(VkImageUsageFlagBits.VK_IMAGE_USAGE_TRANSFER_DST_BIT | VkImageUsageFlagBits.VK_IMAGE_USAGE_TRANSFER_SRC_BIT),
                sharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE,
                initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED
            };
            var image_info_extent = image_info.extent;
            image_info_extent.width = width;
            image_info_extent.height = height;
            image_info_extent.depth = 1;
            image_info.extent = image_info_extent;

            if (vkCreateImage(_device, ref image_info, VK_NULL_HANDLE, out _readback_images[slot]) != VkResult.VK_SUCCESS)
                return false;

            VkMemoryRequirements mem_req = new();
            vkGetImageMemoryRequirements(_device, _readback_images[slot], ref mem_req);

            var mem_type = FindMemoryType(_gpu, mem_req.memoryTypeBits, (uint)VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
            if (mem_type == uint.MaxValue)
                return false;

            VkMemoryAllocateInfo alloc_info = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
                allocationSize = mem_req.size,
                memoryTypeIndex = mem_type
            };

            if (vkAllocateMemory(_device, ref alloc_info, VK_NULL_HANDLE, out _readback_image_memories[slot]) != VkResult.VK_SUCCESS)
                return false;

            if (vkBindImageMemory(_device, _readback_images[slot], _readback_image_memories[slot], 0) != VkResult.VK_SUCCESS)
                return false;

            _readback_image_extents[slot] = new() { width = width, height = height };
            _readback_image_formats[slot] = format;
            return true;
        }

        private uint FindMemoryType(IntPtr gpu, uint typeBits, uint properties)
        {
            unsafe
            {
                var memProps = new VkPhysicalDeviceMemoryProperties();
                vkGetPhysicalDeviceMemoryProperties(gpu, ref memProps);
                for (var i = 0u; i < memProps.memoryTypeCount; ++i)
                {
                    var propertyFlags = memProps.memoryTypes[i * 2];
                    var heapIndex = memProps.memoryTypes[(i * 2) + 1];
                    if ((typeBits & (1u << (int)i)) != 0 && (propertyFlags & properties) == properties)
                        return i;
                }
                return uint.MaxValue;
            }
        }

        private uint FindReadbackMemoryType(IntPtr gpu, uint typeBits)
        {
            unsafe
            {
                var memProps = new VkPhysicalDeviceMemoryProperties();
                vkGetPhysicalDeviceMemoryProperties(gpu, ref memProps);
                var coherentCached = VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT
                                   | VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT
                                   | VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_HOST_CACHED_BIT;
                var coherent       = VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT
                                   | VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;

                // Best: cached + coherent, not device-local (system RAM, fast CPU reads)
                for (var i = 0u; i < memProps.memoryTypeCount; ++i)
                {
                    var propertyFlags = memProps.memoryTypes[i * 2];
                    if ((typeBits & (1u << (int)i)) != 0 && (propertyFlags & (uint)coherentCached) == (uint)coherentCached && (propertyFlags & (uint)VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT) == 0)
                        return i;
                }
                // Good: cached + coherent, any heap (e.g. ReBAR on AMD)
                for (var i = 0u; i < memProps.memoryTypeCount; ++i)
                {
                    var propertyFlags = memProps.memoryTypes[i * 2];
                    if ((typeBits & (1u << (int)i)) != 0 && (propertyFlags & (uint)coherentCached) == (uint)coherentCached)
                        return i;
                }
                // OK: coherent, not device-local
                for (var i = 0u; i < memProps.memoryTypeCount; ++i)
                {
                    var propertyFlags = memProps.memoryTypes[i * 2];
                    if ((typeBits & (1u << (int)i)) != 0 && (propertyFlags & (uint)coherent) == (uint)coherent && (propertyFlags & (uint)VkMemoryPropertyFlagBits.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT) == 0)
                        return i;
                }
                // Fallback: any coherent
                for (var i = 0u; i < memProps.memoryTypeCount; ++i)
                {
                    var propertyFlags = memProps.memoryTypes[i * 2];
                    if ((typeBits & (1u << (int)i)) != 0 && (propertyFlags & (uint)coherent) == (uint)coherent)
                        return i;
                }
                return uint.MaxValue;
            }
        }

        private bool SupportsBlitForFormat(IntPtr gpu, VkFormat format)
        {
            VkFormatProperties properties = new();
            vkGetPhysicalDeviceFormatProperties(gpu, format, ref properties);
            var optimal = properties.optimalTilingFeatures;
            return (optimal & (uint)VkFormatFeatureFlagBits.VK_FORMAT_FEATURE_BLIT_SRC_BIT) != 0
                && (optimal & (uint)VkFormatFeatureFlagBits.VK_FORMAT_FEATURE_BLIT_DST_BIT) != 0;
        }

        private bool IsBGRAFormat(VkFormat format)
            => format == VkFormat.VK_FORMAT_B8G8R8A8_UNORM || format == VkFormat.VK_FORMAT_B8G8R8A8_SRGB;
    }
}
