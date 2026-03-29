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
using static SK.Libretro.Vulkan;

namespace SK.Libretro.Header
{
    internal static partial class RETRO
    {
        public const int HW_RENDER_INTERFACE_VULKAN_VERSION = 5;
        public const int HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE_VULKAN_VERSION = 2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct retro_vulkan_image
    {
        public IntPtr image_view; // VkImageView
        public VkImageLayout image_layout;
        public VkImageViewCreateInfo create_info;
    };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_set_image_t(IntPtr handle,
                                                    IntPtr image,
                                                    uint num_semaphores,
                                                    IntPtr semaphores,
                                                    uint src_queue_family);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate uint retro_vulkan_get_sync_index_t(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate uint retro_vulkan_get_sync_index_mask_t(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_set_command_buffers_t(IntPtr handle, uint num_cmd, IntPtr cmd);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_wait_sync_index_t(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_lock_queue_t(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_unlock_queue_t(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_set_signal_semaphore_t(IntPtr handle, IntPtr semaphore);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate VkApplicationInfo retro_vulkan_get_application_info_t();

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct retro_vulkan_context
    {
        public IntPtr gpu; // VkPhysicalDevice
        public IntPtr device; // VkDevice
        public IntPtr queue; // VkQueue
        public uint queue_family_index;
        public IntPtr presentation_queue; // VkQueue
        public uint presentation_queue_family_index;
    };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate bool retro_vulkan_create_device_t(ref retro_vulkan_context context,
                                                        IntPtr instance,
                                                        IntPtr gpu,
                                                        IntPtr surface,
                                                        vkGetInstanceProcAddrDelegate get_instance_proc_addr,
                                                        IntPtr required_device_extensions,
                                                        uint num_required_device_extensions,
                                                        IntPtr required_device_layers,
                                                        uint num_required_device_layers,
                                                        out VkPhysicalDeviceFeatures required_features);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void retro_vulkan_destroy_device_t();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vulkan_create_instance_wrapper_t(IntPtr opaque, ref VkInstanceCreateInfo create_info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vulkan_create_instance_t(vkGetInstanceProcAddrDelegate get_instance_proc_addr,
                                                            ref VkApplicationInfo app,
                                                            retro_vulkan_create_instance_wrapper_t create_instance_wrapper,
                                                            IntPtr opaque);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vulkan_create_device_wrapper_t(IntPtr gpu, IntPtr opaque, ref VkDeviceCreateInfo create_info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate bool retro_vulkan_create_device2_t(ref retro_vulkan_context context,
                                                         IntPtr instance,
                                                         IntPtr gpu,
                                                         IntPtr surface,
                                                         vkGetInstanceProcAddrDelegate get_instance_proc_addr,
                                                         retro_vulkan_create_device_wrapper_t create_device_wrapper,
                                                         IntPtr opaque);


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct retro_hw_render_context_negotiation_interface_vulkan
    {
        public retro_hw_render_context_negotiation_interface_type interface_type;
        public uint interface_version;
        public IntPtr get_application_info;
        public IntPtr create_device;
        public IntPtr destroy_device;
        public IntPtr create_instance;
        public IntPtr create_device2;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct retro_hw_render_interface_vulkan
    {
        public retro_hw_render_interface_type interface_type;
        public uint interface_version;
        public IntPtr handle;
        public IntPtr instance; // VkInstance
        public IntPtr gpu; // VkPhysicalDevice
        public IntPtr device; // VkDevice
        public IntPtr get_device_proc_addr;
        public IntPtr get_instance_proc_addr;
        public IntPtr queue; // VkQueue
        public uint queue_index;
        public IntPtr set_image;
        public IntPtr get_sync_index;
        public IntPtr get_sync_index_mask;
        public IntPtr set_command_buffers;
        public IntPtr wait_sync_index;
        public IntPtr lock_queue;
        public IntPtr unlock_queue;
        public IntPtr set_signal_semaphore;
    };
}
