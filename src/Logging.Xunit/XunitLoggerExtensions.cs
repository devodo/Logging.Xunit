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
    /// <param name="builder">The builder.</param>
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
    /// <param name="builder">The builder.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="configure">A delegate to configure the <see cref="XunitLoggerOptions"/>.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? configure)
    {
        var options = new XunitLoggerOptions();
        configure?.Invoke(options);
        
        return builder.AddXunit(outputHelper, options);
    }
}