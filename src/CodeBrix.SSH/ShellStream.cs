using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.SSH.Abstractions;
using CodeBrix.SSH.Channels;
using CodeBrix.SSH.Common;

namespace CodeBrix.SSH; //was previously: Renci.SshNet;

/// <summary>
/// Contains operation for working with SSH Shell.
/// </summary>
public sealed class ShellStream : Stream
{
    private const int DefaultBufferSize = 1024;

    private readonly ISession _session;
    private readonly Encoding _encoding;
    private readonly IChannelSession _channel;
    private readonly byte[] _carriageReturnBytes;
    private readonly byte[] _lineFeedBytes;
    private readonly bool _noTerminal;

    private readonly object _sync = new object();

    private System.Net.ArrayBuffer _readBuffer;
    private System.Net.ArrayBuffer _writeBuffer;

    private volatile bool _autoFlush;
    private volatile bool _disableReadBuffering;

    private bool _disposed;

    /// <summary>
    /// Occurs when data was received.
    /// </summary>
    public event EventHandler<ShellDataEventArgs> DataReceived;

    /// <summary>
    /// Occurs when an error occurred.
    /// </summary>
    public event EventHandler<ExceptionEventArgs> ErrorOccurred;

    /// <summary>
    /// Occurs when the channel was closed.
    /// </summary>
    public event EventHandler<EventArgs> Closed;

