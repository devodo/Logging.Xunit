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
    /// <param name="options">The logging options.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper, XunitLoggerOptions options)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        
        if (outputHelper == null)
        {
            throw new ArgumentNullException(nameof(outputHelper));
        }
        
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        
        return builder.AddProvider(new XunitLoggerProvider(outputHelper, options));
    }
    
    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggingBuilder"/> using default <see cref="XunitLoggerOptions"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper)
    {
        return builder.AddXunit(outputHelper, new XunitLoggerOptions());
    }
    
    /// <summary>
    /// Add an <see cref="XunitLoggerProvider"/> to the <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="configure">A delegate to configure the <see cref="XunitLoggerOptions"/>.</param>
    /// <returns>The input builder.</returns>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper outputHelper, Action<XunitLoggerOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        
        var options = new XunitLoggerOptions();
        configure.Invoke(options);
        
        return builder.AddXunit(outputHelper, options);
    }
}