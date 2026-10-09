#nullable enable
namespace AngleSharp.Css.Dom
{
    using AngleSharp.Attributes;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a set of CSS properties.
    /// </summary>
    [DomNoInterfaceObject]
    public interface ICssProperties : IEnumerable<ICssProperty>
    {
        /// <summary>
        /// Gets the value of the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property.</returns>
        String this[String propertyName] { get; }

        /// <summary>
        /// Gets the number of properties in the declaration.
        /// </summary>
        [DomName("length")]
        Int32 Length { get; }

        /// <summary>
        /// Gets the value of a property with the given name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to get the value of.
        /// </param>
        /// <returns>A string or null if nothing has been set.</returns>
        [DomName("getPropertyValue")]
        String GetPropertyValue(String propertyName);

        /// <summary>
        /// Gets the full property model for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The computed or stored property model.</returns>
        ICssProperty? GetProperty(String propertyName);

        /// <summary>
        /// Gets the property model for the name the declaration carries it under, or null when no
        /// declaration here is it.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The stored property model, or null if there is none.</returns>
        /// <remarks>
        /// Unlike <see cref="GetProperty"/>, this one answers with null rather than with a property
        /// built for the occasion, and it does not rebuild a name the declaration has the longhands of
        /// but does not carry - which is what makes it cheap, and also what makes it answer less. What
        /// it does answer, it answers as <see cref="GetProperty"/> would: the property the declaration
        /// holds, or the longhand of a shorthand it holds.
        /// <para>
        /// Null therefore means "nothing here says anything about this name", and it is the caller's to
        /// read: a name nothing declared, a name declared away from this declaration, and a name this
        /// declaration holds only as longhands all arrive here, and only the caller knows which of them
        /// it is looking at and whether <see cref="GetProperty"/> is worth its price to tell.
        /// </para>
        /// </remarks>
        ICssProperty? TryGetProperty(String propertyName);

        /// <summary>
        /// Returns the optional priority, "important" or null, if no priority
        /// has been set.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to get the priority of.
        /// </param>
        /// <returns>A priority or null.</returns>
        [DomName("getPropertyPriority")]
        String GetPropertyPriority(String propertyName);

        /// <summary>
        /// Sets a property with the given name and value. Optionally the
        /// priority can be set to "important" or left empty.
        /// </summary>
        /// <param name="propertyName">The property's name.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="priority">The optional priority.</param>
        [DomName("setProperty")]
        void SetProperty(String propertyName, String propertyValue, String? priority = null);

        /// <summary>
        /// Removes the property with the given name and returns its value.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to be removed.
        /// </param>
        /// <returns>The value of the deleted property.</returns>
        [DomName("removeProperty")]
        String RemoveProperty(String propertyName);
    }
}
