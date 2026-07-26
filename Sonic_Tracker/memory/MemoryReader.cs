using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sonic_Tracker.Memory;

public sealed class MemoryReader : IDisposable
{
    private const uint ProcessVmRead = 0x0010;
    private const uint ProcessQueryInformation = 0x0400;

    private readonly nint _processHandle;
    private readonly nint _moduleBaseAddress;
    private bool _disposed;

    public int ProcessId { get; }

    public MemoryReader(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        ProcessId = process.Id;

        _moduleBaseAddress =
            process.MainModule?.BaseAddress
            ?? throw new InvalidOperationException(
                "Unable to determine the Sonic Origins module base address.");

        _processHandle = OpenProcess(
            ProcessVmRead | ProcessQueryInformation,
            false,
            process.Id);

        if (_processHandle == nint.Zero)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "Unable to open the Sonic Origins process.");
        }
    }

    public int ReadInt32(nint address)
    {
        byte[] buffer = ReadBytes(address, sizeof(int));
        return BitConverter.ToInt32(buffer, 0);
    }

    public nint ReadPointer(nint address)
    {
        byte[] buffer = ReadBytes(address, sizeof(long));
        return new nint(BitConverter.ToInt64(buffer, 0));
    }

    public int ReadInt32AtModuleOffset(
        nint offset) =>
        ReadInt32(
            ResolveModuleOffset(offset));

    public short ReadInt16AtModuleOffset(
        nint offset) =>
        ReadInt16(
            ResolveModuleOffset(offset));

    public nint ResolveModuleOffset(
        nint offset)
    {
        if (offset < nint.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset));
        }

        return _moduleBaseAddress + offset;
    }

    public nint ResolveModuleOffset(
        string moduleName,
        nint offset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            moduleName);

        if (offset < nint.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset));
        }

        using Process process =
            Process.GetProcessById(ProcessId);

        ProcessModule? module =
            process.Modules
                .Cast<ProcessModule>()
                .FirstOrDefault(candidate =>
                    string.Equals(
                        candidate.ModuleName,
                        moduleName,
                        StringComparison.OrdinalIgnoreCase));

        if (module is null)
        {
            throw new InvalidOperationException(
                $"Unable to find {moduleName} in the Sonic Origins process.");
        }

        return module.BaseAddress + offset;
    }

    public short ReadInt16(nint address)
    {
        byte[] buffer = ReadBytes(address, sizeof(short));
        return BitConverter.ToInt16(buffer, 0);
    }

    public byte ReadByte(nint address)
    {
        byte[] buffer = ReadBytes(address, sizeof(byte));
        return buffer[0];
    }

    public byte[] ReadBytes(nint address, int size)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MemoryReader));
        }

        if (address == nint.Zero)
        {
            throw new ArgumentException(
                "The memory address cannot be zero.",
                nameof(address));
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        byte[] buffer = new byte[size];

        bool success = ReadProcessMemory(
            _processHandle,
            address,
            buffer,
            buffer.Length,
            out nint bytesRead);

        if (!success)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"Unable to read memory at 0x{address.ToInt64():X}.");
        }

        if (bytesRead.ToInt64() != size)
        {
            throw new InvalidOperationException(
                $"Expected {size} bytes but only read {bytesRead} bytes.");
        }

        return buffer;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_processHandle != nint.Zero)
        {
            CloseHandle(_processHandle);
        }

        _disposed = true;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(
        uint desiredAccess,
        bool inheritHandle,
        int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadProcessMemory(
        nint processHandle,
        nint baseAddress,
        [Out] byte[] buffer,
        int size,
        out nint numberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
}
