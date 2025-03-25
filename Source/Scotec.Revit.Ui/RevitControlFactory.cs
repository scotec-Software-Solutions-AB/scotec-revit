// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Ui;

/// <summary>
///     Provides factory methods for creating Revit UI controls, such as push buttons, with various configurations.
/// </summary>
/// <remarks>
///     This class simplifies the creation of Revit UI elements by offering methods to generate
///     <see cref="Autodesk.Revit.UI.PushButtonData" /> objects with specified properties, including images, tooltips,
///     and command types. It supports both string-based and <see cref="System.Windows.Media.ImageSource" />-based image
///     inputs.
/// </remarks>
public static class RevitControlFactory
{
    /// <summary>
    ///     Creates a new instance of <see cref="Autodesk.Revit.UI.PushButtonData" /> with the specified properties.
    /// </summary>
    /// <typeparam name="TCommand">
    ///     The type of the command to be executed by the button. This type must implement a Revit
    ///     external command.
    /// </typeparam>
    /// <typeparam name="TCommandAvailability">
    ///     The type that determines the availability of the button. This type must implement a Revit command availability
    ///     class.
    /// </typeparam>
    /// <param name="name">The unique name of the button.</param>
    /// <param name="text">The display text of the button.</param>
    /// <param name="description">The tooltip description for the button.</param>
    /// <param name="smallImageResourcePathPath">The file path to the small image resource for the button.</param>
    /// <param name="largeImageResourcePath">The file path to the large image resource for the button.</param>
    /// <returns>A configured <see cref="Autodesk.Revit.UI.PushButtonData" /> instance.</returns>
    /// <remarks>
    ///     This method simplifies the creation of Revit push buttons by automatically configuring the button's properties,
    ///     including its name, text, tooltip, and associated images. It also associates the button with the specified command
    ///     and optionally with a command availability type.
    /// </remarks>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if <paramref name="smallImageResourcePathPath" /> or <paramref name="largeImageResourcePath" /> is null or
    ///     empty.
    /// </exception>
    public static PushButtonData CreateButtonData<TCommand, TCommandAvailability>(string name, string text, string description,
                                                                                  Uri smallImageResourcePathPath, Uri largeImageResourcePath)
    {
        return CreateButtonData(name, text, description, smallImageResourcePathPath, largeImageResourcePath, typeof(TCommand), typeof(TCommandAvailability));
    }

    /// <summary>
    ///     Creates a <see cref="Autodesk.Revit.UI.PushButtonData" /> object configured with the specified properties,
    ///     including command types and image sources.
    /// </summary>
    /// <typeparam name="TCommand">
    ///     The type of the command to be executed when the button is clicked. This type must implement
    ///     <see cref="Autodesk.Revit.UI.IExternalCommand" />.
    /// </typeparam>
    /// <typeparam name="TCommandAvailability">
    ///     The type that determines the availability of the command. This type must
    ///     implement <see cref="Autodesk.Revit.UI.IExternalCommandAvailability" />.
    /// </typeparam>
    /// <param name="name">The unique name of the button. This name is used to identify the button in the Revit UI.</param>
    /// <param name="text">The display text of the button, shown in the Revit UI.</param>
    /// <param name="description">The tooltip description displayed when hovering over the button.</param>
    /// <param name="smallImageResourcePath">
    ///     The small image displayed on the button, represented as an
    ///     <see cref="System.Windows.Media.ImageSource" />.
    /// </param>
    /// <param name="largeImageResourcePath">
    ///     The large image displayed on the button, represented as an
    ///     <see cref="System.Windows.Media.ImageSource" />.
    /// </param>
    /// <returns>A configured <see cref="Autodesk.Revit.UI.PushButtonData" /> object ready to be added to the Revit UI.</returns>
    /// <remarks>
    ///     This method simplifies the creation of Revit push buttons by allowing the use of generic parameters for the command
    ///     and availability types.
    ///     It also supports specifying images as <see cref="System.Windows.Media.ImageSource" /> objects.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="text" />, <paramref name="description" />,
    ///     <paramref name="smallImageResourcePath" />, or <paramref name="largeImageResourcePath" /> is <c>null</c>.
    /// </exception>
    public static PushButtonData CreateButtonData<TCommand, TCommandAvailability>(string name, string text, string description,
                                                                                  ImageSource smallImageResourcePath, ImageSource largeImageResourcePath)
    {
        return CreateButtonData(name, text, description, smallImageResourcePath, largeImageResourcePath, typeof(TCommand), typeof(TCommandAvailability));
    }

