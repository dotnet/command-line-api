// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.Reflection;
using System.Threading.Tasks;

namespace System.CommandLine.NamingConventionBinder;

/// <summary>
/// Provides methods for creating and working with command handlers that use naming conventions to bind parameters and model properties.
/// </summary>
public static class CommandHandler
{
    /// <summary>
    /// Creates a command handler based on a delegate.
    /// </summary>
    /// <param name="delegate">The delegate to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create(Delegate @delegate) =>
        HandlerDescriptor.FromDelegate(@delegate).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="method">The method to be called when the command handler is invoked.</param>
    /// <param name="target">A target instance to be used if the specified method is an instance method. This can be null if the method is static.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create(MethodInfo method, object? target = null) =>
        HandlerDescriptor.FromMethodInfo(method, target).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T>(
        Action<T> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2>(
        Action<T1, T2> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3>(
        Action<T1, T2, T3> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4>(
        Action<T1, T2, T3, T4> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5>(
        Action<T1, T2, T3, T4, T5> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
        Action<T1, T2, T3, T4, T5, T6> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
        Action<T1, T2, T3, T4, T5, T6, T7> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on an <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Action{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T>(
        Func<T, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2>(
        Func<T1, T2, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3>(
        Func<T1, T2, T3, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4>(
        Func<T1, T2, T3, T4, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5>(
        Func<T1, T2, T3, T4, T5, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
        Func<T1, T2, T3, T4, T5, T6, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
        Func<T1, T2, T3, T4, T5, T6, T7, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Int32}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Int32}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, int> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();
    
    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T>(
        Func<T, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2>(
        Func<T1, T2, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3>(
        Func<T1, T2, T3, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4>(
        Func<T1, T2, T3, T4, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5>(
        Func<T1, T2, T3, T4, T5, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
        Func<T1, T2, T3, T4, T5, T6, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
        Func<T1, T2, T3, T4, T5, T6, T7, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T>(
        Func<T, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2>(
        Func<T1, T2, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3>(
        Func<T1, T2, T3, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4>(
        Func<T1, T2, T3, T4, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5>(
        Func<T1, T2, T3, T4, T5, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6>(
        Func<T1, T2, T3, T4, T5, T6, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7>(
        Func<T1, T2, T3, T4, T5, T6, T7, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    /// <summary>
    /// Creates a command handler based on a <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/>.
    /// </summary>
    /// <param name="action">The <see cref="Func{T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,Task}"/> to be called when the command handler is invoked.</param>
    /// <returns>An instance of <see cref="ICommandHandler"/>.</returns>
    public static ICommandHandler Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task<int>> action) =>
        HandlerDescriptor.FromDelegate(action).GetCommandHandler();

    internal static async Task<int> GetExitCodeAsync(object returnValue, InvocationContext context)
    {
        switch (returnValue)
        {
            case Task<int> exitCodeTask:
                return await exitCodeTask;
            case Task task:
                await task;
                return context.ExitCode;
            case int exitCode:
                return exitCode;
            default:
                return context.ExitCode;
        }
    }
}