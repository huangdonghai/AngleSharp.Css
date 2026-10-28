namespace AngleSharp.Css.Dom
{
    using System;
    using System.IO;

    /// <summary>
    /// A condition function the engine reads the shape of but knows nothing about. CSS Conditional 4
    /// §3.1 calls such a form a <c>&lt;general-enclosed&gt;</c> and makes it false, which is why it has
    /// to answer at all: `@supports not unknown()` is a true condition and the page's rules apply.
    /// </summary>
    sealed class FalseCondition : IConditionFunction
    {
        private readonly String _text;

        public FalseCondition(String text)
        {
            _text = text;
        }

        public Boolean Check(IRenderDevice device) => false;

        public void ToCss(TextWriter writer, IStyleFormatter formatter) => writer.Write(_text);
    }
}
