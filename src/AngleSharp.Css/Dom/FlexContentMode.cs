namespace AngleSharp.Css.Dom
{
    /// <summary>
    /// Represents the modes for flex content, i.e.,
    /// justify-content, align-content, and align-self.
    /// </summary>
    public enum FlexContentMode
    {
        /// <summary>
        /// Automatically determined.
        /// </summary>
        Auto,
        /// <summary>
        /// Follow the head.
        /// </summary>
        Start,
        /// <summary>
        /// Follow the tail.
        /// </summary>
        End,
        /// <summary>
        /// Follow the average.
        /// </summary>
        Center,
        /// <summary>
        /// Place space in between items.
        /// </summary>
        SpaceBetween,
        /// <summary>
        /// Fill with space around content.
        /// </summary>
        SpaceAround,
        /// <summary>
        /// Fill with equal space between and around content.
        /// </summary>
        SpaceEvenly,
        /// <summary>
        /// Align against the start edge of its own writing mode.
        /// </summary>
        SelfStart,
        /// <summary>
        /// Align against the end edge of its own writing mode.
        /// </summary>
        SelfEnd,
        /// <summary>
        /// Stretch to fill.
        /// </summary>
        Stretch,
        /// <summary>
        /// Follow the baseline.
        /// </summary>
        Baseline,
    }
}
