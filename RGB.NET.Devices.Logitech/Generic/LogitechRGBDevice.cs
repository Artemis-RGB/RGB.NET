﻿using System.Linq;
using RGB.NET.Core;

namespace RGB.NET.Devices.Logitech
{
    /// <inheritdoc cref="AbstractRGBDevice{TDeviceInfo}" />
    /// <inheritdoc cref="ILogitechRGBDevice" />
    /// <summary>
    /// Represents a generic Logitech-device. (keyboard, mouse, headset, mousepad).
    /// </summary>
    public abstract class LogitechRGBDevice<TDeviceInfo> : AbstractRGBDevice<TDeviceInfo>, ILogitechRGBDevice
        where TDeviceInfo : LogitechRGBDeviceInfo
    {
        #region Properties & Fields

        /// <inheritdoc />
        /// <summary>
        /// Gets information about the <see cref="T:RGB.NET.Devices.Logitech.LogitechRGBDevice" />.
        /// </summary>
        public override TDeviceInfo DeviceInfo { get; }

        /// <summary>
        /// Gets or sets the update queue performing updates for this device.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected UpdateQueue UpdateQueue { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogitechRGBDevice{TDeviceInfo}"/> class.
        /// </summary>
        /// <param name="info">The generic information provided by Logitech for the device.</param>
        protected LogitechRGBDevice(TDeviceInfo info)
        {
            this.DeviceInfo = info;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the device.
        /// </summary>
        public void Initialize(UpdateQueue updateQueue)
        {
            InitializeLayout();

            if (Size == Size.Invalid)
            {
                Rectangle ledRectangle = new Rectangle(this.Select(x => x.LedRectangle));
                Size = ledRectangle.Size + new Size(ledRectangle.Location.X, ledRectangle.Location.Y);
            }

            UpdateQueue = updateQueue;
        }

        /// <summary>
        /// Initializes the <see cref="Led"/> and <see cref="Size"/> of the device.
        /// </summary>
        protected virtual void InitializeLayout()
        {
            if (!(DeviceInfo is LogitechRGBDeviceInfo info)) return;

            string layout = info.ImageLayout;
            string layoutPath = info.LayoutPath;
            ApplyLayoutFromFile(PathHelper.GetAbsolutePath(this, @"Layouts\Logitech", $"{layoutPath}.xml"), layout, true);
        }

        #endregion
    }
}
