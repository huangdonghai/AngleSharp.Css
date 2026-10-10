#nullable enable
namespace AngleSharp.Css
{
    using AngleSharp.Css.Dom;
    using AngleSharp.Dom;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Caches the style collection of a document per render device, so that gathering the rules and
    /// building their bucket index happens once instead of once per element - or even, as the
    /// window's style collection used to do, once per call.
    /// </summary>
    /// <remarks>
    /// A cached collection is reused as long as none of the three things it was derived from has
    /// changed: the CSS object model revision, the state of the render device (media evaluation
    /// depends on it), and the sequence of sheets the document exposes. The last one is what catches
    /// changes that never reach the CSS object model, e.g. a &lt;style&gt; or &lt;link&gt; element
    /// being added or removed, or a sheet being disabled.
    ///
    /// Anything that cannot be verified causes a rebuild, so the worst case is the work that would
    /// have been done without the cache.
    /// </remarks>
    internal static class DocumentStyleCache
    {
        private static readonly ConditionalWeakTable<IDocument, DocumentEntry> _entries = new();

        // The state a freshly created DefaultRenderDevice reports. Every one of them reports exactly
        // this, so it can be compared against without instantiating a device on every lookup.
        private static readonly DeviceState _defaultState = new DeviceState(new DefaultRenderDevice());

        /// <summary>
        /// Gets the style collection for the given document and render device, reusing a previously
        /// built one whenever it is still valid.
        /// </summary>
        /// <param name="document">The document to get the style collection for.</param>
        /// <param name="device">The render device, or null when none was registered.</param>
        /// <param name="sheets">The sheets of the document, in document order.</param>
        /// <returns>The style collection.</returns>
        public static StyleCollection GetStyleCollection(IDocument document, IRenderDevice? device, IEnumerable<ICssStyleSheet> sheets)
        {
            var entry = _entries.GetValue(document, (IDocument _) => new DocumentEntry());
            var slot = entry.GetSlot(device);
            var state = device is null ? _defaultState : new DeviceState(device);
            var incoming = new List<ICssStyleSheet>();

            foreach (var sheet in sheets)
            {
                incoming.Add(sheet);
            }

            // Read after the enumeration, not before: walking the document's sheets can itself parse
            // a sheet that was not ready yet - which reports a mutation through the very hooks above
            // and would make an otherwise valid entry look stale forever after.
            var revision = CssMutationRevision.Current;

            if (slot.Collection is not null &&
                slot.Revision == revision &&
                slot.State.SameAs(state) &&
                IsSame(slot, incoming))
            {
                return slot.Collection;
            }

            var disabled = new List<Boolean>(incoming.Count);

            for (var i = 0; i < incoming.Count; i++)
            {
                disabled.Add(incoming[i].IsDisabled);
            }

            var collection = new StyleCollection(incoming, device ?? new DefaultRenderDevice());
            slot.Revision = revision;
            slot.State = state;
            slot.Sheets = incoming;
            slot.Disabled = disabled;
            slot.Collection = collection;
            return collection;
        }

        #region Helpers

        private static Boolean IsSame(DeviceSlot slot, List<ICssStyleSheet> sheets)
        {
            var previous = slot.Sheets;

            if (previous.Count != sheets.Count)
            {
                return false;
            }

            for (var i = 0; i < sheets.Count; i++)
            {
                var sheet = sheets[i];

                // The flag is read from the sheet itself, so a sheet that was disabled or enabled
                // since the snapshot is noticed even though it is still the same instance.
                if (!ReferenceEquals(sheet, previous[i]) || sheet.IsDisabled != slot.Disabled[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static Boolean SamePreferences(IReadOnlyDictionary<String, String>? left, IReadOnlyDictionary<String, String>? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null || left.Count != right.Count)
            {
                return false;
            }

            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out var value) || !String.Equals(pair.Value, value, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Nested types

        private sealed class DocumentEntry
        {
            private readonly DeviceSlot _default = new();
            private ConditionalWeakTable<IRenderDevice, DeviceSlot>? _devices;

            public DeviceSlot GetSlot(IRenderDevice? device)
            {
                if (device is null)
                {
                    return _default;
                }

                var devices = _devices ??= new ConditionalWeakTable<IRenderDevice, DeviceSlot>();
                return devices.GetValue(device, (IRenderDevice _) => new DeviceSlot());
            }
        }

        private sealed class DeviceSlot
        {
            public Int64 Revision = Int64.MinValue;
            public DeviceState State;
            public List<ICssStyleSheet> Sheets = new();
            public List<Boolean> Disabled = new();
            public StyleCollection? Collection;
        }

        private readonly struct DeviceState
        {
            private readonly DeviceCategory _category;
            private readonly Int32 _viewPortWidth;
            private readonly Int32 _viewPortHeight;
            private readonly Int32 _deviceWidth;
            private readonly Int32 _deviceHeight;
            private readonly Int32 _resolution;
            private readonly Int32 _frequency;
            private readonly Int32 _colorBits;
            private readonly Int32 _monochromeBits;
            private readonly Boolean _isInterlaced;
            private readonly Boolean _isScripting;
            private readonly Boolean _isGrid;
            private readonly Double _renderWidth;
            private readonly Double _renderHeight;
            private readonly Double _fontSize;
            private readonly IReadOnlyDictionary<String, String>? _preferences;

            public DeviceState(IRenderDevice device)
            {
                _category = device.Category;
                _viewPortWidth = device.ViewPortWidth;
                _viewPortHeight = device.ViewPortHeight;
                _deviceWidth = device.DeviceWidth;
                _deviceHeight = device.DeviceHeight;
                _resolution = device.Resolution;
                _frequency = device.Frequency;
                _colorBits = device.ColorBits;
                _monochromeBits = device.MonochromeBits;
                _isInterlaced = device.IsInterlaced;
                _isScripting = device.IsScripting;
                _isGrid = device.IsGrid;
                _renderWidth = device.RenderWidth;
                _renderHeight = device.RenderHeight;
                _fontSize = device.FontSize;
                _preferences = (device as IRenderDevicePreferences)?.Preferences;
            }

            public Boolean SameAs(in DeviceState other) =>
                _category == other._category &&
                _viewPortWidth == other._viewPortWidth &&
                _viewPortHeight == other._viewPortHeight &&
                _deviceWidth == other._deviceWidth &&
                _deviceHeight == other._deviceHeight &&
                _resolution == other._resolution &&
                _frequency == other._frequency &&
                _colorBits == other._colorBits &&
                _monochromeBits == other._monochromeBits &&
                _isInterlaced == other._isInterlaced &&
                _isScripting == other._isScripting &&
                _isGrid == other._isGrid &&
                _renderWidth == other._renderWidth &&
                _renderHeight == other._renderHeight &&
                _fontSize == other._fontSize &&
                SamePreferences(_preferences, other._preferences);
        }

        #endregion
    }
}
