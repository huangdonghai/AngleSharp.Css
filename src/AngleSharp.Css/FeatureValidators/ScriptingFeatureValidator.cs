namespace AngleSharp.Css.FeatureValidators
{
    using AngleSharp.Css.Converters;
    using AngleSharp.Css.Dom;
    using System;
    using static ValueConverters;

    sealed class ScriptingFeatureValidator : IFeatureValidator
    {
        public Boolean Validate(IMediaFeature feature, IRenderDevice renderDevice)
        {
            var available = ScriptingState.None;

            if (renderDevice.IsScripting)
            {
                available = renderDevice.Category == DeviceCategory.Screen ? ScriptingState.Enabled : ScriptingState.InitialOnly;
            }

            // A feature used without a value is a boolean query, which stands for every state but
            // none: a document whose scripting is off answers false, one that is merely past its
            // first load answers true with initial-only.
            // https://drafts.csswg.org/mediaqueries-5/#mq-boolean-context
            if (!feature.HasValue)
            {
                return available != ScriptingState.None;
            }

            var state = ScriptingStateConverter.Convert(feature.Value);
            return state != null && state.AsEnum<ScriptingState>() == available;
        }
    }
}