    /// <summary>
    /// Gets a value indicating whether data is available on the <see cref="ShellStream"/> to be read.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if data is available to be read; otherwise, <see langword="false"/>.
    /// </value>
    public bool DataAvailable
    {
        get
        {
            lock (_sync)
            {
                return _readBuffer.ActiveLength > 0;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the byte-oriented write methods
    /// (<see cref="Write(byte[], int, int)"/>, <see cref="Write(ReadOnlySpan{byte})"/>,
    /// <see cref="WriteByte(byte)"/> and their asynchronous counterparts) automatically
    /// call <see cref="Flush"/> after each write. The default is <see langword="false"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if every byte-oriented write is immediately flushed to the
    /// channel; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The string-oriented <see cref="Write(string)"/> and <see cref="WriteLine(string)"/>
    /// methods always flush, regardless of the value of this property.
    /// </remarks>
    public bool AutoFlush
    {
        get { return _autoFlush; }
        set { _autoFlush = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether incoming data bypasses the internal read
    /// buffer and is delivered solely via the <see cref="DataReceived"/> event. The default
    /// is <see langword="false"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if incoming data is delivered only via <see cref="DataReceived"/>;
    /// <see langword="false"/> if incoming data is also committed to the internal read buffer.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is an opt-in for event-style consumers (such as interactive terminal hosts) that
    /// receive all data via <see cref="DataReceived"/> and never read from the stream: without
    /// it, the internal read buffer grows without bound because nothing drains it.
    /// </para>
    /// <para>
    /// While this property is <see langword="true"/>, the buffer-reading members —
    /// <see cref="Read()"/>, <see cref="Read(byte[], int, int)"/>, <see cref="Read(Span{byte})"/>,
    /// <see cref="ReadByte"/>, <see cref="ReadLine()"/> and the <c>Expect</c>/<c>BeginExpect</c>
    /// overloads — throw <see cref="InvalidOperationException"/> instead of blocking on a buffer
    /// that will never fill. A thread already blocked in one of those members is woken and
    /// throws when the property is set to <see langword="true"/>.
    /// </para>
    /// <para>
    /// Setting this property to <see langword="true"/> discards any data currently in the read
    /// buffer (an event subscriber has already received a copy of every byte). Set it back to
    /// <see langword="false"/> to resume buffering from that point on. Ideally, set it once,
    /// right after creating the <see cref="ShellStream"/> and before any data arrives.
    /// </para>
    /// </remarks>
    public bool DisableReadBuffering
    {
        get
        {
            return _disableReadBuffering;
        }
        set
        {
            lock (_sync)
            {
                if (_disableReadBuffering == value)
                {
                    return;
                }

                _disableReadBuffering = value;

                if (value)
                {
                    _readBuffer.Discard(_readBuffer.ActiveLength);
                }

                Monitor.PulseAll(_sync);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellStream"/> class.
    /// </summary>
    /// <param name="session">The SSH session.</param>
    /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
    /// <param name="columns">The terminal width in columns.</param>
    /// <param name="rows">The terminal width in rows.</param>
    /// <param name="width">The terminal width in pixels.</param>
    /// <param name="height">The terminal height in pixels.</param>
    /// <param name="terminalModeValues">The terminal mode values.</param>
    /// <param name="bufferSize">The size of the buffer.</param>
    /// <exception cref="SshException">The channel could not be opened.</exception>
    /// <exception cref="SshException">The pseudo-terminal request was not accepted by the server.</exception>
    /// <exception cref="SshException">The request to start a shell was not accepted by the server.</exception>
    internal ShellStream(ISession session, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues, int bufferSize)
           : this(session, bufferSize, noTerminal: false)
    {
        try
        {
            _channel.Open();

            if (!_channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues))
            {
                throw new SshException("The pseudo-terminal request was not accepted by the server. Consult the server log for more information.");
            }

            if (!_channel.SendShellRequest())
            {
                throw new SshException("The request to start a shell was not accepted by the server. Consult the server log for more information.");
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellStream"/> class.
    /// </summary>
    /// <param name="session">The SSH session.</param>
    /// <param name="bufferSize">The size of the buffer.</param>
    /// <exception cref="SshException">The channel could not be opened.</exception>
    /// <exception cref="SshException">The request to start a shell was not accepted by the server.</exception>
    internal ShellStream(ISession session, int bufferSize)
        : this(session, bufferSize, noTerminal: true)
    {
        try
        {
            _channel.Open();

            if (!_channel.SendShellRequest())
            {
                throw new SshException("The request to start a shell was not accepted by the server. Consult the server log for more information.");
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellStream"/> class.
    /// </summary>
    /// <param name="session">The SSH session.</param>
    /// <param name="bufferSize">The size of the buffer.</param>
    /// <param name="noTerminal">Disables pseudo terminal allocation or not.</param>
    /// <exception cref="SshException">The channel could not be opened.</exception>
    private ShellStream(ISession session, int bufferSize, bool noTerminal)
    {
        if (bufferSize == -1)
        {
            bufferSize = DefaultBufferSize;
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _encoding = session.ConnectionInfo.Encoding;
        _session = session;
        _carriageReturnBytes = _encoding.GetBytes("\r");
        _lineFeedBytes = _encoding.GetBytes("\n");

        _channel = _session.CreateChannelSession();
        _channel.DataReceived += Channel_DataReceived;
        _channel.Closed += Channel_Closed;
        _session.Disconnected += Session_Disconnected;
        _session.ErrorOccured += Session_ErrorOccurred;

        _readBuffer = new System.Net.ArrayBuffer(bufferSize);
        _writeBuffer = new System.Net.ArrayBuffer(bufferSize);

        _noTerminal = noTerminal;
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports reading.
    /// </summary>
    /// <value>
    /// <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// It is safe to read from <see cref="ShellStream"/> even after disposal.
    /// </remarks>
    public override bool CanRead
    {
        get { return true; }
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    /// <value>
    /// <see langword="false"/>.
    /// </value>
    public override bool CanSeek
    {
        get { return false; }
    }

    /// <summary>
    /// Gets a value indicating whether the current stream supports writing.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this stream has not been disposed and the underlying channel
    /// is still open, otherwise <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A value of <see langword="true"/> does not necessarily mean a write will succeed. It is possible
    /// that the channel is closed and/or the stream is disposed by another thread between a call to
    /// <see cref="CanWrite"/> and the call to write.
    /// </remarks>
    public override bool CanWrite
    {
        get { return !_disposed; }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

        if (_writeBuffer.ActiveLength > 0)
        {
            _channel.SendData(
                _writeBuffer.DangerousGetUnderlyingBuffer(),
                _writeBuffer.ActiveStartOffset,
                _writeBuffer.ActiveLength);

            _writeBuffer.Discard(_writeBuffer.ActiveLength);
        }
    }

    /// <summary>
    /// Gets the number of bytes currently available for reading.
    /// </summary>
    /// <value>A value representing the length of the stream in bytes.</value>
    public override long Length
    {
        get
        {
            lock (_sync)
            {
                return _readBuffer.ActiveLength;
            }
        }
    }

    /// <summary>
    /// This property always returns 0, and throws <see cref="NotSupportedException"/>
    /// when calling the setter.
    /// </summary>
    /// <value>
    /// 0.
    /// </value>
    /// <exception cref="NotSupportedException">The setter is called.</exception>
#pragma warning disable SA1623 // The property's documentation should begin with 'Gets or sets'
    public override long Position
#pragma warning restore SA1623 // The property's documentation should begin with 'Gets or sets'
    {
        get { return 0; }
        set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// This method always throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
    /// <returns>Never.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// This method always throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes.</param>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Sends new dimensions of the window (terminal) to the server.
    /// </summary>
    /// <param name="columns">The terminal width in columns.</param>
    /// <param name="rows">The terminal height in rows.</param>
    /// <param name="width">The terminal width in pixels.</param>
    /// <param name="height">The terminal height in pixels.</param>
    /// <remarks>
    /// The column/row dimensions override the pixel dimensions (when nonzero). Pixel dimensions refer
    /// to the drawable area of the window.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    public void ChangeWindowSize(uint columns, uint rows, uint width, uint height)
    {
        ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

        _channel.SendWindowChangeRequest(columns, rows, width, height);
    }

    /// <summary>
    /// Expects the specified expression and performs action when one is found.
    /// </summary>
    /// <param name="expectActions">The expected expressions and actions to perform.</param>
    public void Expect(params ExpectAction[] expectActions)
    {
        Expect(Timeout.InfiniteTimeSpan, expectActions);
    }

    /// <summary>
    /// Expects the specified expression and performs action when one is found.
    /// </summary>
    /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="expectActions">The expected expressions and actions to perform, if the specified time elapsed and expected condition have not met, that method will exit without executing any action.</param>
    /// <remarks>
    /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter,
    /// this method blocks indefinitely until either the regex matches the data in the buffer, or the stream
    /// is closed (via disposal or via the underlying channel closing).
    /// </remarks>
    public void Expect(TimeSpan timeout, params ExpectAction[] expectActions)
    {
        _ = ExpectRegex(timeout, lookback: -1, expectActions);
    }

    /// <summary>
    /// Expects the specified expression and performs action when one is found.
    /// </summary>
    /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
    /// <param name="expectActions">The expected expressions and actions to perform, if the specified time elapsed and expected condition have not met, that method will exit without executing any action.</param>
    /// <remarks>
    /// <para>
    /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter,
    /// this method blocks indefinitely until either the regex matches the data in the buffer, or the stream
    /// is closed (via disposal or via the underlying channel closing).
    /// </para>
    /// <para>
    /// Use the <paramref name="lookback"/> parameter to constrain the search space to a fixed-size rolling window at the end of the buffer.
    /// This can reduce the amount of work done in cases where lots of output from the shell is expected to be received before the matching expression is found.
    /// </para>
    /// <para>
    /// Note: in situations with high volumes of data and a small value for <paramref name="lookback"/>, some data may not be searched through.
    /// It is recommended to set <paramref name="lookback"/> to a large enough value to be able to search all data as it comes in,
    /// but which still places a limit on the amount of work needed.
    /// </para>
    /// </remarks>
    public void Expect(TimeSpan timeout, int lookback, params ExpectAction[] expectActions)
    {
        _ = ExpectRegex(timeout, lookback, expectActions);
    }

    /// <summary>
    /// Expects the expression specified by text.
    /// </summary>
    /// <param name="text">The text to expect.</param>
    /// <returns>
    /// The text available in the shell up to and including the expected text,
    /// or <see langword="null"/> if the the stream is closed without a match.
    /// </returns>
    public string Expect(string text)
    {
        return Expect(text, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Expects the expression specified by text.
    /// </summary>
    /// <param name="text">The text to expect.</param>
    /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
    /// <returns>
    /// The text available in the shell up to and including the expected expression,
    /// or <see langword="null"/> if the specified time has elapsed or the stream is closed
    /// without a match.
    /// </returns>
    /// <remarks><inheritdoc cref="Expect(TimeSpan, int, ExpectAction[])"/></remarks>
    public string Expect(string text, TimeSpan timeout, int lookback = -1)
    {
        ValidateTimeout(timeout);
        ValidateLookback(lookback);

        var timeoutTime = DateTime.Now.Add(timeout);

        var expectBytes = _encoding.GetBytes(text);

        lock (_sync)
        {
            while (true)
            {
                ThrowIfReadBufferingDisabled();

                var searchHead = lookback == -1
                    ? 0
                    : Math.Max(0, _readBuffer.ActiveLength - lookback);

                var indexOfMatch = _readBuffer.ActiveReadOnlySpan.Slice(searchHead).IndexOf(expectBytes);

                if (indexOfMatch >= 0)
                {
                    var readLength = searchHead + indexOfMatch + expectBytes.Length;

                    var returnText = GetString(readLength);

                    _readBuffer.Discard(readLength);

                    return returnText;
                }

                if (_disposed)
                {
                    return null;
                }

                if (timeout == Timeout.InfiniteTimeSpan)
                {
                    _ = Monitor.Wait(_sync);
                }
                else
                {
                    var waitTimeout = timeoutTime - DateTime.Now;

                    if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                    {
                        return null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Expects the expression specified by regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to expect.</param>
    /// <returns>
    /// The text available in the shell up to and including the expected expression,
    /// or <see langword="null"/> if the stream is closed without a match.
    /// </returns>
    public string Expect(Regex regex)
    {
        return Expect(regex, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Expects the expression specified by regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to expect.</param>
    /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
    /// <returns>
    /// The text available in the shell up to and including the expected expression,
    /// or <see langword="null"/> if the specified timeout has elapsed or the stream
    /// is closed without a match.
    /// </returns>
    /// <remarks>
    /// <inheritdoc cref="Expect(TimeSpan, int, ExpectAction[])"/>
    /// </remarks>
    public string Expect(Regex regex, TimeSpan timeout, int lookback = -1)
    {
        return ExpectRegex(timeout, lookback, [new ExpectAction(regex, s => { })]);
    }

    private string ExpectRegex(TimeSpan timeout, int lookback, ExpectAction[] expectActions)
    {
        ValidateTimeout(timeout);
        ValidateLookback(lookback);

        var timeoutTime = DateTime.Now.Add(timeout);

        lock (_sync)
        {
            while (true)
            {
                ThrowIfReadBufferingDisabled();

                var bufferText = GetString(_readBuffer.ActiveLength);

                var searchStart = lookback == -1
                    ? 0
                    : Math.Max(bufferText.Length - lookback, 0);

                foreach (var expectAction in expectActions)
                {
                    var matchEnumerator = expectAction.Expect.EnumerateMatches(bufferText.AsSpan(searchStart));

                    if (matchEnumerator.MoveNext())
                    {
                        var match = matchEnumerator.Current;

                        var returnText = bufferText.Substring(0, searchStart + match.Index + match.Length);
                        _readBuffer.Discard(_encoding.GetByteCount(returnText));

                        expectAction.Action(returnText);

                        return returnText;
                    }
                }

                if (_disposed)
                {
                    return null;
                }

                if (timeout == Timeout.InfiniteTimeSpan)
                {
                    Monitor.Wait(_sync);
                }
                else
                {
                    var waitTimeout = timeoutTime - DateTime.Now;

                    if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                    {
                        return null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Begins the expect.
    /// </summary>
    /// <param name="expectActions">The expect actions.</param>
    /// <returns>
    /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
    /// </returns>
    public IAsyncResult BeginExpect(params ExpectAction[] expectActions)
    {
        return BeginExpect(Timeout.InfiniteTimeSpan, callback: null, state: null, expectActions);
    }

    /// <summary>
    /// Begins the expect.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <param name="expectActions">The expect actions.</param>
    /// <returns>
    /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
    /// </returns>
    public IAsyncResult BeginExpect(AsyncCallback callback, params ExpectAction[] expectActions)
    {
        return BeginExpect(Timeout.InfiniteTimeSpan, callback, state: null, expectActions);
    }

    /// <summary>
    /// Begins the expect.
    /// </summary>
    /// <param name="callback">The callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="expectActions">The expect actions.</param>
    /// <returns>
    /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
    /// </returns>
    public IAsyncResult BeginExpect(AsyncCallback callback, object state, params ExpectAction[] expectActions)
    {
        return BeginExpect(Timeout.InfiniteTimeSpan, callback, state, expectActions);
    }

    /// <summary>
    /// Begins the expect.
    /// </summary>
    /// <param name="timeout">The timeout. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="callback">The callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="expectActions">The expect actions.</param>
    /// <returns>
    /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
    /// </returns>
    public IAsyncResult BeginExpect(TimeSpan timeout, AsyncCallback callback, object state, params ExpectAction[] expectActions)
    {
        return BeginExpect(timeout, lookback: -1, callback, state, expectActions);
    }

    /// <summary>
    /// Begins the expect.
    /// </summary>
    /// <param name="timeout">The timeout. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <param name="lookback">The amount of data to search through from the most recent data in the buffer, or -1 to always search the entire buffer.</param>
    /// <param name="callback">The callback.</param>
    /// <param name="state">The state.</param>
    /// <param name="expectActions">The expect actions.</param>
    /// <returns>
    /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
    /// </returns>
    public IAsyncResult BeginExpect(TimeSpan timeout, int lookback, AsyncCallback callback, object state, params ExpectAction[] expectActions)
    {
        ThrowIfReadBufferingDisabled();

        return TaskToAsyncResult.Begin(Task.Run(() => ExpectRegex(timeout, lookback, expectActions)), callback, state);
    }

    /// <summary>
    /// Ends the execute.
    /// </summary>
    /// <param name="asyncResult">The async result.</param>
    /// <returns>
    /// The text available in the shell up to and including the expected expression.
    /// </returns>
    public string EndExpect(IAsyncResult asyncResult)
    {
        return TaskToAsyncResult.End<string>(asyncResult);
    }

    /// <summary>
    /// Reads the next line from the shell. If a line is not available it will block and wait for a new line.
    /// </summary>
    /// <returns>
    /// The line read from the shell.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method blocks indefinitely until either a line is available in the buffer, or the stream is closed
    /// (via disposal or via the underlying channel closing).
    /// </para>
    /// <para>
    /// When the stream is closed and there are no more newlines in the buffer, this method returns the remaining data
    /// (if any) and then <see langword="null"/> indicating that no more data is in the buffer.
    /// </para>
    /// </remarks>
    public string ReadLine()
    {
        return ReadLine(Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Reads a line from the shell. If line is not available it will block the execution and will wait for new line.
    /// </summary>
    /// <param name="timeout">Time to wait for input. Must non-negative or equal to -1 millisecond (for infinite timeout).</param>
    /// <returns>
    /// The line read from the shell, or <see langword="null"/> when no input is received for the specified timeout.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If a TimeSpan representing -1 millisecond is specified for the <paramref name="timeout"/> parameter, this method
    /// blocks indefinitely until either a line is available in the buffer, or the stream is closed (via disposal or via
    /// the underlying channel closing).
    /// </para>
    /// <para>
    /// When the stream is closed and there are no more newlines in the buffer, this method returns the remaining data
    /// (if any) and then <see langword="null"/> indicating that no more data is in the buffer.
    /// </para>
    /// </remarks>
    public string ReadLine(TimeSpan timeout)
    {
        ValidateTimeout(timeout);

        var timeoutTime = DateTime.Now.Add(timeout);

        lock (_sync)
        {
            while (true)
            {
                ThrowIfReadBufferingDisabled();

                var indexOfCr = _readBuffer.ActiveReadOnlySpan.IndexOf(_carriageReturnBytes);

                if (indexOfCr >= 0)
                {
                    // We have found \r. We only need to search for \n up to and just after the \r
                    // (in order to consume \r\n if we can).
                    var indexOfLf = indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length <= _readBuffer.ActiveLength
                        ? _readBuffer.ActiveReadOnlySpan.Slice(0, indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length).IndexOf(_lineFeedBytes)
                        : _readBuffer.ActiveReadOnlySpan.Slice(0, indexOfCr).IndexOf(_lineFeedBytes);

                    if (indexOfLf >= 0 && indexOfLf < indexOfCr)
                    {
                        // If there is \n before the \r, then return up to the \n
                        var returnText = GetString(indexOfLf);

                        _readBuffer.Discard(indexOfLf + _lineFeedBytes.Length);

                        return returnText;
                    }
                    else if (indexOfLf == indexOfCr + _carriageReturnBytes.Length)
                    {
                        // If we have \r\n, then consume both
                        var returnText = GetString(indexOfCr);

                        _readBuffer.Discard(indexOfCr + _carriageReturnBytes.Length + _lineFeedBytes.Length);

                        return returnText;
                    }
                    else
                    {
                        // Return up to the \r
                        var returnText = GetString(indexOfCr);

                        _readBuffer.Discard(indexOfCr + _carriageReturnBytes.Length);

                        return returnText;
                    }
                }
                else
                {
                    // There is no \r. What about \n?
                    var indexOfLf = _readBuffer.ActiveReadOnlySpan.IndexOf(_lineFeedBytes);

                    if (indexOfLf >= 0)
                    {
                        var returnText = GetString(indexOfLf);

                        _readBuffer.Discard(indexOfLf + _lineFeedBytes.Length);

                        return returnText;
                    }
                }

                if (_disposed)
                {
                    var lastLine = _readBuffer.ActiveLength == 0
                        ? null
                        : GetString(_readBuffer.ActiveLength);

                    _readBuffer.Discard(_readBuffer.ActiveLength);

                    return lastLine;
                }

                if (timeout == Timeout.InfiniteTimeSpan)
                {
                    _ = Monitor.Wait(_sync);
                }
                else
                {
                    var waitTimeout = timeoutTime - DateTime.Now;

                    if (waitTimeout < TimeSpan.Zero || !Monitor.Wait(_sync, waitTimeout))
                    {
                        return null;
                    }
                }
            }
        }
    }

    private static void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Value must be non-negative or equal to -1 millisecond (for infinite timeout)");
        }
    }

    private static void ValidateLookback(int lookback)
    {
        if (lookback is <= 0 and not -1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookback), "Value must be positive or equal to -1 (for no window)");
        }
    }

    private void ThrowIfReadBufferingDisabled()
    {
        if (_disableReadBuffering)
        {
            throw new InvalidOperationException("The internal read buffer is disabled because DisableReadBuffering is set. Receive data via the DataReceived event instead.");
        }
    }

    /// <summary>
    /// Reads all of the text currently available in the shell.
    /// </summary>
    /// <returns>
    /// The text available in the shell.
    /// </returns>
    public string Read()
    {
        lock (_sync)
        {
            ThrowIfReadBufferingDisabled();

            var text = GetString(_readBuffer.ActiveLength);

            _readBuffer.Discard(_readBuffer.ActiveLength);

            return text;
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        return Read(buffer.AsSpan(offset, count));
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        lock (_sync)
        {
            ThrowIfReadBufferingDisabled();

            while (_readBuffer.ActiveLength == 0 && !_disposed)
            {
                _ = Monitor.Wait(_sync);

                ThrowIfReadBufferingDisabled();
            }

            var bytesRead = Math.Min(buffer.Length, _readBuffer.ActiveLength);

            _readBuffer.ActiveReadOnlySpan.Slice(0, bytesRead).CopyTo(buffer);

            _readBuffer.Discard(bytesRead);

            return bytesRead;
        }
    }

    /// <inheritdoc/>
    public override int ReadByte()
    {
        byte b = default;
        var read = Read(new Span<byte>(ref b));
        return read == 0 ? -1 : b;
    }

    private string GetString(int length)
    {
        Debug.Assert(Monitor.IsEntered(_sync));
        Debug.Assert(length <= _readBuffer.ActiveLength);

        return _encoding.GetString(
            _readBuffer.DangerousGetUnderlyingBuffer(),
            _readBuffer.ActiveStartOffset,
            length);
    }

    /// <summary>
    /// Writes the specified text to the shell.
    /// </summary>
    /// <param name="text">The text to be written to the shell.</param>
    /// <remarks>
    /// If <paramref name="text"/> is <see langword="null"/>, nothing is written.
    /// Otherwise, <see cref="Flush"/> is called after writing the data to the buffer.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    public void Write(string text)
    {
        if (text is null)
        {
            return;
        }

        Write(_encoding.GetBytes(text));
        Flush();
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);

        Write(buffer.AsSpan(offset, count));
    }

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

        while (!buffer.IsEmpty)
        {
            if (_writeBuffer.AvailableLength == 0)
            {
                Flush();
            }

            var bytesToCopy = Math.Min(buffer.Length, _writeBuffer.AvailableLength);

            Debug.Assert(bytesToCopy > 0);

            buffer.Slice(0, bytesToCopy).CopyTo(_writeBuffer.AvailableSpan);

            _writeBuffer.Commit(bytesToCopy);

            buffer = buffer.Slice(bytesToCopy);
        }

        if (_autoFlush)
        {
            Flush();
        }
    }

    /// <inheritdoc/>
    public override void WriteByte(byte value)
    {
        Write([value]);
    }

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBufferArguments(buffer, offset, count);

        return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowObjectDisposedIf(_disposed, this);

        while (!buffer.IsEmpty)
        {
            if (_writeBuffer.AvailableLength == 0)
            {
                await FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            var bytesToCopy = Math.Min(buffer.Length, _writeBuffer.AvailableLength);

            Debug.Assert(bytesToCopy > 0);

            buffer.Slice(0, bytesToCopy).CopyTo(_writeBuffer.AvailableMemory);

            _writeBuffer.Commit(bytesToCopy);

            buffer = buffer.Slice(bytesToCopy);
        }

        if (_autoFlush)
        {
            await FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
    {
        return TaskToAsyncResult.Begin(WriteAsync(buffer, offset, count), callback, state);
    }

    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult)
    {
        TaskToAsyncResult.End(asyncResult);
    }

    /// <summary>
    /// Writes the line to the shell.
    /// </summary>
    /// <param name="line">The line to be written to the shell.</param>
    /// <remarks>
    /// If <paramref name="line"/> is <see langword="null"/>, only the line terminator is written.
    /// <see cref="Flush"/> is called once the data is written.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    public void WriteLine(string line)
    {
        // By default, the terminal driver translates carriage return to line feed on input.
        // See option ICRLF at https://www.man7.org/linux/man-pages/man3/termios.3.html.
        Write(line + (_noTerminal ? "\n" : "\r"));
    }

    /// <summary>
    /// Writes the specified text to the shell and immediately flushes any buffered data
    /// to the channel.
    /// </summary>
    /// <param name="text">The text to be written to the shell.</param>
    /// <remarks>
    /// Equivalent to calling <see cref="Write(string)"/> followed by <see cref="Flush"/>.
    /// If <paramref name="text"/> is <see langword="null"/>, nothing is written, but any
    /// previously buffered data is still flushed.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    public void WriteAndFlush(string text)
    {
        Write(text);
        Flush();
    }

    /// <summary>
    /// Writes a sequence of bytes to the shell and immediately flushes the buffered data
    /// to the channel.
    /// </summary>
    /// <param name="buffer">An array of bytes. <paramref name="count"/> bytes are copied from <paramref name="buffer"/> to the shell.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the shell.</param>
    /// <param name="count">The number of bytes to be written to the shell.</param>
    /// <remarks>
    /// Equivalent to calling <see cref="Write(byte[], int, int)"/> followed by <see cref="Flush"/>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
    public void WriteAndFlush(byte[] buffer, int offset, int count)
    {
        Write(buffer, offset, count);
        Flush();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing);
            return;
        }

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // Do not dispose _session (we don't own it)
            _session.Disconnected -= Session_Disconnected;
            _session.ErrorOccured -= Session_ErrorOccurred;

            // But we do own _channel
            _channel.DataReceived -= Channel_DataReceived;
            _channel.Closed -= Channel_Closed;
            _channel.Dispose();

            Monitor.PulseAll(_sync);
        }

        base.Dispose(disposing);
    }

    private void Session_ErrorOccurred(object sender, ExceptionEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

    private void Session_Disconnected(object sender, EventArgs e)
    {
        Dispose();
    }

    private void Channel_Closed(object sender, ChannelEventArgs e)
    {
        Dispose();

        if (Closed != null)
        {
            // Handle event on different thread
            ThreadAbstraction.ExecuteThread(() => Closed?.Invoke(this, EventArgs.Empty));
        }
    }

    private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
    {
        lock (_sync)
        {
            if (!_disableReadBuffering)
            {
                _readBuffer.EnsureAvailableSpace(e.Data.Count);

                e.Data.AsSpan().CopyTo(_readBuffer.AvailableSpan);

                _readBuffer.Commit(e.Data.Count);

                Monitor.PulseAll(_sync);
            }
        }

        DataReceived?.Invoke(this, new ShellDataEventArgs(e.Data.ToArray()));
    }
}