    /// <summary>
    ///     Creates a <see cref="Autodesk.Revit.UI.PushButtonData" /> object for a specified command type with the given
    ///     properties.
    /// </summary>
    /// <typeparam name="TCommand">
    ///     The type of the command to be executed when the button is clicked. This type must implement
    ///     <see cref="Autodesk.Revit.UI.IExternalCommand" />.
    /// </typeparam>
    /// <param name="name">The unique name of the button.</param>
    /// <param name="text">The display text of the button.</param>
    /// <param name="description">The tooltip description of the button.</param>
    /// <param name="smallImageResourcePathPath">The resource path to the small image for the button.</param>
    /// <param name="largeImageResourcePath">The resource path to the large image for the button.</param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.PushButtonData" /> object configured with the specified properties.
    /// </returns>
    /// <remarks>
    ///     This method simplifies the creation of a push button by automatically associating it with the specified command
    ///     type.
    ///     The images for the button are loaded from the provided resource paths.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="text" />, <paramref name="description" />,
    ///     <paramref name="smallImageResourcePathPath" />, or <paramref name="largeImageResourcePath" /> is <c>null</c>.
    /// </exception>
    public static PushButtonData CreateButtonData<TCommand>(string name, string text, string description, Uri smallImageResourcePathPath,
                                                            Uri largeImageResourcePath)
    {
        return CreateButtonData(name, text, description, smallImageResourcePathPath, largeImageResourcePath, typeof(TCommand));
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Autodesk.Revit.UI.PushButtonData" /> with the specified properties.
    /// </summary>
    /// <typeparam name="TCommand">
    ///     The type of the command to be executed when the button is clicked. This type must implement
    ///     a Revit external command.
    /// </typeparam>
    /// <param name="name">The unique name of the button. This name is used to identify the button in the Revit UI.</param>
    /// <param name="text">The display text of the button, which appears on the button in the Revit UI.</param>
    /// <param name="description">The tooltip text displayed when the user hovers over the button.</param>
    /// <param name="smallImageResourcePath">
    ///     The small image to be displayed on the button, represented as an
    ///     <see cref="System.Windows.Media.ImageSource" />.
    /// </param>
    /// <param name="largeImageResourcePath">
    ///     The large image to be displayed on the button, represented as an
    ///     <see cref="System.Windows.Media.ImageSource" />.
    /// </param>
    /// <returns>A configured <see cref="Autodesk.Revit.UI.PushButtonData" /> object that can be added to a Revit ribbon panel.</returns>
    /// <remarks>
    ///     This method simplifies the creation of a push button by allowing the use of
    ///     <see cref="System.Windows.Media.ImageSource" />
    ///     objects for specifying the button's images. The <typeparamref name="TCommand" /> type must be a valid Revit
    ///     external command.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="text" />, or <paramref name="description" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if <paramref name="name" /> is an empty string or contains invalid characters.
    /// </exception>
    public static PushButtonData CreateButtonData<TCommand>(string name, string text, string description, ImageSource smallImageResourcePath,
                                                            ImageSource largeImageResourcePath)
    {
        return CreateButtonData(name, text, description, smallImageResourcePath, largeImageResourcePath, typeof(TCommand));
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Autodesk.Revit.UI.PushButtonData" /> with the specified properties.
    /// </summary>
    /// <param name="name">The unique name of the button, used as an identifier.</param>
    /// <param name="text">The display text of the button.</param>
    /// <param name="description">The tooltip description for the button.</param>
    /// <param name="smallImageResourcePath">The file path to the small image resource for the button.</param>
    /// <param name="largeImageResourcePath">The file path to the large image resource for the button.</param>
    /// <param name="commandType">The <see cref="System.Type" /> of the command to be executed when the button is clicked.</param>
    /// <param name="commandAvailabilityType">
    ///     The <see cref="System.Type" /> of the command availability class, which determines whether the button is enabled or
    ///     disabled.
    ///     This parameter is optional and can be <c>null</c>.
    /// </param>
    /// <returns>A configured <see cref="Autodesk.Revit.UI.PushButtonData" /> instance.</returns>
    /// <remarks>
    ///     This method simplifies the creation of Revit push buttons by allowing developers to specify essential properties,
    ///     such as images, text, and associated command types. It supports optional command availability logic.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="text" />, <paramref name="description" />,
    ///     <paramref name="smallImageResourcePath" />, <paramref name="largeImageResourcePath" />, or
    ///     <paramref name="commandType" /> is <c>null</c>.
    /// </exception>
    public static PushButtonData CreateButtonData(string name, string text, string description, Uri smallImageResourcePath,
                                                  Uri largeImageResourcePath, Type commandType, Type? commandAvailabilityType = null)
    {
        return CreateButtonData(name, text, description, CreateImageSource(smallImageResourcePath), CreateImageSource(largeImageResourcePath), commandType,
            commandAvailabilityType);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Autodesk.Revit.UI.PushButtonData" /> with the specified properties.
    /// </summary>
    /// <param name="name">The unique name of the button.</param>
    /// <param name="text">The text displayed on the button.</param>
    /// <param name="description">The tooltip description for the button.</param>
    /// <param name="smallImageSource">The small image displayed on the button.</param>
    /// <param name="largeImageSource">The large image displayed on the button.</param>
    /// <param name="commandType">The type of the command to be executed when the button is clicked.</param>
    /// <param name="commandAvailabilityType">
    ///     The type that determines the availability of the button. This parameter is optional and can be <c>null</c>.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.PushButtonData" /> object configured with the specified properties.
    /// </returns>
    /// <remarks>
    ///     This method simplifies the creation of a push button in the Revit UI by allowing you to specify images,
    ///     tooltips, and command types. The <paramref name="commandAvailabilityType" /> parameter can be used to
    ///     control the button's availability based on specific conditions.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="text" />, <paramref name="description" />,
    ///     <paramref name="smallImageSource" />, <paramref name="largeImageSource" />, or <paramref name="commandType" /> is
    ///     <c>null</c>.
    /// </exception>
    public static PushButtonData CreateButtonData(string name, string text, string description, ImageSource smallImageSource, ImageSource largeImageSource,
                                                  Type commandType, Type? commandAvailabilityType = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (smallImageSource == null)
        {
            throw new ArgumentNullException(nameof(smallImageSource));
        }

        if (largeImageSource == null)
        {
            throw new ArgumentNullException(nameof(largeImageSource));
        }

        if (commandType == null)
        {
            throw new ArgumentNullException(nameof(commandType));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(text));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
        }

        var pushButtonData = new PushButtonData(name, text, commandType.Assembly.Location, commandType.FullName)
        {
            Image = smallImageSource,
            LargeImage = largeImageSource,
            ToolTip = description
        };

        if (commandAvailabilityType is not null)
        {
            pushButtonData.AvailabilityClassName = commandAvailabilityType.FullName;
        }

        return pushButtonData;
    }

    /// <summary>
    ///     Creates an <see cref="ImageSource" /> from the specified image resource path.
    /// </summary>
    /// <param name="imageResourcePath">
    ///     The URI of the image resource. This must be a valid, non-null <see cref="Uri" /> pointing to the image.
    /// </param>
    /// <returns>
    ///     An <see cref="ImageSource" /> object representing the image located at the specified URI.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="imageResourcePath" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method is used internally to load an image from a URI and convert it into an <see cref="ImageSource" />
    ///     that can be utilized in Revit UI elements.
    /// </remarks>
    private static ImageSource CreateImageSource(Uri imageResourcePath)
    {
        if (imageResourcePath == null)
        {
            throw new ArgumentNullException(nameof(imageResourcePath));
        }

        var image = new BitmapImage(imageResourcePath);

        return image;
    }
}
