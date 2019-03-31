﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RGB.NET.Core;
using RGB.NET.Devices.SteelSeries.API;
using RGB.NET.Devices.SteelSeries.HID;

namespace RGB.NET.Devices.SteelSeries
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a device provider responsible for SteelSeries- devices.
    /// </summary>
    public class SteelSeriesDeviceProvider : IRGBDeviceProvider
    {
        #region Properties & Fields

        private static SteelSeriesDeviceProvider _instance;
        /// <summary>
        /// Gets the singleton <see cref="SteelSeriesDeviceProvider"/> instance.
        /// </summary>
        public static SteelSeriesDeviceProvider Instance => _instance ?? new SteelSeriesDeviceProvider();

        /// <inheritdoc />
        /// <summary>
        /// Indicates if the SDK is initialized and ready to use.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets whether the application has exclusive access to the SDK or not.
        /// </summary>
        public bool HasExclusiveAccess => false;

        /// <inheritdoc />
        public IEnumerable<IRGBDevice> Devices { get; private set; }

        /// <summary>
        /// The <see cref="DeviceUpdateTrigger"/> used to trigger the updates for SteelSeries devices. 
        /// </summary>
        public DeviceUpdateTrigger UpdateTrigger { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SteelSeriesDeviceProvider"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this constructor is called even if there is already an instance of this class.</exception>
        public SteelSeriesDeviceProvider()
        {
            if (_instance != null) throw new InvalidOperationException($"There can be only one instance of type {nameof(SteelSeriesDeviceProvider)}");
            _instance = this;

            UpdateTrigger = new DeviceUpdateTrigger();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Initialize(RGBDeviceType loadFilter = RGBDeviceType.All, bool exclusiveAccessIfPossible = false, bool throwExceptions = false)
        {
            try
            {
                IsInitialized = false;

                UpdateTrigger?.Stop();

                if (!SteelSeriesSDK.IsInitialized)
                    SteelSeriesSDK.Initialize();

                IList<IRGBDevice> devices = new List<IRGBDevice>();
                DeviceChecker.LoadDeviceList(loadFilter);

                try
                {
                    foreach ((string model, RGBDeviceType deviceType, int _, SteelSeriesDeviceType steelSeriesDeviceType, string imageLayout, string layoutPath, Dictionary<LedId, SteelSeriesLedId> ledMapping) in DeviceChecker.ConnectedDevices)
                    {
                        ISteelSeriesRGBDevice device = new SteelSeriesRGBDevice(new SteelSeriesRGBDeviceInfo(deviceType, model, steelSeriesDeviceType, imageLayout, layoutPath));
                        SteelSeriesDeviceUpdateQueue updateQueue = new SteelSeriesDeviceUpdateQueue(UpdateTrigger, steelSeriesDeviceType.GetAPIName());
                        device.Initialize(updateQueue, ledMapping);
                        devices.Add(device);
                    }
                }
                catch { if (throwExceptions) throw; }

                UpdateTrigger?.Start();

                Devices = new ReadOnlyCollection<IRGBDevice>(devices);
                IsInitialized = true;
            }
            catch
            {
                IsInitialized = false;
                if (throwExceptions) throw;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void ResetDevices()
        {
            if (IsInitialized)
                try
                {
                    SteelSeriesSDK.ResetLeds();
                }
                catch {/* shit happens */}
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                SteelSeriesSDK.Dispose();
            }
            catch {/* shit happens */}
        }

        #endregion
    }
}
