using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit;

/// <summary>
/// Extensions methods for configuring Xunit logging.
/// </summary>
public static class XunitLoggerExtensions
{
    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="options">The logging options. If not provided the default options are used.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper, XunitLoggerOptions? options = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        
        if (outputHelper == null)
        {
            throw new ArgumentNullException(nameof(outputHelper));
        }
        
        return builder.AddProvider(new XunitLoggerProvider(outputHelper, options ?? new XunitLoggerOptions()));
    }

    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="configure">A delegate to configure the <see cref="XunitLoggerOptions"/>.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? configure)
    {
        var options = new XunitLoggerOptions();
        configure?.Invoke(options);
        
        return builder.AddXunit(outputHelper, options);
    }
    
    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggerFactory"/>.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="options">The logging options. If not provided the default options are used.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper outputHelper, XunitLoggerOptions? options = null)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }
        
        if (outputHelper == null)
        {
            throw new ArgumentNullException(nameof(outputHelper));
        }
        
        factory.AddProvider(new XunitLoggerProvider(outputHelper, options ?? new XunitLoggerOptions()));

        return factory;
    }
    
    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggerFactory"/>.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="configure">A delegate to configure the <see cref="XunitLoggerOptions"/>.</param>
    /// <returns></returns>
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? configure)
    {
        var options = new XunitLoggerOptions();
        configure?.Invoke(options);
        
        return factory.AddXunit(outputHelper, options);
    }
}