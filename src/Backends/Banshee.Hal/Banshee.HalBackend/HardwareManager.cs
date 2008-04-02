//
// HardwareManager.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Hyena;
using Banshee.Hardware;

namespace Banshee.HalBackend
{
    public sealed class HardwareManager : IHardwareManager
    {
        private Hal.Manager manager;
        
        public event DeviceAddedHandler DeviceAdded;
        public event DeviceRemovedHandler DeviceRemoved;
        
        public HardwareManager ()
        {
            manager = new Hal.Manager ();
            manager.DeviceAdded += OnHalDeviceAdded;
            manager.DeviceRemoved += OnHalDeviceRemoved;
        }
        
        public void Dispose ()
        {
        }
        
        private IEnumerable<T> GetAllBlockDevices<T> () where T : IBlockDevice
        {
            foreach (Hal.Device hal_device in manager.FindDeviceByCapabilityAsDevice ("block")) {
                IBlockDevice device = BlockDevice.Resolve<T> (manager, hal_device);
                if (device != null) {
                    yield return (T)device;
                }
            }
        }
        
        public IEnumerable<IBlockDevice> GetAllBlockDevices ()
        {
            return GetAllBlockDevices<IBlockDevice> ();
        }
        
        public IEnumerable<ICdromDevice> GetAllCdromDevices ()
        {
            return GetAllBlockDevices<ICdromDevice> ();
        }
        
        public IEnumerable<IDiskDevice> GetAllDiskDevices ()
        {
            return GetAllBlockDevices<IDiskDevice> ();
        }
        
        private void OnHalDeviceAdded (object o, Hal.DeviceAddedArgs args)
        {
            if (!args.Device.QueryCapability ("block")) {
                return;
            }
            
            IDevice device = BlockDevice.Resolve<IBlockDevice> (manager, args.Device);
            if (device == null) {
                device = Volume.Resolve (null, manager, args.Device);
            }
            
            if (device != null) {
                OnDeviceAdded (device);
            }
        }
        
        private void OnHalDeviceRemoved (object o, Hal.DeviceRemovedArgs args)
        {
            OnDeviceRemoved (args.Udi);
        }
        
        private void OnDeviceAdded (IDevice device)
        {
            DeviceAddedHandler handler = DeviceAdded;
            if (handler != null) {
                handler (this, new DeviceAddedArgs (device));
            }
        }
        
        private void OnDeviceRemoved (string uuid)
        {
            DeviceRemovedHandler handler = DeviceRemoved;
            if (handler != null) {
                handler (this, new DeviceRemovedArgs (uuid));
            }
        }
    }
}